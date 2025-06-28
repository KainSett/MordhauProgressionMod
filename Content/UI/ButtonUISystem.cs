using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MordhauProgression.Content.UI;

[Autoload(Side = ModSide.Client)]
public class ButtonUISystem : ModSystem
{
    internal ButtonUIState state;
    private UserInterface Interface;

    public void Show() => Interface?.SetState(state);

    public void Hide() => Interface?.SetState(null);

    public override void Load()
    {
        state = new ButtonUIState();
        Interface = new UserInterface();
        state.Activate();
        Show();
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

        layers.Insert(index, new LegacyGameInterfaceLayer("MordhauProgression: Button", delegate
        {
            Interface.Draw(Main.spriteBatch, new GameTime());

            return true;

        }, InterfaceScaleType.UI));
    }
}
