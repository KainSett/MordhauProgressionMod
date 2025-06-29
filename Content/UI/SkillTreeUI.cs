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

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class TraitButtonUISystem : ModSystem
{
    internal TraitButtonUIState state;
    private UserInterface Interface;

    public void Show() => Interface?.SetState(state);

    public void Hide() => Interface?.SetState(null);

    public override void Load()
    {
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

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ingame Options"));
        if (index == -1)
            return;

        layers.Insert(index - 2, new LegacyGameInterfaceLayer("MordhauProgression: Trait Button", delegate
        {
            Interface.Draw(Main.spriteBatch, new GameTime());

            return true;

        }, InterfaceScaleType.None));
    }
}

public class TraitButtonUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Textures.Icons.Value;

        Vector2 origin = texture.Size() * 0;

        var rect = texture.Frame(4, 3, type, Tier, -2, -2);
        var scale = Scale * Height.Pixels / 32;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var pos = new Vector2(Left.Pixels, Top.Pixels);

        var scaleOffset = rect.Size() * (Scale - 1) / 2;
        //Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        //var offset = -rect.TopLeft() * scale + new Vector2(2 * type, 2 * 0) * scale;
        spriteBatch.Draw(texture, pos - scaleOffset, rect, Color.White, 0, origin, scale, SpriteEffects.None, 0);

        if (Flash != 0)
        {
            texture = TextureAssets.MagicPixel.Value;

            var PixelScale = Scale * new Vector2(Width.Pixels, Height.Pixels) / new Vector2(texture.Width, texture.Height);

            var color = Flash > 0 ? Color.White : Color.DarkGray;
            color *= float.Abs(Flash);

            spriteBatch.Draw(texture, pos - scaleOffset, null, color with { A = 150 }, 0, origin, PixelScale, SpriteEffects.None, 0);
        }
    }
    public int Tier = 0;

    public int type = 0;

    public float Scale = 1f;

    public float Flash = 0;

    public Rectangle Area = Rectangle.Empty;

    public override void LeftClick(UIMouseEvent evt)
    {
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
        var newTier = (int)Clamp(Tier + 1, 0, 2);

        if (newTier != Tier)
            Flash = 1f;

        Tier = newTier;
    }

    public void SetToOriginalArea()
    {
        Top.Set(Area.Top, 0);
        Left.Set(Area.Left, 0);
        MinWidth.Set(Area.Width, 0);
        MaxWidth.Set(Area.Width, 0);
        Width.Set(Area.Width, 0);
        MinHeight.Set(Area.Height, 0);
        MaxHeight.Set(Area.Height, 0);
        Height.Set(Area.Height, 0);
    }

    public override void Recalculate()
    {
        base.Recalculate();

        var dim = GetDimensions();
        if (GetInnerDimensions().ToRectangle() != Area)
        {
            SetToOriginalArea();
        }
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
            Scale = Clamp(Scale + 0.015f, 1, 1.2f);
        }

        else Scale = Clamp(Scale - 0.015f, 1, 1.2f);
    }
}

public class TraitButtonUIState : UIState
{
    public override void OnInitialize()
    {
        var screenHalved = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 2, Main.instance.GraphicsDevice.Viewport.Height / 5);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                TraitButtonUIElement button = new();
                button.SetPadding(0);

                var pos = new Vector2(x * 100 - 40 * (x % 2) - 360, y * 80 + 100 * (y / 4 - 1));
                pos += screenHalved;

                button.Area = new Rectangle((int)pos.X, (int)pos.Y, 48, 48);

                button.Left.Set(pos.X, 0f);
                button.Top.Set(pos.Y, 0f);

                button.Width.Set(48, 0);
                button.Height.Set(48, 0);
                button.MinWidth.Set(48, 0);
                button.MaxWidth.Set(48, 0);
                button.MaxHeight.Set(48, 0);
                button.MinHeight.Set(48, 0);

                button.type = x % 4;
                button.Tier = 0;

                Append(button);


                button.Activate();
            }
        }
    }
}