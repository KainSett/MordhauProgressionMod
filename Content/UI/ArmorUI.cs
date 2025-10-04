using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MordhauProgression.Common.Assets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class ArmorUISystem : ModSystem
{
    internal ArmorUIState state;
    private UserInterface Interface;

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
        UIDetoursSystem.NameList.Add(LayerName);
        ReInitialize();
    }

    public void ReInitialize()
    {
        state = new ArmorUIState();
        Interface = new UserInterface();
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

        UIDetoursSystem.UILayers = layers;
    }
}

public class ArmorUIItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!item.wornArmor)
            return;


        var type = -1;

        if (item.headSlot != -1) type = 0;
        else if (item.bodySlot != -1) type = 1;
        else if (item.legSlot != -1) type = 2;

        if (type == -1)
            return;

        var prefix = "";
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            var loadout = player.loadouts[Main.LocalPlayer.CurrentLoadoutIndex];
            if (loadout.Any(e => e.type == type))
                prefix = Language.GetTextValue($"Mods.MordhauProgression.Armor.T{Math.Min(loadout.First(e => e.type == type).tier, 3)}.Prefix") + " ";
        }
        

        var tooltip = tooltips.Find(t => t.Name == "ItemName");
        if (tooltip is not null)
            tooltip.Text = prefix + tooltip.Text;
    }
}

public class ArmorUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        var tier = GetArmorTier(type);
        if (tier == -1)
            return;

        Texture2D texture = Textures.Armor.Value;

        var rect = texture.Frame(3, 4, type, tier, -3, -3);
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
        if (GetArmorTier(type) != 0)
            Flash = -1f;

        Main.LocalPlayer.GetModPlayer<UIPlayer>().Points[Main.LocalPlayer.CurrentLoadoutIndex] += GetArmorTier(type);

        SetArmorTier(type, 0);
    }

    private void OnLeftInteract()
    {
        var newTier = (int)Clamp(GetArmorTier(type) + 1, 0, 3);

        if (newTier != GetArmorTier(type))
            Flash = 1f;

        Main.LocalPlayer.GetModPlayer<UIPlayer>().Points[Main.LocalPlayer.CurrentLoadoutIndex]--;

        SetArmorTier(type, newTier);
    }
    #endregion

    public static void SetArmorTier(int type, int tier)
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            var loadout = player.loadouts[Main.LocalPlayer.CurrentLoadoutIndex];
            if (loadout.Any(e => e.type == type))
            {
                var l = loadout.First(e => e.type == type);
                loadout[loadout.IndexOf(l)] = (type, tier);

                player.loadouts[Main.LocalPlayer.CurrentLoadoutIndex] = loadout;
            }
        }
    }

    public static int GetArmorTier(int type)
    {
        var tier = -1;

        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            var loadout = player.loadouts[Main.LocalPlayer.CurrentLoadoutIndex];
            if (loadout.Any(e => e.type == type))
                tier = loadout.First(e => e.type == type).tier;
        }


        return tier;
    }

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
            var tier = GetArmorTier(type);
            if (tier == -1)
                return;

            var learn = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Learn");
            var reset = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Reset");
            var cur = "\n" + Language.GetTextValue("Mods.MordhauProgression.Tooltips.Current");
            var effect = Language.GetTextValue($"Mods.MordhauProgression.Armor.T{Math.Min(tier, 3)}.Effect");
            var name = Language.GetTextValue($"Mods.MordhauProgression.Armor.T{Math.Min(tier, 3)}.Prefix") + " " + Language.GetTextValue($"Mods.MordhauProgression.Armor.{type}");

            var open = tier == 3 ? reset : learn + "\n" + reset;
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

                sb.Draw(texture, pos, null, color with { A = (byte)(color.A * 1.8f) }, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);

                color = Color.WhiteSmoke;
                color *= scale - 0.8f;

                ChatManager.DrawColorCodedString(sb, font, cur, pos + textOffset, color with { A = (byte)(color.A * 1.8f) }, 0, Vector2.Zero, new Vector2(1f), 160);


                color = tier == 3 ? Color.Plum : tier == 2 ? Color.Khaki.MultiplyRGB(Color.Khaki) : tier == 1 ? Color.AntiqueWhite : Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, name, pos + textOffset, color with { A = (byte)(color.A * 1.8f) }, 0, Vector2.Zero, new Vector2(1f), 160);


                color = Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, text, pos + textOffset, color with { A = (byte)(color.A * 1.8f) }, 0, Vector2.Zero, new Vector2(1f), 160);

                if (tier != 3)
                {

                    name = Language.GetTextValue($"Mods.MordhauProgression.Armor.T{tier + 1}.Prefix") + " " + Language.GetTextValue($"Mods.MordhauProgression.Armor.{type}");
                    text = name + "\n" + Language.GetTextValue($"Mods.MordhauProgression.Armor.T{tier + 1}.Effect");
                    textSize = ChatManager.GetStringSize(font, text, new Vector2(1f), 140);
                    text = text.Replace(name, "");
                    desiredSize = textOffset * 2 + textSize;
                    WindowScale = desiredSize / texture.Size();

                    pos = new Vector2(Left.Pixels-desiredSize.X - (scale - 1) * Width.Pixels / 2, Top.Pixels - (scale - 1) * Height.Pixels / 2);

                    color = Color.DarkSlateGray;
                    color *= scale - 0.7f;

                    sb.Draw(texture, pos, null, color with { A = (byte)(color.A * 1.8f) }, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);


                    color = tier == 2 ? Color.Plum : tier == 1 ? Color.Khaki.MultiplyRGB(Color.Khaki) : tier == 0 ? Color.AntiqueWhite : Color.WhiteSmoke;
                    color *= scale - 0.5f;

                    ChatManager.DrawColorCodedString(sb, font, name, pos + textOffset, color with { A = (byte)(color.A * 1.8f) }, 0, Vector2.Zero, new Vector2(1f), 160);

                    color = Color.WhiteSmoke;
                    color *= scale - 0.5f;

                    ChatManager.DrawColorCodedString(sb, font, text, pos + textOffset, color with { A = (byte)(color.A * 1.8f) }, 0, Vector2.Zero, new Vector2(1f), 160);
                }
            };
        }
    }
}

public class ArmorUIState : UIState
{
    public override void OnInitialize()
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            player.loadouts.Clear();
            for (int i = 0; i < 3; i++) {
                var tier = player.ArmorTiersData.Count == 0 || player.ArmorTiersData[0].Count == 0 ? [0, 0, 0] : player.ArmorTiersData[i];

                player.loadouts.Add(i, [(0, tier[0]), (1, tier[1]), (2, tier[2])]);
            } 
        }


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

            Append(button);


            button.Activate();
        }
    }
}