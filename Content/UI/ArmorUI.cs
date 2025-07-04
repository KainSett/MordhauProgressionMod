using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.UI;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class ArmorUISystem : ModSystem
{
    internal ArmorUIState state;
    private UserInterface Interface;

    public static HashSet<ArmorUIElement> ArmorRegistry = [];

    public void Show()
    {
        Interface?.SetState(state);

        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
        {
            p.Armor?.Clear();

            foreach (var part in ArmorRegistry)
            {
                p.Armor?.Add(part);
            }
        }
    }

    public void Hide()
    {
        Interface?.SetState(null);
    }

    public override void Load()
    {
        HideResourceBarsSystem.NameList.Add(LayerName);

        state = new ArmorUIState();
        Interface = new UserInterface();
        state.Activate();
    }


    public bool IsActive()
    {
        if (Interface.CurrentState == null)
            return false;

        return true;
    }

    public override void UpdateUI(GameTime gameTime) => Interface?.Update(gameTime);

    public static string LayerName = "MordhauProgression: Armor";

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

        layers.Insert(index + 2, l);

        HideResourceBarsSystem.UILayers = layers;
    }
}

public class ArmorUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {

        Texture2D texture = Textures.Armor.Value;

        var rect = texture.Frame(3, 4, type, Tier, -3, -3);
        var scale = Scale;

        Vector2 origin = rect.Size() * 0.5f;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2);

        spriteBatch.Draw(texture, center, rect, Color.White, 0, origin, scale, SpriteEffects.None, 0);


        if (Flash != 0)
        {
            texture = TextureAssets.MagicPixel.Value;

            var PixelScale = Scale * new Vector2(Width.Pixels, Height.Pixels) / new Vector2(texture.Width, texture.Height);

            var color = Flash > 0 ? Color.White : Color.DarkGray;
            color *= float.Abs(Flash);

            spriteBatch.Draw(texture, center, null, color with { A = 150 }, 0, texture.Size() * 0.5f, PixelScale, SpriteEffects.None, 0);
        }
    }

    #region Fields
    public int Tier = 0;

    public int type = 0;

    public float Scale = 1f;

    public float Flash = 0;
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
        var newTier = (int)Clamp(Tier + 1, 0, 3);

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


        if (ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
            Scale = Clamp(Scale + 0.015f, 1, 1 + 12f / 66f);
        }

        else Scale = Clamp(Scale - 0.015f, 1, 1 + 12f / 66f);


        if (Scale != 1)
        {
            var learn = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Learn");
            var reset = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Reset");
            var cur = "\n" + Language.GetTextValue("Mods.MordhauProgression.Tooltips.Current");
            var effect = Language.GetTextValue($"Mods.MordhauProgression.Armor.T{Math.Min(Tier, 3)}.Effect");
            var name = Language.GetTextValue($"Mods.MordhauProgression.Armor.T{Math.Min(Tier, 3)}.Prefix") + " " + Language.GetTextValue($"Mods.MordhauProgression.Armor.{type}");

            var open = Tier == 3 ? reset : learn + "\n" + reset;
            var text = $"{name}{cur}\n{effect}\n\n{open}";
            var scale = 1 + 12f / 66f;

            var color = Color.DarkSlateGray;
            color *= scale - 0.7f;

            var texture = Textures.Window.Value;
            var font = FontAssets.MouseText.Value;

            var textSize = ChatManager.GetStringSize(font, text, new Vector2(1f), 160);
            var textOffset = new Vector2(15);
            text = text.Replace(name, "");
            text = text.Replace(cur, "\n");

            var desiredSize = textOffset * 2 + textSize;

            var center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2);
            var sb = Main.spriteBatch;


            UIPlayer.CurrentTooltipUI = () =>
            {
                var WindowScale = desiredSize / texture.Size();

                scale = 1 + 12f / 66f;

                var pos = Main.MouseScreen;

                sb.Draw(texture, pos, null, color, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);

                color = Color.WhiteSmoke;
                color *= scale - 0.8f;

                ChatManager.DrawColorCodedString(sb, font, cur, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);


                color = Tier == 3 ? Color.Plum : Tier == 2 ? Color.Khaki.MultiplyRGB(Color.Khaki) : Tier == 1 ? Color.AntiqueWhite : Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, name, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);


                color = Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, text, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);

                if (Tier != 3)
                {

                    name = Language.GetTextValue($"Mods.MordhauProgression.Armor.T{Tier + 1}.Prefix") + " " + Language.GetTextValue($"Mods.MordhauProgression.Armor.{type}");
                    text = name + "\n" + Language.GetTextValue($"Mods.MordhauProgression.Armor.T{Tier + 1}.Effect");
                    textSize = ChatManager.GetStringSize(font, text, new Vector2(1f), 140);
                    text = text.Replace(name, "");
                    desiredSize = textOffset * 2 + textSize;
                    WindowScale = desiredSize / texture.Size();

                    pos = new Vector2(Left.Pixels-desiredSize.X - (scale - 1) * Width.Pixels / 2, Top.Pixels - (scale - 1) * Height.Pixels / 2);

                    color = Color.DarkSlateGray;
                    color *= scale - 0.7f;

                    sb.Draw(texture, pos, null, color, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);


                    color = Tier == 2 ? Color.Plum : Tier == 1 ? Color.Khaki.MultiplyRGB(Color.Khaki) : Tier == 0 ? Color.AntiqueWhite : Color.WhiteSmoke;
                    color *= scale - 0.5f;

                    ChatManager.DrawColorCodedString(sb, font, name, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);

                    color = Color.WhiteSmoke;
                    color *= scale - 0.5f;

                    ChatManager.DrawColorCodedString(sb, font, text, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);
                }
            };
        }
    }
}

public class ArmorUIState : UIState
{
    public override void OnInitialize()
    {
        var pos = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 1.2f, Main.instance.GraphicsDevice.Viewport.Height / 2 - 40 - 150);

        for (int y = 0; y < 3; y++)
        {
            ArmorUIElement button = new();
            button.SetPadding(0);

            pos += new Vector2(0, 100);

            button.Left.Set(pos.X, 0f);
            button.Top.Set(pos.Y, 0f);

            button.Width.Set(66, 0);
            button.Height.Set(66, 0);
            button.MinWidth.Set(66, 0);
            button.MaxWidth.Set(66, 0);
            button.MaxHeight.Set(66, 0);
            button.MinHeight.Set(66, 0);

            button.type = y;
            button.Tier = 0;

            Append(button);


            ArmorUISystem.ArmorRegistry.Add(button);

            button.Activate();
        }
    }
}