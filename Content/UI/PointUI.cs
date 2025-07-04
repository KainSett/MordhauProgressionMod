using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.UI;
using Terraria;
using System.Linq;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class PointUISystem : ModSystem
{
    internal PointUIState state;
    private UserInterface Interface;
    public static List<Vector2> Position = [];

    public void Show()
    {
        Interface?.SetState(state);
    }

    public void Hide()
    {
        Interface?.SetState(null);
    }

    public override void Load()
    {
        state = new PointUIState();
        Interface = new UserInterface();
        state.Activate();

        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
    }

    private void On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
    {
        if (inv == Main.LocalPlayer.armor && slot < 3)
        {
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

    public static string LayerName = "MordhauProgression: Point";

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

public class PointUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (PointUISystem.Position.Count <= type || PointUISystem.Position[type] == Vector2.Zero)
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

    public override void Update(GameTime gameTime)
    {
        if (PointUISystem.Position.Count <= type || PointUISystem.Position[type] == Vector2.Zero)
            return;

        Left.Set(PointUISystem.Position[type].X + 40 * Main.UIScale - Width.Pixels / 2, 0);
        Top.Set(PointUISystem.Position[type].Y + 2 * Main.UIScale - Height.Pixels / 2, 0);
        PointUISystem.Position[type] = Vector2.Zero;


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

        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
        {
            if (p.Armor.Any(e => e.type == type))
                frame.row = p.Armor.First(e => e.type == type).Tier;
        }
    }
}

public class PointUIState : UIState
{
    public override void OnInitialize()
    {
        var pos = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 1.2f, Main.instance.GraphicsDevice.Viewport.Height / 2 - 40 - 300);

        for (int i = 0; i < 3; i++)
        {
            PointUIElement element = new();

            element.Left.Set(pos.X, 0f);
            element.Top.Set(pos.Y, 0f);

            element.Width.Set(20, 0);
            element.Height.Set(20, 0);
            element.MinWidth.Set(20, 0);
            element.MaxWidth.Set(20, 0);
            element.MaxHeight.Set(20, 0);
            element.MinHeight.Set(20, 0);

            element.type = i;

            Append(element);
            element.Activate();
        }
    }
}