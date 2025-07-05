using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.UI;
using Terraria;
using System.Linq;
using System.Data;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class RoleUISystem : ModSystem
{
    internal RoleUIState state;
    private UserInterface Interface;
    public static int HoveredRow = 0;

    public void Show() => Interface?.SetState(state);
    

    public void Hide() => Interface?.SetState(null);
    

    public override void Load()
    {
        UIDetoursSystem.NameList.Add(LayerName);
        ReInitialize();
    }

    public void ReInitialize()
    {
        state = new RoleUIState();
        Interface = new UserInterface();
    }


    public bool IsActive()
    {
        if (Interface.CurrentState == null)
            return false;

        return true;
    }

    public override void UpdateUI(GameTime gameTime) => Interface?.Update(gameTime);

    public static string LayerName = "MordhauProgression: Role";

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

public class RoleUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Textures.Roles.Value;

        var rect = texture.Frame(8, 1, index.row, 0, -3, -3);
        var scale = 1f;

        Vector2 origin = rect.Size() * 0.5f;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2 - 10);

        spriteBatch.Draw(texture, center, rect, Color.White with { A = 0 }, 0, origin, scale, SpriteEffects.None, 0);

        if (RoleUISystem.HoveredRow == index.row)
        {
            var c = Color.White with { A = 0 };
            c *= 0.8f;

            spriteBatch.Draw(texture, center, rect, c, 0, origin, scale, SpriteEffects.None, 0);
        }

        var color = Color.Gold with { A = 50 };

        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            var level = player.SubRoleLevel[index.row].level;
            if (level != 0)
            {
                var cutRect = rect with { Height = (int)(rect.Height * (level / 8f)), Y = (int)(rect.Height - rect.Height * (level / 8f)) };

                if (player.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].Any(e => e.trait.Flash > 0 && e.trait.ActualRow == index.row))
                {

                    color *= 0.5f + player.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].First(e => e.trait.Flash > 0 && e.trait.ActualRow == index.row).trait.Flash;
                    color.A = 0;
                }


                spriteBatch.Draw(texture, center + new Vector2(0, rect.Height - rect.Height * (level / 8f)), cutRect, color, 0, origin, scale, SpriteEffects.None, 0);
            }



            var font = FontAssets.MouseText.Value;

            var Scale = new Vector2(1.5f);
            var text = Language.GetTextValue($"Mods.MordhauProgression.Traits.{index.role}.{index.name}.Name");
            var textSize = ChatManager.GetStringSize(font, text, Scale);

            color = Color.Khaki.MultiplyRGB(Color.Khaki * 1.5f) * (level / 8f);
            color *= RoleUISystem.HoveredRow == index.row ? 0.66f : 0.44f;

            center += new Vector2(-textSize.X / 2, Height.Pixels / 2);

            ChatManager.DrawColorCodedString(spriteBatch, font, text, center - new Vector2(0, 0), color with { A = 0 }, 0, Vector2.Zero, Scale, 160);

            color = Color.White;
            color *= RoleUISystem.HoveredRow == index.row ? 0.5f : 0.25f;
            color *= 1 - (level / 8f);

            ChatManager.DrawColorCodedString(spriteBatch, font, text, center - new Vector2(0, 0), color with { A = 50 }, 0, Vector2.Zero, Scale, 160);


            if (index.row % 2 == 0)
                return;

            center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2);

            Scale = new Vector2(2f);
            text = Language.GetTextValue($"Mods.MordhauProgression.Traits.{index.role}.Name");
            textSize = ChatManager.GetStringSize(font, text, Scale);

            color = Color.White;
            color *= 0.33f;

            center += new Vector2(-textSize.X / 2 - 60, -Height.Pixels - 10);

            ChatManager.DrawColorCodedString(spriteBatch, font, text, center, color, 0, Vector2.Zero, Scale, 160);
        }
    }

    public (string role, string name, int row) index = new();

    public override void Update(GameTime gameTime)
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            var first = player.SkillTree[Main.LocalPlayer.CurrentLoadoutIndex].FirstOrDefault(t => t.index == 0 && t.trait.id.subRole == index.name).trait;
            if (first == null)
                return;


            var center = new Vector2(first.Left.Pixels + first.Width.Pixels / 2 + 0.1f, first.Top.Pixels - first.Height.Pixels / 2 + 0.1f);

            Left.Set(center.X - Width.Pixels / 2, 0f);
            Top.Set(center.Y - Height.Pixels, 0f);

        }
        else return;

        RoleUISystem.HoveredRow = -1;
    }
}

public class RoleUIState : UIState
{
    public override void OnInitialize()
    {
        for (int x = 0; x < 8; x++)
        {
            RoleUIElement element = new();
            element.SetPadding(0);

            var role = "Melee Warrior";
            bool first = x % 2 == 0;
            switch (x)
            {
                case < 2:
                    if (!first)
                        role = "Melee Tank";
                    break;

                case < 4:
                    role = first ? "Ranger Sharpshooter" : "Ranger Archer";
                    break;

                case < 6:
                    role = first ? "Mage Sorcerer" : "Mage Wizard";
                    break;

                default:
                    role = first ? "Summoner Commander" : "Summoner Defender";
                    break;
            }

            element.Left.Set(0, 0f);
            element.Top.Set(0, 0f);

            element.Width.Set(100, 0);
            element.Height.Set(100, 0);
            element.MinWidth.Set(100, 0);
            element.MaxWidth.Set(100, 0);
            element.MaxHeight.Set(100, 0);
            element.MinHeight.Set(100, 0);

            element.index = (role[..role.IndexOf(' ')], role[(role.IndexOf(' ') + 1)..], x);


            Append(element);

            element.Activate();
        }
    }
}