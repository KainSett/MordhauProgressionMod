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
    public override void OnEnterWorld()
    {
        ModContent.GetInstance<WindowUISystem>()?.state.Deactivate();
        ModContent.GetInstance<ArmorUISystem>()?.state.Deactivate();
        ModContent.GetInstance<ArmorCoinUISystem>()?.state.Deactivate();
        ModContent.GetInstance<TraitButtonUISystem>()?.state.Deactivate();
        ModContent.GetInstance<PointUISystem>()?.state.Deactivate();
        ModContent.GetInstance<RoleUISystem>()?.state.Deactivate();
        ModContent.GetInstance<TooltipUISystem>()?.state.Deactivate();

        ModContent.GetInstance<WindowUISystem>()?.ReInitialize();
        ModContent.GetInstance<ArmorUISystem>()?.ReInitialize();
        ModContent.GetInstance<ArmorCoinUISystem>()?.ReInitialize();
        ModContent.GetInstance<TraitButtonUISystem>()?.ReInitialize();
        ModContent.GetInstance<PointUISystem>()?.ReInitialize();
        ModContent.GetInstance<RoleUISystem>()?.ReInitialize();
        ModContent.GetInstance<TooltipUISystem>()?.ReInitialize();

        ModContent.GetInstance<WindowUISystem>()?.state.Activate();
        ModContent.GetInstance<ArmorUISystem>()?.state.Activate();
        ModContent.GetInstance<ArmorCoinUISystem>()?.state.Activate();
        ModContent.GetInstance<TraitButtonUISystem>()?.state.Activate();
        ModContent.GetInstance<PointUISystem>()?.state.Activate();
        ModContent.GetInstance<RoleUISystem>()?.state.Activate();
        ModContent.GetInstance<TooltipUISystem>()?.state.Activate();
    }

    public int Points = 0;

    public bool WindowOpen = false;

    public Dictionary<int, List<(string role, int row, int index, int tier, TraitButtonUIElement trait)>> SkillTree = [];

    public List<(int row, int level)> SubRoleLevel = [];

    public HashSet<ArmorUIElement> Armor = [];

    public Dictionary<int, List<(int type, int tier)>> loadouts = [];

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
            CurrentTooltipUI = null;
            ModContent.GetInstance<TraitButtonUISystem>()?.Hide();
            ModContent.GetInstance<ArmorUISystem>()?.Hide();
            ModContent.GetInstance<RoleUISystem>()?.Hide();
            ModContent.GetInstance<TooltipUISystem>()?.Hide();
            ModContent.GetInstance<PointUISystem>()?.Show();
            ModContent.GetInstance<ArmorCoinUISystem>()?.Show();
        }
        else
        {
            if (CurrentTooltipUI != null)
                ModContent.GetInstance<TooltipUISystem>()?.Show(CurrentTooltipUI);

            else ModContent.GetInstance<TooltipUISystem>()?.Hide();


            CurrentTooltipUI = null;
            ModContent.GetInstance<PointUISystem>()?.Hide();
        }

        if (SkillTree?.Count == 0)
            return;

        SubRoleLevel.Clear();
        for (int i = 0; i < 8; i++)
            SubRoleLevel.Add((i, 0));


        var currentTree = SkillTree[Main.LocalPlayer.CurrentLoadoutIndex];
        for (int i = 0; i < currentTree.Count; i++)
        {
            var c = currentTree[i];
            var value = SubRoleLevel.First(l => l.row == c.trait.ActualRow);
            SubRoleLevel[value.row] = (value.row, c.tier + value.level);
            

            if (c.index == 0)
                c.trait.Open = true;

            else if (currentTree.First(s => s.role == c.role && s.row == c.row && s.index == c.index - 1).tier == 0)
                c.trait.Open = false;
            else c.trait.Open = true;


            if (!c.trait.Open)
                currentTree[currentTree.IndexOf(c)] = (c.role, c.row, c.index, 0, c.trait);
        }
        SkillTree[Main.LocalPlayer.CurrentLoadoutIndex] = currentTree;
    }
}