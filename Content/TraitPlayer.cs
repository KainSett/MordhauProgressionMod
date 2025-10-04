using Terraria.ModLoader;

namespace MordhauProgression.Content;

public class TraitPlayer : ModPlayer
{
    public override void PostUpdateMiscEffects()
    {
        foreach (var (role, row, index, tier, open, flash) in Player.GetModPlayer<UIPlayer>().SkillTree[Player.CurrentLoadoutIndex])
        {
            if (!open)
                continue;

            if (role == "Melee")
                UpdateMelee(row, index, tier);
            else if (role == "Ranger")
                UpdateRanger(row, index, tier);
            else if (role == "Mage")
                UpdateMage(row, index, tier);
            else if (role == "Summoner")
                UpdateSummoner(row, index, tier);
            else 
                UpdateOther(row, index, tier);
        }
    }

    public void UpdateMelee(int row, int index, int tier)
    {

    }

    public void UpdateRanger(int row, int index, int tier)
    {

    }

    public void UpdateMage(int row, int index, int tier)
    {

    }

    public void UpdateSummoner(int row, int index, int tier)
    {

    }

    public void UpdateOther(int row, int index, int tier)
    {

    }
}