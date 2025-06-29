using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using MordhauProgression.Common.Config;
using MordhauProgression.Common.Assets;

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

        spriteBatch.Draw(texture, GetInnerDimensions().Position(), null, color with { A = 100 }, 0, origin, screenSize / texture.Size(), SpriteEffects.None, 0);
    }
}
