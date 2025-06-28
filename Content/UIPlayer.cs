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
        if (ModContent.GetInstance<ButtonUISystem>()?.IsActive() != true && ModContent.GetInstance<WindowUISystem>()?.IsActive() != true)
        {
            ModContent.GetInstance<ButtonUISystem>()?.Show();
        }
        else if (ModContent.GetInstance<WindowUISystem>()?.IsActive() == true)
        {
            ModContent.GetInstance<ButtonUISystem>()?.Hide();
        }
    }
}