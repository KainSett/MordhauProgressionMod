using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using MordhauProgression.Content.UI;
using System.Linq;
using System;

namespace MordhauProgression.Content;

public class UIPlayer : ModPlayer
{
    public int Points = 0;

    public bool WindowOpen = false;

    public HashSet<(string role, int row, int index, TraitButtonUIElement trait)> SkillTree = [];

    public static List<(int row, int level)> SubRoleLevel = [];

    public HashSet<ArmorUIElement> Armor = [];

    public static Action CurrentTooltipUI = new(() => { });

    public override void ResetEffects()
    {
        if (Main.LocalPlayer.velocity.Y < 0)
        {
            ModContent.GetInstance<TraitButtonUISystem>()?.Hide();
            ModContent.GetInstance<ArmorUISystem>()?.Hide();
            ModContent.GetInstance<WindowUISystem>()?.OpenUI();
            ModContent.GetInstance<TraitButtonUISystem>()?.Show();
            ModContent.GetInstance<ArmorUISystem>()?.Show();
            ModContent.GetInstance<RoleUISystem>()?.Show();
        }

        if (WindowUISystem.IsActive() != true)
        {
            SkillTree.Clear();
            Armor.Clear();
            CurrentTooltipUI = null;
            ModContent.GetInstance<TraitButtonUISystem>()?.Hide();
            ModContent.GetInstance<ArmorUISystem>()?.Hide();
            ModContent.GetInstance<RoleUISystem>()?.Hide();
            ModContent.GetInstance<TooltipUISystem>()?.Hide();
        }
        else
        {
            if (CurrentTooltipUI != null)
                ModContent.GetInstance<TooltipUISystem>()?.Show(CurrentTooltipUI);

            else ModContent.GetInstance<TooltipUISystem>()?.Hide();


            CurrentTooltipUI = null;
        }


        if (SkillTree?.Count == 0)
            return;

        SubRoleLevel.Clear();
        for (int i = 0; i < 8; i++)
            SubRoleLevel.Add((i, 0));


        foreach (var (role, row, index, trait) in SkillTree)
        {
            var value = SubRoleLevel.First(l => l.row == trait.ActualRow);
            SubRoleLevel[value.row] = (value.row, trait.Tier + value.level);
            

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