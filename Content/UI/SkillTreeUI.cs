using Microsoft.Xna.Framework;
using MordhauProgression.Content.UI;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using Terraria.GameContent;
using System;
using Terraria.UI.Chat;
using Terraria.Localization;

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
        HideResourceBarsSystem.NameList.Add(LayerName);

        state = new TraitButtonUIState();
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
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2);

        var texture = Textures.Icons.Value;

        var rect = texture.Frame(4, 3, type, Tier, -3, -3);
        var scale = Scale;

        Vector2 origin = rect.Size() * 0.5f;

        spriteBatch.Draw(texture, center, rect, Color.White, 0, origin, scale, SpriteEffects.None, 0);


        if (Flash != 0 || !Open)
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

    public bool Open = false;

    public (string role, int row, int index) data = new();

    public (string subRole, string name) id = new();

    public int ActualRow = 0;
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


        if (ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
            Scale = Clamp(Scale + 0.015f, 1, 1 + 12f / 66f);
        }

        else Scale = Clamp(Scale - 0.015f, 1, 1 + 12f / 66f);


        var sub = "Warrior";
        var first = data.row == 0;

        switch (data.role)
        {
            case "Melee":
                if (!first)
                {
                    ActualRow = 1;
                    sub = "Tank";
                }
                break;

            case "Ranger":
                if (first)
                {
                    ActualRow = 2;
                    sub = "Sharpshooter";
                }
                else
                {
                    ActualRow = 3;
                    sub = "Archer";
                }
                break;

            case "Mage":
                if (first)
                {
                    ActualRow = 4;
                    sub = "Sorcerer";
                }
                else
                {
                    ActualRow = 5;
                    sub = "Wizard";
                }
                break;

            default:
                if (first)
                {
                    ActualRow = 6;
                    sub = "Commander";
                }
                else
                {
                    ActualRow = 7;
                    sub = "Defender";
                }
                break;
        }


        id.name = Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{sub}.{data.index}.Name");
        id.subRole = sub;

        if (Scale != 1)
        {
            RoleUISystem.HoveredRow = ActualRow;
            var learn = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Learn");
            var reset = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Reset");
            var cur = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Current");

            var effect = Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index}.T{Math.Max(Tier, 1)}");
            if (Tier != 0)
            {
                effect = $"{cur}\n{effect}";
            }

            var open = Tier == 2 ? reset : learn + "\n" + reset;
            if (!Open)
                open = string.Format(Language.GetTextValue("Mods.MordhauProgression.Tooltips.Required"), Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index - 1}.Name"));


            var text = $"{id.name}\n{effect}\n\n{open}";
            var scale = 1 + 12f / 66f;

            var texture = Textures.Window.Value;
            var font = FontAssets.MouseText.Value;

            var textSize = ChatManager.GetStringSize(font, text, new Vector2(1f), 160);
            var textOffset = new Vector2(15);

            text = text.Replace(id.name, "");
            text = text.Replace(cur, "");

            var color = Color.DarkSlateGray;
            color *= (scale - 0.7f);

            var desiredSize = textOffset * 2 + textSize;

            var sb = Main.spriteBatch;


            UIPlayer.CurrentTooltipUI = () =>
            {
                var WindowScale = desiredSize / texture.Size();

                scale = 1 + 12f / 66f;

                var pos = Main.MouseScreen;

                sb.Draw(texture, pos, null, color, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);

                color = Color.WhiteSmoke;
                color *= scale - 0.8f;

                if (Tier != 0)
                ChatManager.DrawColorCodedString(sb, font, "\n" + cur, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);


                color = Tier == 3 ? Color.Plum : Tier == 2 ? Color.Khaki.MultiplyRGB(Color.Khaki) : Tier == 1 ? Color.AntiqueWhite : Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, id.name, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);


                color = Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, text, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);

                if (Tier == 1)
                {
                    var name = Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index}.Name");
                    text = name + "\n" + Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index}.T{Tier + 1}");
                    textSize = ChatManager.GetStringSize(font, text, new Vector2(1f), 140);
                    text = text.Replace(name, "");
                    desiredSize = textOffset * 2 + textSize;
                    WindowScale = desiredSize / texture.Size();

                    pos = new Vector2(Left.Pixels - desiredSize.X - (scale - 1) * Width.Pixels / 2, Top.Pixels - (scale - 1) * Height.Pixels / 2);

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
        var screenHalved = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 2, Main.instance.GraphicsDevice.Viewport.Height / 2 - 40 - 100);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                TraitButtonUIElement button = new();
                button.SetPadding(0);

                var pos = new Vector2(x * 140 - 10 * (x % 2) - 33 - 485, y * 100);
                pos += screenHalved;

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


                var row = x % 2;
                var index = y;
                var role = x switch
                {
                    < 2 => "Melee",
                    < 4 => "Ranger",
                    < 6 => "Mage",
                    _ => "Summoner",
                };
                TraitButtonUISystem.TraitRegistry.Add((role, row, index, button));
                button.SetTraitData(role, row, index);

                button.Activate();
            }
        }
    }
}