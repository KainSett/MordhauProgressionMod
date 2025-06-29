using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MordhauProgression.Content.UI;

[Autoload(Side = ModSide.Client)]
public class WindowUISystem : ModSystem
{
    internal WindowUIState state;

    public void OpenUI()
    {
        //if (player.mount.Active)
        //    player.mount.Dismount(player);

        Main.ClosePlayerChat();

        // Fixes some bugs with menus opened via right click.
        Main.mouseRightRelease = false;

        Main.ingameOptionsWindow = false;
        Main.chatText = string.Empty;

        IngameFancyUI.OpenUIState(state);
    }

    public override void Load()
    {
        state = new WindowUIState();
        state.Activate();
    }

    public bool IsActive()
    {
        if (!Main.inFancyUI) 
            return false;

        return true;
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (state.InUI)
            Main.LocalPlayer.mouseInterface = true;
    }
}
