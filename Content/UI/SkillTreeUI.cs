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
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Fancy UI"));
        if (index == -1)
            return;

        layers.Insert(index+1, new LegacyGameInterfaceLayer("MordhauProgression: Trait Button", delegate
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
        var scale = Height.Pixels / 32;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var pos = new Vector2(Left.Pixels, Top.Pixels);

        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        var offset = -rect.TopLeft() * scale + new Vector2(2 * type, 2 * 0) * scale;
        spriteBatch.Draw(texture, pos, rect, Color.White, 0, origin, scale, SpriteEffects.None, 0);
    }
    public int Tier = 0;

    public int type = 0;

    public override void LeftClick(UIMouseEvent evt)
    {
        if (evt.Target == this)
            OnLeftInteract(evt);
    }

    public override void RightClick(UIMouseEvent evt)
    {
        if (evt.Target == this)
            OnRightInteract(evt);
    }
    private void OnRightInteract(UIMouseEvent evt)
    {
        Tier = 0;
    }

    private void OnLeftInteract(UIMouseEvent evt)
    {
        Tier = (int)Clamp(Tier + 1, 0, 2);
    }

    public override void Update(GameTime gameTime)
    {

        if (ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }

    public override void Recalculate()
    {
        base.Recalculate();
        Top.Set(Top.Pixels / Main.UIScale, 0);
        Left.Set(Left.Pixels / Main.UIScale, 0);
        Height.Set(Clamp(Height.Pixels / Main.UIScale, MinHeight.Pixels, MaxHeight.Pixels), 0);
        Width.Set(Clamp(Width.Pixels / Main.UIScale, MinWidth.Pixels, MaxWidth.Pixels), 0);
    }
}

public class TraitButtonUIState : UIState
{
    /*public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }*/

    public override void OnInitialize()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                TraitButtonUIElement button = new();
                button.SetPadding(0);

                var pos = new Vector2(x * 100 - 40 * (x % 2) - 400, y * 80);
                pos += new Vector2(Main.screenWidth / 2, Main.screenHeight / 4);


                button.Left.Set(pos.X, 0f);
                button.Top.Set(pos.Y, 0f);

                button.Width.Set(48, 0);
                button.Height.Set(48, 0);
                button.MinWidth.Set(48, 0);
                button.MaxWidth.Set(48, 0);
                button.MaxHeight.Set(48, 0);
                button.MinHeight.Set(48, 0);
                //button.HAlign = 0.5f;
                button.type = x % 4;
                button.Tier = 0;

                Append(button);

                ModContent.GetInstance<TraitButtonUISystem>()?.Show();

                button.Activate();
            }
        }
    }
}