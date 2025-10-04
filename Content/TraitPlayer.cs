using MordhauProgression.Content.UI;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MordhauProgression.Content;

public class TraitPlayer : ModPlayer
{
    public override void PostUpdateMiscEffects()
    {
        DodgeChance = 0;
        OberhauChance = 0;
        MiracleChance = 0;
        AdrenalineLimit = 0;
        FletcherChance = 0;
        ScattershotChance = 0;
        BodkinChance = 0;
        YeomenDamage = 0;
        ProficiencySpeed = 0;
        LuckyDamage = 0;
        SniperDamage = 0;
        TalentSize = 0;
        DeterminationHeal = 0;
        PlotChance = 0;
        ExpertSpeed = 0;
        CharismaSpeed = 0;
        NurturingNum = 0;
        TrapsNum = 0;
        MedicalNeal = 0;
        LuckNum = 0;
        HerbologyChance = 0;
        AlchemyNum = 0;


        foreach (var (role, row, index, tier, open, flash) in Player.GetModPlayer<UIPlayer>().SkillTree[Player.CurrentLoadoutIndex])
        {
            if (!open)
                continue;

            var name = TraitButtonUIElement.GetName("Melee", row, index);

            if (role == "Melee")
                UpdateMelee(name, row, index, tier);
            else if (role == "Ranger")
                UpdateRanger(name, row, index, tier);
            else if (role == "Mage")
                UpdateMage(name, row, index, tier);
            else if (role == "Summoner")
                UpdateSummoner(name, row, index, tier);
            else 
                UpdateOther(name, row, index, tier);
        }
    }

    public float DodgeChance = 0;

    public float OberhauChance = 0;

    public float MiracleChance = 0;

    public int AdrenalineLimit = 0;

    public void UpdateMelee(string name, int row, int index, int tier)
    {
        switch (name)
        {
            case "Dexterity":
                Player.GetAttackSpeed(DamageClass.Melee) += 0.1f * tier;
                break;

            case "Brute":
                Player.GetArmorPenetration(DamageClass.Melee) += 3 * tier;
                break;

            case "Agility":
                DodgeChance = 0.1f * tier;
                break;

            case "Oberhau":
                OberhauChance = 0.05f * tier;
                break;


            case "Toughness":
                Player.endurance += 0.06f * tier;
                break;

            case "Gigantism":
                Player.statLifeMax2 += 30 * tier;
                break;

            case "Miracle":
                MiracleChance = 0.1f * tier;
                break;

            case "Adrenaline":
                AdrenalineLimit = 30 * tier;
                break;
        }
    }

    public float FletcherChance = 0;

    public float ScattershotChance = 0;

    public float BodkinChance = 0;

    public int YeomenDamage = 0;

    public float ProficiencySpeed = 0;

    public float LuckyDamage = 0;

    public int SniperDamage = 0;

    public void UpdateRanger(string name, int row, int index, int tier)
    {
        switch (name)
        {
            case "Yeomen":
                YeomenDamage = 2 * tier;
                break;

            case "Fletcher":
                FletcherChance = 0.2f * tier;
                break;

            case "Scattershot":
                ScattershotChance = 0.1f * tier;
                break;

            case "Proficiency":
                ProficiencySpeed = 0.1f * tier;
                break;


            case "Scope":
                Player.GetCritChance(DamageClass.Ranged) += 5 * tier;
                break;

            case "Lucky Shot":
                LuckyDamage += 0.07f * tier;
                break;

            case "Bodkin Tip":
                BodkinChance = 0.1f * tier;
                break;

            case "Sniper":
                SniperDamage = 6 * tier;
                break;
        }
    }

    public float TalentSize = 0;

    public int DeterminationHeal = 0;

    public float PlotChance = 0;

    public float ExpertSpeed = 0;

    public void UpdateMage(string name, int row, int index, int tier)
    {

        switch (name)
        {
            case "Talent":
                TalentSize = 0.1f * tier;
                break;

            case "Mana Vessel":
                Player.statManaMax2 += 40 * tier;
                break;

            case "Determination":
                DeterminationHeal = 10 * tier;
                break;

            case "Plot Armor":
                PlotChance = 0.2f * tier;
                break;


            case "Observance":
                Player.GetArmorPenetration(DamageClass.Magic) += 4 * tier;
                break;

            case "Efficiency":
                Player.manaCost -= 0.06f * tier;
                break;

            case "Mana Well":
                if (Main.GlobalTimeWrappedHourly % 60 == 0)
                    Player.statMana = Math.Min(Player.statManaMax2, Player.statMana + 3 * tier);
                break;

            case "Expert Casting":
                ExpertSpeed += 0.1f * tier;
                break;
        }
    }

    public float CharismaSpeed = 0;

    public int NurturingNum = 0;

    public int TrapsNum = 0;

    public int MedicalNeal = 0;

    public void UpdateSummoner(string name, int row, int index, int tier)
    {
        switch (name)
        {
            case "Precision":
                Player.GetArmorPenetration(DamageClass.Summon) += 3 * tier;
                break;

            case "Hand Sleight":
                Player.whipRangeMultiplier += 0.1f * tier;
                break;

            case "Charisma":
                CharismaSpeed = 0.1f * tier;
                break;

            case "Nurturing":
                NurturingNum = tier;
                break;


            case "Brotherhood":
                Player.GetDamage(DamageClass.Summon) += 0.05f * tier;
                break;

            case "Pleasantry":
                Player.maxTurrets += tier;
                break;

            case "Traps":
                TrapsNum = tier;
                break;

            case "Medical Supplies":
                MedicalNeal = tier;
                break;
        }
    }

    public float LuckNum = 0;

    public float HerbologyChance = 0;

    public float AlchemyNum = 0;

    public void UpdateOther(string name, int row, int index, int tier)
    {
        switch (name)
        {
            case "Stroke of Luck":
                LuckNum = 0.2f * tier;
                break;

            case "Herbology":
                HerbologyChance = 0.1f * tier;
                break;

            case "Mining":
                Player.pickSpeed -= 0.1f * tier;
                break;

            case "Alchemy":
                AlchemyNum = 0.1f * tier;
                break;
        }
    }
}