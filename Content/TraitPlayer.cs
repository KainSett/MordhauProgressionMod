using Microsoft.Xna.Framework;
using MordhauProgression.Content.UI;
using ReLogic.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MordhauProgression.Content;

public class TraitPlayer : ModPlayer
{
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var chance = 0f;
        if (!target.active && target.boss)
            chance = 0.5f;

        else if (!target.active && target.rarity > 1)
            chance = 0.1f;

        if (Main.rand.NextFloat() < chance)
        {
            Item.NewItem(target.GetItemSource_Loot(), target.Center, new Item(ModContent.ItemType<PointCoin>()));
        }
    }

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
        MedicalHeal = 0;
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

    public int WellCounter = 0;

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
                WellCounter = (WellCounter + 1) % 60;
                if (WellCounter == 0)
                {
                    Player.statMana = Math.Min(Player.statManaMax2, Player.statMana + 3 * tier);
                }
                break;

            case "Expert Casting":
                ExpertSpeed += 0.1f * tier;
                break;
        }
    }

    public float CharismaSpeed = 0;

    public int NurturingNum = 0;

    public int TrapsNum = 0;

    public int MedicalHeal = 0;

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
                MedicalHeal = tier;
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

        else if (modifiers.DamageType == DamageClass.Ranged)
            modifiers.CritDamage += LuckyDamage;
    }

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (proj.aiStyle == ProjAIStyleID.Arrow && proj.TryGetGlobalProjectile<TraitProj>(out var p) && p.IsArrow)
        {
            modifiers.FinalDamage.Flat += YeomenDamage;

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

        if (Main.rand.NextFloat() < PlotChance && Main.myPlayer == Player.whoAmI)
        {
            for (int i = Main.rand.Next(1, 4); i > 0; i--) 
            {
                var pos = Player.Center - new Vector2(0, Main.screenHeight / 1.5f).RotatedByRandom(PiOver4 / 2);
                Projectile.NewProjectile(Player.GetSource_OnHurt(info.DamageSource), pos, pos.DirectionTo(Player.Center).RotatedByRandom(PiOver4 / 2), ProjectileID.Meteor1 + Main.rand.Next(3), 50, 4);
            }
        }
    }

    public override void Load()
    {
        On_Player.Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float += AdrenalineHurt;
        On_Player.ManaEffect += DeterminationMana;
        On_Player.AddBuff_DetermineBuffTimeToAdd += AlchemyTime;
    }

    private static int AlchemyTime(On_Player.orig_AddBuff_DetermineBuffTimeToAdd orig, Player self, int type, int time1)
    {
        if (type == BuffID.PotionSickness)
            time1 = (int)(time1 * (1 - self.GetModPlayer<TraitPlayer>().AlchemyNum));

        return orig(self, type, time1);
    }

    private static void DeterminationMana(On_Player.orig_ManaEffect orig, Player self, int manaAmount)
    {
        if (self.TryGetModPlayer<TraitPlayer>(out var p) && p.DeterminationHeal > 0)
            self.Heal(self.GetModPlayer<TraitPlayer>().DeterminationHeal);

        orig(self, manaAmount);
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

    public override void ModifyLuck(ref float luck)
    {
        luck += LuckNum;
    }

    public List<int> NurturedMinions = [];
    #endregion
}

public class NurtureItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public override bool? UseItem(Item item, Player player)
    {
        if (item.DamageType == DamageClass.Summon && item.buffType != 0 && player.TryGetModPlayer<TraitPlayer>(out var p))
        {
            p.NurturedMinions = [-1, -1];
            for (int i = 0; i < p.NurturingNum; i++) 
            {
                var proj = Projectile.NewProjectileDirect(item.GetSource_FromThis(), player.Center - new Vector2(0, 20), Vector2.Zero, item.shoot, item.damage / 2, item.knockBack / 2);
                proj.minionSlots = 0;
                p.NurturedMinions[i] = (proj.identity);
            } 
        }

        return base.UseItem(item, player);
    }
}

