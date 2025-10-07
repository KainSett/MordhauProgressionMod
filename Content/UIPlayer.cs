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

    public override void Initialize()
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            player.SkillTree.Clear();
            player.loadouts.Clear();
            player.Points.Clear();
            player.TotalPoints = 0;
        }
    }

    public void PopulateDataTemplates(List<List<int>> traits = null, List<List<int>> armor = null)
    {
        TraitTiersData.Clear();
        ArmorTiersData.Clear();

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                var value = traits is null || traits.Count == 0 ? [0, 0, 0] : traits[x * 4 + y];
                TraitTiersData.Add(TraitButtonUIElement.GetName(TraitButtonUIElement.GetRole(x), x, y), value);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            var value = armor is null || armor.Count == 0 ? [0, 0, 0] : armor[i];
            ArmorTiersData.Add(i, value);
        }
    }

    public override void LoadData(TagCompound tag)
    {
        PopulateDataTemplates();

        var t = tag.GetCompound("TraitTiersData");
        var a = tag.GetCompound("ArmorTiersData");

        for (int i = 0; i < TraitTiersData.Count; i++)
        {
            var name = TraitButtonUIElement.GetName(TraitButtonUIElement.GetRole((int)float.Floor(i / 4f)), (int)float.Floor(i / 4f), i % 4);
            TraitTiersData[name] = [..t.GetList<int>(name)];
        }

        for (int i = 0; i < ArmorTiersData.Count; i++) 
        {
            ArmorTiersData[i] = [..a.GetList<int>($"{i}")];
        }

        Points = [.. tag.GetList<int>("Points")];
        TotalPoints = tag.GetInt("TotalPoints");
    }

    public override void SaveData(TagCompound tag)
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            if (player.SkillTree.Count == 0 || player.SkillTree.First().Value.Count == 0 || player.loadouts.Count == 0 || player.loadouts.First().Value.Count == 0)
            {
                //TraitTiersData.Clear();
                //ArmorTiersData.Clear();
                return;
            }

            List<List<int>> tree = [];
            for (int i = 0; i < player.SkillTree.First().Value.Count; i++)
            {
                tree.Add([player.SkillTree[0][i].tier, player.SkillTree[1][i].tier, player.SkillTree[2][i].tier]);
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


        tag["Points"] = Points;
        tag["TotalPoints"] = TotalPoints;
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

        if (Points.Count == 0)
            Points = [0, 0, 0];

        ModContent.GetInstance<WindowUISystem>()?.ReInitialize();
        ModContent.GetInstance<ArmorCoinUISystem>()?.ReInitialize();
        ModContent.GetInstance<PointUISystem>()?.ReInitialize();
        ModContent.GetInstance<TooltipUISystem>()?.ReInitialize();

        ModContent.GetInstance<WindowUISystem>()?.state.Activate();
        ModContent.GetInstance<ArmorCoinUISystem>()?.state.Activate();
        ModContent.GetInstance<PointUISystem>()?.state.Activate();
        ModContent.GetInstance<TooltipUISystem>()?.state.Activate();
    }

    public List<int> Points = [20, 20, 20];

    public int TotalPoints = 20;

    public bool WindowOpen = false;

    public Dictionary<int, List<(string role, int row, int index, int tier, bool open, float flash)>> SkillTree = [];

    public List<(int row, int level)> SubRoleLevel = [];

    public Dictionary<int, List<(int type, int tier)>> loadouts = [];

    public static Action CurrentTooltipUI = new(() => { });

    public void OpenWindow()
    {
        WindowOpen = true;
        ModContent.GetInstance<TraitButtonUISystem>()?.Hide();
        ModContent.GetInstance<ArmorUISystem>()?.Hide();
        ModContent.GetInstance<WindowUISystem>()?.OpenUI();
        ModContent.GetInstance<TraitButtonUISystem>()?.Show();
        ModContent.GetInstance<ArmorUISystem>()?.Show();
        ModContent.GetInstance<RoleUISystem>()?.Show();
    }

    public void CloseWindow()
    {
        WindowOpen = false;
    }

    public override void ResetEffects()
    {
        ModContent.GetInstance<PointUISystem>()?.Show();

        if (CurrentTooltipUI != null)
            ModContent.GetInstance<TooltipUISystem>()?.Show(CurrentTooltipUI);
        else ModContent.GetInstance<TooltipUISystem>()?.Hide();

        if (!WindowUISystem.IsActive())
            WindowOpen = false;

        if (WindowOpen)
        {



            CurrentTooltipUI = null;
        }
        else
        {
            CurrentTooltipUI = null;
            ModContent.GetInstance<TraitButtonUISystem>()?.Hide();
            ModContent.GetInstance<ArmorUISystem>()?.Hide();
            ModContent.GetInstance<RoleUISystem>()?.Hide();
            ModContent.GetInstance<ArmorCoinUISystem>()?.Show();
        }

        if (SkillTree?.Count == 0)
            return;

        SubRoleLevel.Clear();
        for (int i = 0; i < 9; i++)
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
            {
                c.tier = 0;
            }

            currentTree[i] = (c.role, c.row, c.index, c.tier, c.open, c.flash);
        }
        SkillTree[Main.LocalPlayer.CurrentLoadoutIndex] = currentTree;
    }
}