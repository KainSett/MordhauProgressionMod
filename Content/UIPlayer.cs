using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using MordhauProgression.Content.UI;
using System.Linq;
using System;
using Terraria.ModLoader.IO;
using Humanizer;

namespace MordhauProgression.Content;

public class UIPlayer : ModPlayer
{
    public Dictionary<string, List<int>> TraitTiersData = [];

    public Dictionary<int, List<int>> ArmorTiersData = [];

    public void PopulateDataTemplates(List<List<int>> traits = null, List<List<int>> armor = null)
    {
        TraitTiersData.Clear();
        ArmorTiersData.Clear();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                var value = traits is null ? [0, 0, 0] : traits[x * 4 + y];
                TraitTiersData.Add(TraitButtonUIElement.GetName(TraitButtonUIElement.GetRole(x), x, y), value);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            var value = armor is null ? [0, 0, 0] : armor[i];
            ArmorTiersData.Add(i, value);
        }
    }

    public override void LoadData(TagCompound tag)
    {
        PopulateDataTemplates();

        var t = tag.GetCompound("TraitTiersData");
        var a = tag.GetCompound("ArmorTiersData");

        var tCopy = TraitTiersData;
        foreach (var name in TraitTiersData)
        {
            tCopy[name.Key] = [..t.GetList<int>(name.Key)];
        }
        TraitTiersData = tCopy;

        var aCopy = ArmorTiersData;
        foreach (var name in ArmorTiersData)
        {
            aCopy[name.Key] = [..a.GetList<int>($"{name.Key}")];
        }
        ArmorTiersData = aCopy;
    }

    public override void SaveData(TagCompound tag)
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            List<List<int>> tree = [];
            foreach (var trait in player.SkillTree)
            {
                List<int> values = [];
                foreach (var value in trait.Value)
                {
                    values.Add(value.tier);
                }

                tree.Add(values);
            }

            List<List<int>> loadout = [];
            foreach (var part in player.loadouts)
            {
                List<int> values = [];
                foreach (var value in part.Value)
                {
                    values.Add(value.tier);
                }

                loadout.Add(values);
            }

            PopulateDataTemplates(tree, loadout);
        }

        var traitTag = new TagCompound();
        foreach (var pair in TraitTiersData)
        {
            traitTag[pair.Key] = pair.Value;
        }

        tag["TraitTiersData"] = traitTag;


        var armorTag = new TagCompound();
        foreach (var pair in ArmorTiersData)
        {
            armorTag[$"{pair.Key}"] = pair.Value;
        }

        tag["ArmorTiersData"] = armorTag;
    }

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

    public Dictionary<int, List<(string role, int row, int index, int tier, bool open, float flash)>> SkillTree = [];

    public List<(int row, int level)> SubRoleLevel = [];

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
            var (row, level) = SubRoleLevel.First(l => l.row == c.row);
            SubRoleLevel[row] = (row, c.tier + level);
            

            if (c.index == 0)
                c.open = true;

            else if (currentTree.First(s => s.role == c.role && s.row == c.row && s.index == c.index - 1).tier == 0)
                c.open = false;

            else c.open = true;

            if (!c.open)
                c.tier = 0;

            currentTree[i] = (c.role, c.row, c.index, c.tier, c.open, c.flash);
        }
        SkillTree[Main.LocalPlayer.CurrentLoadoutIndex] = currentTree;
    }
}