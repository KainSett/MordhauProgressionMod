using MordhauProgression.Content.UI;
using ReLogic.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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

            var name = TraitButtonUIElement.GetName(role, row, index);

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
                ProficiencySpeed = 0.15f * tier;
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

    #region Effects
    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (Player.dashDelay != 0 && Main.rand.NextFloat() < DodgeChance)
        {
            Player.SetImmuneTimeForAllTypes(50);
            Player.immune = true;
            return true;
        }
        return base.FreeDodge(info);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Main.rand.NextFloat() < OberhauChance && (modifiers.DamageType == DamageClass.Melee || modifiers.DamageType == DamageClass.MeleeNoSpeed))
            modifiers.FinalDamage *= 2;
    }

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (proj.aiStyle == ProjAIStyleID.Arrow && proj.TryGetGlobalProjectile<ArrowProj>(out var p) && p.IsArrow)
        {
            modifiers.FinalDamage.Flat += YeomenDamage;

            modifiers.CritDamage += LuckyDamage;

            var ammo = Player.ChooseAmmo(Player.HeldItem);
            if (Main.rand.NextFloat() < FletcherChance && ammo is not null && !ammo.IsAir)
                ammo.stack++;
        }
    }

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (source.AmmoItemIdUsed == AmmoID.Arrow && Main.rand.NextFloat() < ScattershotChance)
        {
            velocity = velocity.RotatedByRandom(PiOver4);
            ScattershotChance = 0;
            Player.itemTime = 0;
        }

        return base.Shoot(item, source, position, velocity, type, damage, knockback);
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        if (Main.rand.NextFloat() < MiracleChance)
            Player.Heal(info.Damage / 2);
    }

    public override void Load()
    {
        On_Player.Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float += AdrenalineHurt;
    }

    private static double AdrenalineHurt(On_Player.orig_Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float orig, Player self, Terraria.DataStructures.PlayerDeathReason damageSource, int Damage, int hitDirection, out Player.HurtInfo info, bool pvp, bool quiet, int cooldownCounter, bool dodgeable, float armorPenetration, float scalingArmorPenetration, float knockback)
    {
        var Orig = orig(self, damageSource, Damage, hitDirection, out info, pvp, quiet, cooldownCounter, dodgeable, armorPenetration, scalingArmorPenetration, knockback);
        if (self.TryGetModPlayer<TraitPlayer>(out var p) && p.AdrenalineLimit > info.Damage)
        {
            info.Knockback = 0;
            knockback = 0;
        }

        return Orig;
    }
    #endregion
}

public class ArrowProj : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        return entity.aiStyle == ProjAIStyleID.Arrow;
    }

    public bool IsArrow = false;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is EntitySource_ItemUse_WithAmmo && (source as EntitySource_ItemUse_WithAmmo).AmmoItemIdUsed == AmmoID.Arrow)
            IsArrow = true;

        if (IsArrow && Main.LocalPlayer.TryGetModPlayer<TraitPlayer>(out var p))
        {
            if (Main.rand.NextFloat() < p.BodkinChance && projectile.penetrate != -1 && projectile.maxPenetrate != -1)
            {
                projectile.maxPenetrate++;
                projectile.penetrate++;
            }
        }
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(IsArrow);
        binaryWriter.Write7BitEncodedInt(projectile.penetrate);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        var arrow = bitReader.ReadBit();
        IsArrow = arrow || IsArrow;

        projectile.penetrate = binaryReader.Read7BitEncodedInt();
    }

    public override bool PreAI(Projectile projectile)
    {
        if (IsArrow)
            projectile.velocity *= Main.player[projectile.owner].GetModPlayer<TraitPlayer>().ProficiencySpeed + 1f;

        return base.PreAI(projectile);
    }

    public override void PostAI(Projectile projectile)
    {
        if (IsArrow)
            projectile.velocity /= Main.player[projectile.owner].GetModPlayer<TraitPlayer>().ProficiencySpeed + 1f;
    }
}