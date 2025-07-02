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
using Terraria.Localization;
using Terraria.Chat;

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

        On_PlayerInput.SetZoom_UI += On_PlayerInput_SetZoom_UI;
    }

    private void On_PlayerInput_SetZoom_UI(On_PlayerInput.orig_SetZoom_UI orig)
    {
        var cond = Main.inFancyUI && WindowUISystem.IsActive();
        if (cond)
        {
            var oldScale = Main.UIScale;
            Main.UIScale = 1f;

            orig();

            Main.UIScale = oldScale;
        }
        else orig();
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

        var rect = texture.Frame(4, 3, type, Tier, -3, -3);
        var scale = Scale;

        Vector2 origin = rect.Size() * 0.5f;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2);

        spriteBatch.Draw(texture, center, rect, Color.White, 0, origin, scale, SpriteEffects.None, 0);


        if (Flash != 0 || !Open)
        {
            texture = TextureAssets.MagicPixel.Value;

            var PixelScale = Scale * new Vector2(Width.Pixels, Height.Pixels) / new Vector2(texture.Width, texture.Height);

            var color = Flash > 0 ? Color.White : Color.DarkGray;
            color *= float.Abs(Flash);

            spriteBatch.Draw(texture, center, null, color with { A = 150 }, 0, texture.Size() * 0.5f, PixelScale, SpriteEffects.None, 0);
        }

        if (Scale != 1)
        {
            var learn = Language.GetTextValue("Mods.MordhauProgression.Traits.Learn");
            var reset = Language.GetTextValue("Mods.MordhauProgression.Traits.Reset");
            var chance = Language.GetTextValue("Mods.MordhauProgression.Traits.T2Chance");
            var bonus = Language.GetTextValue("Mods.MordhauProgression.Traits.T2Bonus");

            var effect = Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index}.T{Math.Max(Tier, 1)}");
            if (Tier == 1)
            {
                effect += "\n";
                effect += effect.Contains(chance[chance.IndexOf(' ')..]) ? chance : bonus;
            }

            var open = Tier == 2 ? reset : learn + "\n" + reset;
            if (!Open)
                open = string.Format(Language.GetTextValue("Mods.MordhauProgression.Traits.Required"), Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index - 1}.Name"));


            var text = $"\n{effect}\n\n{open}";

            texture = Textures.Window.Value;
            var font = FontAssets.MouseText.Value;

            var textSize = ChatManager.GetStringSize(font, text, new Vector2(1f), 160);
            var textOffset = new Vector2(10);

            var color = Color.DarkSlateGray;
            color *= (scale - 0.8f);

            var desiredSize = textOffset * 2 + textSize;
            var WindowScale = desiredSize / texture.Size();

            scale = 1 + 10f / 55f;

            var pos = data.row == 0 ? center + new Vector2(-Width.Pixels / 2 * scale - desiredSize.X, -Height.Pixels / 2 * scale) : center + new Vector2(Width.Pixels / 2 * scale, -Height.Pixels / 2 * scale);

            spriteBatch.Draw(texture, pos, null, color, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);


            color = Tier == 2 ? Color.Gold : Tier == 1 ? Color.AntiqueWhite : Color.WhiteSmoke;
            color *= Open ? (scale - 0.5f) : (scale - 0.9f);

            ChatManager.DrawColorCodedString(spriteBatch, font, id.name, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);

            color = !Open ? Color.Gray : Tier != 0 ? Color.Gold : Color.WhiteSmoke;
            color *= Open ? (scale - 0.5f) : (scale - 0.9f);

            var textHeight = ChatManager.GetStringSize(font, text, new Vector2(1f), 160).Y;

            if (Tier == 1)
            {
                var goldText = effect.Contains(chance) ? chance : bonus;

                goldText = '\n' + goldText;
                while (ChatManager.GetStringSize(font, text[..text.IndexOf(goldText[(goldText.LastIndexOf("\n") + 1)..])], new Vector2(1f), 160).Y > ChatManager.GetStringSize(font, goldText, new Vector2(1f), 160).Y)
                {
                    goldText = '\n' + goldText;
                }
                goldText = Tier != 1 ? goldText : '\n' + goldText;

                ChatManager.DrawColorCodedString(spriteBatch, font, goldText, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);

                text = Tier != 1 ? text.Replace(effect, "") : effect.Contains(chance) ? text.Replace(chance, "") : text.Replace(bonus, "");
            }

            while (ChatManager.GetStringSize(font, text, new Vector2(1f), 160).Y < textHeight)
            {
                text = "\n" + text;
            }

            if (!Open)
            {
                effect = effect.Contains("\n") ? effect[..effect.IndexOf("\n")] : effect;

                ChatManager.DrawColorCodedString(spriteBatch, font, "\n" + effect, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);

                text = text.Replace(effect, "");
            }

            while (ChatManager.GetStringSize(font, text, new Vector2(1f), 160).Y < textHeight)
            {
                text = "\n" + text;
            }

            color = Color.WhiteSmoke;
            color *= Open ? (scale - 0.5f) : (scale - 0.9f);


            ChatManager.DrawColorCodedString(spriteBatch, font, text, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);
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
            Scale = Clamp(Scale + 0.015f, 1, 1 + 10f / 55f);
        }

        else Scale = Clamp(Scale - 0.015f, 1, 1 + 10f / 55f);


        var sub = "Warrior";
        var first = data.row == 0;

        switch (data.role)
        {
            case "Melee":
                if (!first)
                    sub = "Tank";
                break;

            case "Ranger":
                if (first)
                    sub = "Sharpshooter";
                else sub = "Archer";
                break;

            case "Mage":
                if (first)
                    sub = "Sorcerer";
                else sub = "Wizard";
                break;

            default:
                if (first)
                    sub = "Commander";
                else sub = "Defender"; 
                break;
        }


        id.name = Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{sub}.{data.index}.Name");
        id.subRole = sub;
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

                button.Width.Set(66, 0);
                button.Height.Set(66, 0);
                button.MinWidth.Set(66, 0);
                button.MaxWidth.Set(66, 0);
                button.MaxHeight.Set(66, 0);
                button.MinHeight.Set(66, 0);

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
                            role = "Melee";
                        else
                            role = "Mage";

                        break;
                    case < 4:
                        if (float.Sign(i) < 0)
                            role = "Ranger";
                        else
                            role = "Summoner";
                        break;
                }

                TraitButtonUISystem.TraitRegistry.Add((role, row, index, button));
                button.SetTraitData(role, row, index);

                button.Activate();
            }
        }
    }
}