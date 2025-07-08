
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using System.Linq;
using System;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class TooltipUISystem : ModSystem
{
    internal TooltipUIState state;
    private UserInterface Interface;
    public Action Draw = new(() => { });


    public void Show(Action draw)
    {
        Interface?.SetState(state);

        var element = state.Children.FirstOrDefault();
        if (element == null)
            return;


        Draw = draw;
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
        state = new TooltipUIState();
        Interface = new UserInterface();
    }


    public bool IsActive()
    {
        if (Interface.CurrentState == null)
            return false;

        return true;
    }

    public override void UpdateUI(GameTime gameTime) => Interface?.Update(gameTime);

    public static string LayerName = "MordhauProgression: Tooltip";

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
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

public class TooltipUIElement : UIElement
{
    public override void OnInitialize() => IgnoresMouseInteraction = true;

    public override void Draw(SpriteBatch spriteBatch)
    {
        ModContent.GetInstance<TooltipUISystem>()?.Draw?.Invoke();
    }

    public override void Update(GameTime gameTime)
    {
        ModContent.GetInstance<TooltipUISystem>().Draw = null;
    }
}

public class TooltipUIState : UIState
{
    public override void OnInitialize()
    {
        var position = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 2 - 600, Main.instance.GraphicsDevice.Viewport.Height / 2 - 40 - 250);

        TooltipUIElement element = new();
        element.SetPadding(0);

        element.Left.Set(position.X, 0f);
        element.Top.Set(position.Y, 0f);

        element.Width.Set(160, 0);
        element.Height.Set(160, 0);

        Append(element);
        element.Activate();
    }
}