public class TraitProj : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public bool IsArrow = false;

    public bool IsBullet = false;

    public bool IsNurtured = false;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is EntitySource_ItemUse_WithAmmo)
        {
            if ((source as EntitySource_ItemUse_WithAmmo).AmmoItemIdUsed == AmmoID.Arrow)
                IsArrow = true;

            else if ((source as EntitySource_ItemUse_WithAmmo).AmmoItemIdUsed == AmmoID.Bullet)
                IsBullet = true;
        }

        if (Main.LocalPlayer.TryGetModPlayer<TraitPlayer>(out var p))
        {
            if (projectile.DamageType == DamageClass.Ranged && Main.rand.NextFloat() < p.BodkinChance && projectile.penetrate != -1 && projectile.maxPenetrate != -1)
            {
                projectile.maxPenetrate++;
                projectile.penetrate++;
            }

            else if (projectile.DamageType == DamageClass.Magic)
            {
                var scale = 1 + p.TalentSize;
                projectile.Resize((int)(projectile.width * scale), (int)(projectile.height * scale));
                projectile.scale *= scale;
            }

            else if (projectile.sentry)
            {
                for (int i = 0; i < p.TrapsNum; i++)
                    SpawnTrap(projectile);
            }
        }
    }

    public void SpawnTrap(Projectile projectile)
    {
        traps.Add(Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), projectile.Center + new Vector2(Main.rand.NextFloat() * 300 - 150, -100), Vector2.Zero, ModContent.ProjectileType<BearTrap>(), 15, 2).identity);
    }


    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(IsArrow);
        bitWriter.WriteBit(IsBullet);
        binaryWriter.Write7BitEncodedInt(projectile.penetrate);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        var arrow = bitReader.ReadBit();
        IsArrow = arrow || IsArrow;
        var bullet = bitReader.ReadBit();
        IsBullet = bullet || IsBullet;

        projectile.penetrate = binaryReader.Read7BitEncodedInt();
    }

    public int counter = 120;

    public int timer = 0;

    public override bool PreAI(Projectile projectile)
    {
        var p = Main.player[projectile.owner].GetModPlayer<TraitPlayer>();
        if (IsArrow)
            projectile.velocity *= p.ProficiencySpeed + 1f;

        if (projectile.minion)
            projectile.velocity *= p.CharismaSpeed + 1f;


        if (IsBullet)
        {
            counter = Math.Max(-1, counter - 1);
            if (counter % 60 == 0)
                projectile.damage += p.SniperDamage / 2;
        }

        if (projectile.DamageType == DamageClass.Magic)
        {
            projectile.velocity *= p.ExpertSpeed + 1f;
        }

        if (traps.Count > 0)
            foreach (var proj in Array.FindAll(Main.projectile, proj => traps.Contains(proj.identity)))
                proj.timeLeft = 50;

        else if (projectile.sentry)
            for (int i = 0; i < p.TrapsNum; i++)
                SpawnTrap(projectile);

        timer = (timer + 1) % 60;

        if (timer == 0 && p.MedicalHeal > 0)
            foreach (var player in Array.FindAll(Main.player, pl => pl.active && pl.Center.DistanceSQ(projectile.Center) <= 600 * 600))
                player.Heal(p.MedicalHeal);

        if (p.NurturedMinions.Contains(projectile.identity))
        {
            if (!IsNurtured)
            {
                var scale = 0.75f;
                projectile.Resize((int)(projectile.width * scale), (int)(projectile.height * scale));
                projectile.scale *= scale;
            }
            IsNurtured = true;
            projectile.minionSlots = 0;
        }
        else if (IsNurtured)
            projectile.Kill();

        return base.PreAI(projectile);
    }

    public override void PostAI(Projectile projectile)
    {
        var p = Main.player[projectile.owner].GetModPlayer<TraitPlayer>();
        if (IsArrow)
            projectile.velocity /= p.ProficiencySpeed + 1f;

        if (projectile.minion)
            projectile.velocity /= p.CharismaSpeed + 1f;

        if (projectile.DamageType == DamageClass.Magic)
        {
            projectile.velocity /= p.ExpertSpeed + 1f;
        }
    }

    public List<int> traps = [];

    public override void OnKill(Projectile projectile, int timeLeft)
    {
        if (traps.Count > 0)
            foreach (var p in Array.FindAll(Main.projectile, proj => traps.Contains(proj.identity)))
            {
                traps.Remove(p.identity);
                p.Kill();
            }
    }
}

public class HerbologyTile : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (type != TileID.BloomingHerbs)
            return;

        var dist = 900f * 900f;
        var who = -1;
        foreach (var pl in Main.ActivePlayers)
        {
            if (pl.Center.DistanceSQ(new Point(i, j).ToWorldCoordinates()) > dist)
                continue;

            dist = pl.Center.DistanceSQ(new Point(i, j).ToWorldCoordinates());
            who = pl.whoAmI;
        }

        if (who >= 0 && Main.player[who].TryGetModPlayer<TraitPlayer>(out var p) && Main.rand.NextFloat() < p.HerbologyChance)
        {
            for (int x = 0; x < 100; x++)
            {
                WorldGen.KillTile_GetItemDrops(i, j, Main.tile[i, j], out var item, out var stack, out var item2, out var stack2);

                if (item > 0)
                {
                    int num = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, item, 1, noBroadcast: false, -1);
                    Main.item[num].TryCombiningIntoNearbyItems(num);
                    break;
                }
            }
        } 
    }
}