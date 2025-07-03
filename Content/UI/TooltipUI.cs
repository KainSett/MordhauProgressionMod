
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


    public void Show(Rectangle area, Action draw)
    {
        Interface?.SetState(state);

        var element = state.Children.FirstOrDefault();
        if (element == null)
            return;

        element.Top.Set(area.Y, 0);
        element.Left.Set(area.X, 0);
        element.Width.Set(area.Width, 0);
        element.Height.Set(area.Height, 0);

        Draw = draw;
    }

    public void Hide()
    {
        Interface?.SetState(null);

        Draw = null;
    }

    public override void Load()
    {
        HideResourceBarsSystem.NameList.Add(LayerName);

        state = new TooltipUIState();
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

    public static string LayerName = "MordhauProgression: Tooltip";

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

public class TooltipUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        ModContent.GetInstance<TooltipUISystem>()?.Draw?.Invoke();
    }

    public override void Update(GameTime gameTime)
    {
        
    }
}

public class TooltipUIState : UIState
{
    public override void OnInitialize()
    {
        TooltipUIElement element = new();
        element.SetPadding(0);

        element.Left.Set(0, 0f);
        element.Top.Set(0, 0f);

        element.Width.Set(160, 0);
        element.Height.Set(160, 0);

        Append(element);
        element.Activate();
    }
}