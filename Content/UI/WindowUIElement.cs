using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using MordhauProgression.Common.Config;
using MordhauProgression.Common.Assets;
using Terraria.GameContent;
using System.Linq;
using System.Collections.Generic;

namespace MordhauProgression.Content.UI;

public class WindowUIElement : UIElement
{
    public override void OnInitialize() => IgnoresMouseInteraction = true;

    public override void Draw(SpriteBatch spriteBatch)
    {
        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

        Texture2D texture = Textures.Window.Value;

        Vector2 origin = texture.Size() * 0;

        var color = Color.White * 150f;

        spriteBatch.Draw(texture, GetInnerDimensions().Position(), null, color with { A = 240 }, 0, origin, screenSize / texture.Size(), SpriteEffects.None, 0);


        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);



        for (int i = 0; i < 4; i++)
        {
            int id = i == 0 ? Terraria.ID.ItemID.WarriorEmblem
                : i == 1 ? Terraria.ID.ItemID.RangerEmblem
                : i == 2 ? Terraria.ID.ItemID.SorcererEmblem
                : Terraria.ID.ItemID.SummonerEmblem;

            string name = i == 0 ? "Mewee"
                : i == 1 ? "Rangr"
                : i == 2 ? "Mag"
                : "Summonr";


            var offset = i > 1 ? 128 : -128;
            var pos = Vector2.Zero;

            List<Vector2> fChain = [];
            List<Vector2> sChain = [];
            List<int> fPower = [];
            List<int> sPower = [];


            TraitButtonUIElement firstTrait = null;

            for(int c = 0; c < 4; c++)
            {
                firstTrait = TraitButtonUISystem.TraitRegistry?.FirstOrDefault(t => t.role == name && t.index == c && t.row == 0).trait;
                if (firstTrait != null)
                {
                    pos = c == 0 ? new Vector2(firstTrait.Left.Pixels + firstTrait.Width.Pixels, firstTrait.Top.Pixels + firstTrait.Height.Pixels / 2) : pos;

                    var center = new Vector2(firstTrait.Left.Pixels + firstTrait.Width.Pixels / 2, firstTrait.Top.Pixels + firstTrait.Height.Pixels / 2);
                    fChain.Add(center);
                    fPower.Add(firstTrait.Tier);
                }
            }

            for (int c = 0; c < 4; c++)
            {
                firstTrait = TraitButtonUISystem.TraitRegistry?.FirstOrDefault(t => t.role == name && t.index == c && t.row == 1).trait;
                if (firstTrait != null)
                {
                    pos += c == 0 ? new Vector2((firstTrait.Left.Pixels - pos.X) / 2, offset) : Vector2.Zero;

                    var center = new Vector2(firstTrait.Left.Pixels + firstTrait.Width.Pixels / 2, firstTrait.Top.Pixels + firstTrait.Height.Pixels / 2);
                    sChain.Add(center);
                    sPower.Add(firstTrait.Tier);
                }
            }

            if (pos == Vector2.Zero)
                return;

            texture = TextureAssets.Chains[8].Value;

            var scale = new Vector2(20, 50) / texture.Size();


            for (int s = 0; s < 2; s++)
            {
                var chain = s == 0 ? sChain : fChain;
                var power = s == 0 ? sPower : fPower;

                for (int n = 0; n < 4; n++)
                {
                    var prev = n == 0 ? pos : chain[n - 1];
                    var Center = chain[n] - ((chain[n] - prev) / 1.5f);
                    color = Color.White;
                    color *= 1f / (2.01f - 0.5f * power[n]);


                    spriteBatch.Draw(texture, Center, null, color with { A = 255 }, prev.DirectionTo(Center).ToRotation() + PiOver2, texture.Size() / 2, scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(texture, Center + (Center - prev), null, color with { A = 255 }, prev.DirectionTo(Center).ToRotation() + PiOver2, texture.Size() / 2, scale, SpriteEffects.None, 0);
                }
            }


            texture = TextureAssets.Item[id].Value;
            spriteBatch.Draw(texture, pos, null, Color.White, 0, texture.Size() * 0.5f, 3f, SpriteEffects.None, 0);
        }
    }
}
