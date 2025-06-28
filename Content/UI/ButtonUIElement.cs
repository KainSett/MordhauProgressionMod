using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using MordhauProgression.Common.Config;
using MordhauProgression.Common.Assets;

namespace MordhauProgression.Content.UI;

public class ButtonUIElement : UIElement
{
    private static readonly Vector2 ButtonOffset = new(30f);

    public override void OnInitialize() => IgnoresMouseInteraction = true;

    public override void Draw(SpriteBatch spriteBatch)
    {
        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
        Vector2 center = ModContent.GetInstance<ClientConfig>().ButtonUIPosition * screenSize + ButtonOffset;

        Texture2D texture = Textures.Button.Value;

        Vector2 origin = texture.Size() * 0.5f;

        spriteBatch.Draw(texture, center, null, Color.White, 0, origin, 2, SpriteEffects.None, 0);
    }
}
