using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.UI;
using Terraria;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class PointUISystem : ModSystem
{
    internal PointUIState state;
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
        HideResourceBarsSystem.NameList.Add(LayerName);

        state = new PointUIState();
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

    public static string LayerName = "MordhauProgression: Point";

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

public class PointUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Textures.Point.Value;

        Vector2 origin = texture.Size() * 0;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var pos = new Vector2(Left.Pixels, Top.Pixels);

        spriteBatch.Draw(texture, pos, null, Color.White, 0, origin, 1, SpriteEffects.None, 0);

    }

    #region Fields

    #endregion

    public override void Update(GameTime gameTime)
    {
        if (ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }
}

public class PointUIState : UIState
{
    public override void OnInitialize()
    {
        var pos = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 2, Main.instance.GraphicsDevice.Viewport.Height / 1.33f);


        PointUIElement element = new();

        element.Left.Set(pos.X, 0f);
        element.Top.Set(pos.Y, 0f);

        element.Width.Set(48, 0);
        element.Height.Set(48, 0);
        element.MinWidth.Set(48, 0);
        element.MaxWidth.Set(48, 0);
        element.MaxHeight.Set(48, 0);
        element.MinHeight.Set(48, 0);

        Append(element);
        element.Activate();
    }
}