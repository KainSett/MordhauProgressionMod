using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using MordhauProgression.Content.UI;

namespace MordhauProgression.Content;

public class UIPlayer : ModPlayer
{
    public int Points = 0;

    public bool WindowOpen = false;

    public override void ResetEffects()
    {
        if (ModContent.GetInstance<WindowUISystem>()?.IsActive() != true)
        {
            //ModContent.GetInstance<WindowUISystem>()?.OpenUI();
            ModContent.GetInstance<TraitButtonUISystem>()?.Show();
            
            if (Main.LocalPlayer.velocity.Y < 0)
                ModContent.GetInstance<TraitButtonUISystem>()?.Hide();
        }
    }
}