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
using System.Linq;

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
            UIPlayer.CurrentTooltipUI = () =>
            {
                var learn = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Learn");
                var reset = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Reset");

                var effect = Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index}.T{Math.Max(Tier, 1)}");

                var f = effect.FirstOrDefault(e => char.IsNumber(e), ' ');
                var l = char.IsNumber(effect[effect.IndexOf(f) + 1]) ? effect[effect.IndexOf(f) + 1] : effect[effect.IndexOf(f)];
                var number = f == ' ' ? "1" : effect[effect.IndexOf(f)..(effect.IndexOf(l) + 1)];
                var bonus = string.Format(Language.GetTextValue("Mods.MordhauProgression.Tooltips.T2Bonus"), number);
                if (effect.Contains('%'))
                    bonus = bonus[..bonus.IndexOf(' ')] + '%' + bonus[bonus.IndexOf(' ')..];
                var chance = string.Format(Language.GetTextValue("Mods.MordhauProgression.Tooltips.T2Chance"), number + "%");

                var next = effect.Contains(chance) ? chance : bonus;

                if (Tier != 0)
                {
                    if (Tier != 1)
                        next = "";
                    else
                        next = "\n" + next;

                    effect = $"{effect}{next}";
                }

                var open = Tier == 2 ? reset : learn + "\n" + reset;
                if (!Open)
                    open = string.Format(Language.GetTextValue("Mods.MordhauProgression.Tooltips.Required"), Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index - 1}.Name"));


                var scale = 1 + 12f / 66f;

                var texture = Textures.Window.Value;
                var font = FontAssets.MouseText.Value;


                var sb = Main.spriteBatch;


                var text = $"{id.name}\n{effect}\n\n{open}";

                var textSize = ChatManager.GetStringSize(font, text, new Vector2(1f), 160);
                var textOffset = new Vector2(15);

                text = text.Replace(id.name, "");


                var desiredSize = textOffset * 2 + textSize;

                var color = Color.DarkSlateGray;
                color *= (scale - 0.7f);

                var WindowScale = desiredSize / texture.Size();

                scale = 1 + 12f / 66f;

                var pos = Main.MouseScreen;

                sb.Draw(texture, pos, null, color, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);


                color = Tier == 3 ? Color.Plum : Tier == 2 ? Color.Khaki.MultiplyRGB(Color.Khaki) : Tier == 1 ? Color.AntiqueWhite : Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, id.name, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);

                if (Tier == 1)
                {
                    while (ChatManager.GetStringSize(font, next, new Vector2(1f), 160).Y <= ChatManager.GetStringSize(font, text[..text.LastIndexOf(next.Replace("\n", ""))], new Vector2(1f), 160).Y)
                    {
                        next = "\n" + next;
                    }

                    color = Tier == 2 ? Color.Plum : Tier == 1 ? Color.Khaki.MultiplyRGB(Color.Khaki) : Color.AntiqueWhite;

                    ChatManager.DrawColorCodedString(sb, font, next, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);
                    text = text[..text.LastIndexOf(next.Replace("\n", ""))] + text[(text.LastIndexOf(next.Replace("\n", "")) + next.Replace("\n", "").Length)..];
                }


                color = Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, text, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);
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