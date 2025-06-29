using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using MordhauProgression.Content.UI;
using System.Linq;

namespace MordhauProgression.Content;

public class UIPlayer : ModPlayer
{
    public int Points = 0;

    public bool WindowOpen = false;

    public HashSet<(string role, int row, int index, TraitButtonUIElement trait)> SkillTree = [];

    public override void ResetEffects()
    {
        if (Main.LocalPlayer.velocity.Y < 0)
        {
            ModContent.GetInstance<TraitButtonUISystem>()?.Hide();
            ModContent.GetInstance<WindowUISystem>()?.OpenUI();
            ModContent.GetInstance<TraitButtonUISystem>()?.Show();
        }

        if (ModContent.GetInstance<WindowUISystem>()?.IsActive() != true)
        {
            SkillTree.Clear();
            ModContent.GetInstance<TraitButtonUISystem>()?.Hide();
        }


        if (SkillTree?.Count == 0)
            return;

        foreach (var (role, row, index, trait) in SkillTree)
        {
            if (index == 0)
                trait.Open = true;

            else if (SkillTree.First(s => s.role == role && s.row == row && s.index == index - 1).trait.Tier == 0)
                trait.Open = false;
            else trait.Open = true;


            if (!trait.Open)
                trait.Tier = 0;
        }
    }
}