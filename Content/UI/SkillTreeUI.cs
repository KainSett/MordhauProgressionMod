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
using System.Data;
using System.Reflection;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class TraitButtonUISystem : ModSystem
{
    internal TraitButtonUIState state;
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
        state = new TraitButtonUIState();
        Interface = new UserInterface();
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

        UIDetoursSystem.UILayers = layers;
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

        var rect = texture.Frame(32, 3, type, GetTraitTier(data.role, data.row, data.index), -1, -1);
        var scale = Scale;

        Vector2 origin = rect.Size() * 0.5f;

        spriteBatch.Draw(texture, center, rect, Color.White, 0, origin, scale * 3, SpriteEffects.None, 0);

        var Flash = 0f;
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
        {
            if (p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].Count > 0)
                Flash = p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].FirstOrDefault(e => e.row == data.row && e.index == data.index).flash;

            if (Flash != 0 || !Open)
            {
                texture = TextureAssets.MagicPixel.Value;

                var PixelScale = Scale * new Vector2(Width.Pixels, Height.Pixels) / new Vector2(texture.Width, texture.Height);

                var color = Flash > 0 ? Color.White : Color.DarkGray;
                color *= float.Abs(Flash);

                spriteBatch.Draw(texture, center, null, color with { A = 150 }, 0, texture.Size() * 0.5f, PixelScale, SpriteEffects.None, 0);
            }
        }
    }

    #region Fields
    public int type = 0;

    public float Scale = 1f;

    public bool Open
    {
        get {
            if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
            {
                if (p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].Count > 0)
                {
                    var tree = p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex];
                    var trait = tree.FirstOrDefault(e => e.row == data.row && e.index == data.index);
                    return trait.open;
                }
                else return false;
            }
            else return false;
        }
        set {
            if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
            {
                if (p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].Count > 0)
                {
                    var tree = p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex];
                    var trait = tree.FirstOrDefault(e => e.row == data.row && e.index == data.index);
                    var index = tree.IndexOf(trait);

                    trait.open = value;

                    tree[index] = trait;
                }
            }
        }
    }

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
        if (GetTraitTier(data.role, data.row, data.index) != 0)
        {
            if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
            {
                if (p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].Count > 0)
                {
                    var tree = p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex];
                    var trait = tree.FirstOrDefault(e => e.row == data.row && e.index == data.index);
                    var index = tree.IndexOf(trait);
                    trait.flash = 1;

                    p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex][index] = trait;
                }
            }
        }

        SetTraitTier(data.role, data.row, data.index, 0);
    }

    private void OnLeftInteract()
    {
        if (!Open)
            return;

        var newTier = (int)Clamp(GetTraitTier(data.role, data.row, data.index) + 1, 0, 2);

        if (newTier != GetTraitTier(data.role, data.row, data.index))
        {
            if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
            {
                if (p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].Count > 0)
                {
                    var tree = p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex];
                    var trait = tree.FirstOrDefault(e => e.row == data.row && e.index == data.index);
                    var index = tree.IndexOf(trait);
                    trait.flash = -1;

                    p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex][index] = trait;
                }
            }
        }

        SetTraitTier(data.role, data.row, data.index, newTier);
    }
    #endregion

    public static void SetTraitTier(string role, int row, int index, int tier)
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            var loadout = player.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex];
            if (loadout.Any(e => e.role == role && e.row == row && e.index == index))
            {
                var l = loadout.First(e => e.role == role && e.row == row && e.index == index);
                loadout[loadout.IndexOf(l)] = (role, row, index, tier, l.open, l.flash);

                player.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex] = loadout;
            }
        }
    }

    public static int GetTraitTier(string role, int row, int index)
    {
        var tier = -1;

        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            var loadout = player.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex];
            if (loadout.Any(e => e.role == role && e.row == row && e.index == index))
                tier = loadout.First(e => e.role == role && e.row == row && e.index == index).tier;
        }


        return tier;
    }

    public override void Update(GameTime gameTime)
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var p))
        {
            if (p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].Count > 0)
            {
                var tree = p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex];
                var trait = tree.FirstOrDefault(e => e.row == data.row && e.index == data.index);
                var index = tree.IndexOf(trait);

                if (trait.flash > 0)
                    trait.flash = Clamp(trait.flash - 0.05f, 0, trait.flash);

                else if (trait.flash < 0)
                    trait.flash = Clamp(trait.flash + 0.05f, trait.flash, 0);

                p.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex][index] = trait;
            }
        }


        if (ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
            Scale = Clamp(Scale + 0.015f, 1, 1 + 12f / 66f);
        }

        else Scale = Clamp(Scale - 0.015f, 1, 1 + 12f / 66f);


        var sub = "Warrior";
        var first = data.row % 2 == 0;

        switch (data.role)
        {
            case "Melee":
                if (!first)
                    sub = "Tank";

                break;

            case "Ranger":
                if (first)
                    sub = "Sharpshooter";

                else
                    sub = "Archer";

                break;

            case "Mage":
                if (first)
                    sub = "Sorcerer";

                else
                    sub = "Wizard";

                break;

            default:
                if (first)
                    sub = "Commander";

                else
                    sub = "Defender";

                break;
        }


        id.name = GetName(data.role, data.row, data.index);
        id.subRole = sub;
        var tier = GetTraitTier(data.role, data.row, data.index);

        if (Scale != 1)
        {
            RoleUISystem.HoveredRow = data.row;
            UIPlayer.CurrentTooltipUI = () =>
            {
                var learn = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Learn");
                var reset = Language.GetTextValue("Mods.MordhauProgression.Tooltips.Reset");

                var effect = Language.GetTextValue($"Mods.MordhauProgression.Traits.{data.role}.{id.subRole}.{data.index}.T{Math.Max(tier, 1)}");

                var f = effect.FirstOrDefault(e => char.IsNumber(e), ' ');
                var l = char.IsNumber(effect[effect.IndexOf(f) + 1]) ? effect[effect.IndexOf(f) + 1] : effect[effect.IndexOf(f)];
                var number = f == ' ' ? "1" : effect[effect.IndexOf(f)..(effect.IndexOf(l) + 1)];
                var bonus = string.Format(Language.GetTextValue("Mods.MordhauProgression.Tooltips.T2Bonus"), number);
                if (effect.Contains('%'))
                    bonus = bonus[..bonus.IndexOf(' ')] + '%' + bonus[bonus.IndexOf(' ')..];
                var chance = string.Format(Language.GetTextValue("Mods.MordhauProgression.Tooltips.T2Chance"), number + "%");

                var next = effect.Contains(chance) ? chance : bonus;

                if (tier != 0)
                {
                    if (tier != 1)
                        next = "";
                    else
                        next = "\n" + next;

                    effect = $"{effect}{next}";
                }

                var open = tier == 2 ? reset : learn + "\n" + reset;
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


                color = tier == 3 ? Color.Plum : tier == 2 ? Color.Khaki.MultiplyRGB(Color.Khaki) : tier == 1 ? Color.AntiqueWhite : Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, id.name, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);

                if (tier == 1)
                {
                    while (ChatManager.GetStringSize(font, next, new Vector2(1f), 160).Y <= ChatManager.GetStringSize(font, text[..text.LastIndexOf(next.Replace("\n", ""))], new Vector2(1f), 160).Y)
                    {
                        next = "\n" + next;
                    }

                    color = tier == 2 ? Color.Plum : tier == 1 ? Color.Khaki.MultiplyRGB(Color.Khaki) : Color.AntiqueWhite;

                    ChatManager.DrawColorCodedString(sb, font, next, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);
                    text = text[..text.LastIndexOf(next.Replace("\n", ""))] + text[(text.LastIndexOf(next.Replace("\n", "")) + next.Replace("\n", "").Length)..];
                }


                color = Color.WhiteSmoke;
                color *= scale - 0.4f;

                ChatManager.DrawColorCodedString(sb, font, text, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);
            };
        }
    }

    public static string GetRole(int row)
    {
        var role = row switch
        {
            < 2 => "Melee",
            < 4 => "Ranger",
            < 6 => "Mage",
            _ => "Summoner",
        };

        return role;
    }

    public static string GetName(string role, int row, int index)
    {
        string name = "";

        var sub = "Warrior";
        var first = row % 2 == 0;

        switch (role)
        {
            case "Melee":
                if (!first)
                    sub = "Tank";

                break;

            case "Ranger":
                if (first)
                    sub = "Sharpshooter";

                else
                    sub = "Archer";

                break;

            case "Mage":
                if (first)
                    sub = "Sorcerer";

                else
                    sub = "Wizard";

                break;

            default:
                if (first)
                    sub = "Commander";

                else
                    sub = "Defender";

                break;
        }

        name = Language.GetTextValue($"Mods.MordhauProgression.Traits.{role}.{sub}.{index}.Name");

        return name;
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
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            player.SkillTree.Clear();
            for (int i = 0; i < 3; i++)
                player.SkillTree.Add(i, []);


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

                    button.type = y + x * 4;

                    Append(button);


                    var row = x;
                    var index = y;
                    var role = TraitButtonUIElement.GetRole(row);

                    var tier = player.TraitTiersData.Count == 0 ? [0, 0, 0] : player.TraitTiersData[TraitButtonUIElement.GetName(role, x, y)];

                    for (int a = 0; a < 3; a++)
                        player.SkillTree[a].Add((role, row, index, tier[a], false, 0));

                    button.SetTraitData(role, row, index);

                    button.Activate();
                }
            }
        }
    }
}