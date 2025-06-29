using Microsoft.Xna.Framework;
using MordhauProgression.Common.Config;
using MordhauProgression.Content.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using MordhauProgression.Common.Assets;
using Terraria.GameInput;
using Terraria.UI.Gamepad;
using System.Collections.Generic;
using Terraria.GameContent;
using System.Linq;
using System;
using System.Data;
using System.Reflection;
using Terraria.UI.Chat;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class TraitButtonUISystem : ModSystem
{
    internal TraitButtonUIState state;
    private UserInterface Interface;

    public static HashSet<(string role, int row, int index, TraitButtonUIElement trait)> TraitRegistry = [];

    public void Show()
    {
        Interface?.SetState(state);

        foreach (var trait in TraitRegistry)
        {
            if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
                p.SkillTree?.Add(trait);
        } 
    }

    public void Hide()
    { 
        Interface?.SetState(null); 
        
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
            p.SkillTree?.Clear();
    }

    public override void Load()
    {
        state = new TraitButtonUIState();
        Interface = new UserInterface();
        state.Activate();

        On_UserInterface.GetMousePosition += On_UserInterface_GetMousePosition;
    }

    private void On_UserInterface_GetMousePosition(On_UserInterface.orig_GetMousePosition orig, UserInterface self)
    {
        var cond = self.CurrentState is TraitButtonUIState;
        if (cond)
        {
            Main.mouseX = (int)(Main.mouseX * Main.UIScale);
            Main.mouseY = (int)(Main.mouseY * Main.UIScale);
        }

        orig(self);

        if (cond)
        {
            Main.mouseX = (int)(Main.mouseX / Main.UIScale);
            Main.mouseY = (int)(Main.mouseY / Main.UIScale);
        }
    }

    public bool IsActive()
    {
        if (Interface.CurrentState == null)
            return false;

        return true;
    }

    public override void UpdateUI(GameTime gameTime) => Interface?.Update(gameTime);

    public static string LayerName = "MordhauProgression: Trait Button";

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ingame Options"));
        if (index == -1)
            return;

        var l = new LegacyGameInterfaceLayer(LayerName, delegate
        {
            Interface.Draw(Main.spriteBatch, new GameTime());

            return true;

        }, InterfaceScaleType.None);

        layers.Insert(index + 3, l);

        HideResourceBarsSystem.UILayers = layers;
    }
}

public class TraitButtonUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Textures.Icons.Value;

        Vector2 origin = texture.Size() * 0;

        var rect = texture.Frame(4, 3, type, Tier, -2, -2);
        var scale = Scale * Height.Pixels / 32;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var pos = new Vector2(Left.Pixels, Top.Pixels);

        var scaleOffset = (rect.Size() + new Vector2(6)) * (Scale - 1) / 2;

        spriteBatch.Draw(texture, pos - scaleOffset, rect, Color.White, 0, origin, scale, SpriteEffects.None, 0);

        if (Flash != 0 || !Open)
        {
            texture = TextureAssets.MagicPixel.Value;

            var PixelScale = Scale * new Vector2(Width.Pixels, Height.Pixels) / new Vector2(texture.Width, texture.Height);

            var color = Flash > 0 ? Color.White : Color.DarkGray;
            color *= float.Abs(Flash);

            spriteBatch.Draw(texture, pos - scaleOffset, null, color with { A = 150 }, 0, origin, PixelScale, SpriteEffects.None, 0);
        }

        if (Scale != 1)
        {
            texture = Textures.Window.Value;
            var font = FontAssets.MouseText.Value;

            var offset = data.row == 1 ? new Vector2(Width.Pixels + scaleOffset.X * 2, -scaleOffset.Y)
                : new Vector2(Width.Pixels - scaleOffset.X / 2, -scaleOffset.Y);
            var textOffset = new Vector2(10);

            origin = data.row == 1 ? Vector2.Zero : new Vector2(180f + Width.Pixels + scaleOffset.X, 0);

            var color = Color.DarkSlateGray;
            color *= (Scale - 0.7f);

            var WindowScale = (new Vector2(20, 20) + ChatManager.GetStringSize(font, $"{data.role}, {data.index}", new Vector2(1f), 180)) / texture.Size();

            spriteBatch.Draw(texture, pos + offset - origin, null, color, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);

            color = Color.WhiteSmoke;
            color *= (Scale - 0.7f);

            ChatManager.DrawColorCodedString(spriteBatch, font, $"{data.role}, {data.index}", pos + offset + textOffset, color, 0, origin, new Vector2(1f), 180);
        }
    }

    #region Fields
    public int Tier = 0;

    public int type = 0;

    public float Scale = 1f;

    public float Flash = 0;

    public bool Open = false;

    public (string role, int row, int index) data = new();
    #endregion

    #region Events
    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);
        if (evt.Target == this)
            OnLeftInteract();
    }

    public override void RightClick(UIMouseEvent evt)
    {
        if (evt.Target == this)
            OnRightInteract();
    }

    private void OnRightInteract()
    {
        if (Tier != 0)
            Flash = -1f;

        Tier = 0;
    }

    private void OnLeftInteract()
    {
        if (!Open)
            return;

        var newTier = (int)Clamp(Tier + 1, 0, 2);

        if (newTier != Tier)
            Flash = 1f;

        Tier = newTier;
    }
    #endregion

    public override void Update(GameTime gameTime)
    {
        if (Flash > 0)
            Flash = Clamp(Flash - 0.05f, 0, Flash);

        else if (Flash < 0)
            Flash = Clamp(Flash + 0.05f, Flash, 0);


        if (ContainsPoint(Main.MouseScreen * Main.UIScale))
        {
            Main.LocalPlayer.mouseInterface = true;
            Scale = Clamp(Scale + 0.015f, 1, 1.2f);
        }

        else Scale = Clamp(Scale - 0.015f, 1, 1.2f);
    }

    public void SetTraitData(string role, int row, int index)
    {
        data.role = role;
        data.row = row;
        data.index = index;
    }
}

public class TraitButtonUIState : UIState
{
    public override void OnInitialize()
    {
        var screenHalved = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 2, Main.instance.GraphicsDevice.Viewport.Height / 2);

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                TraitButtonUIElement button = new();
                button.SetPadding(0);

                var i = y - 3.5f;
                i += float.Sign(i) * 0.5f;

                var pos = new Vector2(x * 200 - 50 * (x % 2) - 325, i * 80);
                pos += screenHalved;

                button.Left.Set(pos.X, 0f);
                button.Top.Set(pos.Y, 0f);

                button.Width.Set(48, 0);
                button.Height.Set(48, 0);
                button.MinWidth.Set(48, 0);
                button.MaxWidth.Set(48, 0);
                button.MaxHeight.Set(48, 0);
                button.MinHeight.Set(48, 0);

                button.type = (y + x + (float.Sign(i) + 1) / 2) % 4;
                button.Tier = 0;

                Append(button);


                var row = x % 2;
                var index = 4 - (int)Math.Floor(float.Abs(i));
                var role = "Melee";
                switch (x)
                {
                    case < 2:
                        if (float.Sign(i) < 0)
                            role = "Mewee";
                        else
                            role = "Mag";

                        break;
                    case < 4:
                        if (float.Sign(i) < 0)
                            role = "Rangr";
                        else
                            role = "Summonr";
                        break;
                }

                TraitButtonUISystem.TraitRegistry.Add((role, row, index, button));
                button.SetTraitData(role, row, index);

                button.Activate();
            }
        }
    }
}