using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using System.Linq;
using Terraria.ID;
using static MordhauProgression.Content.UI.ArmorUIElement;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class ArmorCoinUISystem : ModSystem
{
    internal ArmorCoinUIState state;
    private UserInterface Interface;
    public static List<Vector2> Position = [];

    public void Show()
    {
        Interface?.SetState(state);
    }

    public void Hide()
    {
        Interface?.SetState(null);
        ReInitialize();
    }

    public override void Load()
    {
        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += RegisterArmoSlotsPositions;
        On_ItemSlot.OverrideLeftClick += DontPickupIfHoveredOverArmorCoinUI;
        ReInitialize();
    }

    public void ReInitialize()
    {
        state = new ArmorCoinUIState();
        Interface = new UserInterface();
    }

    public override void Unload()
    {
        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color -= RegisterArmoSlotsPositions;
        On_ItemSlot.OverrideLeftClick -= DontPickupIfHoveredOverArmorCoinUI;
    }

    private bool DontPickupIfHoveredOverArmorCoinUI(On_ItemSlot.orig_OverrideLeftClick orig, Item[] inv, int context, int slot)
    {
        if (ModContent.GetInstance<ArmorCoinUISystem>()?.state.Children.Any(e => e.ContainsPoint(Main.MouseScreen * Main.UIScale)) != true)//!AreaContainsPoint(p, new Vector2(16 * Main.UIScale), Main.MouseScreen))
        {
            return orig(inv, context, slot);
        }
        else return true;
    }

    public static bool AreaContainsPoint(Vector2 pos, Vector2 size, Vector2 point)
    {
        return pos.X < point.X && pos.Y < point.Y && point.X < pos.X + size.X && point.Y < pos.Y + size.Y;
    }

    private void RegisterArmoSlotsPositions(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
    {
        if (inv == Main.LocalPlayer.armor && slot < 3)
        {
            var scale = Main.UIScale;
            Main.UIScale = 1;
            var p = ArmorCoinUIElement.GetCoinPosition(position);
            Main.UIScale = scale;
            if (AreaContainsPoint(p, new Vector2(16 * Main.UIScale), Main.MouseScreen))
            {
                var i = new Item();
                i.TurnToAir();
                /*for (int a = 0; a < inv.Length - 1; a ++)
                {
                    inv[a] = i;
                }*/
                //Main.HoverItem = i;
                Main.hoverItemName = "";
            }

            if (slot == 0)
                Position.Clear();


            Position.Add(position * Main.UIScale);

        }
        orig(spriteBatch, inv, context, slot, position, lightColor);
    }

    public bool IsActive()
    {
        if (Interface.CurrentState == null)
            return false;

        return true;
    }

    public override void UpdateUI(GameTime gameTime) => Interface?.Update(gameTime);

    public static string LayerName = "MordhauProgression: ArmorCoin";

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (index == -1)
            return;

        var l = new LegacyGameInterfaceLayer(LayerName, delegate
        {
            Interface.Draw(Main.spriteBatch, new GameTime());

            return true;

        }, InterfaceScaleType.None);

        layers.Insert(index + 1, l);
    }
}

public class ArmorCoinUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (ArmorCoinUISystem.Position.Count <= type || ArmorCoinUISystem.Position[type] == Vector2.Zero)
            return;

        Texture2D texture = Textures.Coins.Value;
        var rect = texture.Frame(4, 3, frame.row, frame.index, -1, -1);

        Vector2 origin = rect.Size() * 0.5f;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2);

        var effect = SpriteEffects.None;
        if (frame.second)
            effect = SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(texture, center, rect, Color.White, 0, origin, 2 * Main.UIScale, effect, 0);
    }

    #region Fields
    public (int row, int index, bool second) frame = (0, 1, false);

    private int timer = 0;

    public int type = 0;
    #endregion

    public static Vector2 GetCoinPosition(Vector2 ItemSlotPos, float dimension = 16)
    {
        return new Vector2(ItemSlotPos.X + dimension * 2 * Main.UIScale, ItemSlotPos.Y - dimension / 3 * Main.UIScale);
    }

    public override void Update(GameTime gameTime)
    {
        if (ArmorCoinUISystem.Position.Count <= type || ArmorCoinUISystem.Position[type] == Vector2.Zero)
            return;

        var pos = GetCoinPosition(ArmorCoinUISystem.Position[type]);

        Left.Set(pos.X, 0);
        Top.Set(pos.Y, 0);
        ArmorCoinUISystem.Position[type] = Vector2.Zero;

        if (ContainsPoint(Main.MouseScreen * Main.UIScale))
        {
            Main.LocalPlayer.mouseInterface = true;

            timer = (timer + 1) % 6;
            if (timer % 6 != 0)
                return;

            if (frame.index == 2)
            {

                frame.second = false;
            }
            else if (frame.index == 0)
            {
                frame.second = true;
            }

            frame.index += frame.second ? 1 : -1;
        }
        else
            frame = (0, 0, false);

        var tier = GetArmorTier(type);
        if (tier != -1)
            frame.row = tier;
    }
}

public class ArmorCoinUIState : UIState
{
    public override void OnInitialize()
    {
        var pos = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 1.2f, Main.instance.GraphicsDevice.Viewport.Height / 2 - 40 - 300);

        for (int i = 0; i < 3; i++)
        {
            ArmorCoinUIElement element = new();

            element.Left.Set(pos.X, 0f);
            element.Top.Set(pos.Y, 0f);

            element.Width.Set(16, 0);
            element.Height.Set(16, 0);
            element.MinWidth.Set(8, 0);
            element.MaxWidth.Set(32, 0);
            element.MaxHeight.Set(32, 0);
            element.MinHeight.Set(8, 0);

            element.type = i;

            Append(element);
            element.Activate();
        }
    }
}