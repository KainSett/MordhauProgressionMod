using Terraria.ModLoader;

namespace MordhauProgression.Content;

public class ArmorPlayer : ModPlayer
{
    public override void PostUpdateEquips()
    {
        for (int i = 0; i < 3; i++)
        {
            if (Player.armor[i] is null || Player.armor[i].IsAir)
            {
                Player.statLifeMax2 = (int)(Player.statLifeMax2 * 0.89f);
                Player.DefenseEffectiveness *= (Player.DefenseEffectiveness.Value * 0.95f) / Player.DefenseEffectiveness.Value;
                Player.moveSpeed += 0.05f;
                continue;
            }

            switch (Player.GetModPlayer<UIPlayer>().loadouts[Player.CurrentLoadoutIndex][i].tier)
            {
                case 3: 
                    Player.statLifeMax2 = (int)(Player.statLifeMax2 * 1.05f);
                    Player.DefenseEffectiveness *= (Player.DefenseEffectiveness.Value * 1.02f) / Player.DefenseEffectiveness.Value;
                    Player.moveSpeed -= 0.05f;
                    break;

                case 2:
                    Player.statLifeMax2 += 10;
                    Player.endurance += 0.02f;
                    Player.moveSpeed -= 0.02f;
                    break;

                case 1:
                    Player.statLifeMax2 -= 25;
                    Player.DefenseEffectiveness *= (Player.DefenseEffectiveness.Value * 0.98f) / Player.DefenseEffectiveness.Value;
                    Player.moveSpeed += 0.02f;
                    break;

                default:
                    Player.statLifeMax2 = (int)(Player.statLifeMax2 * 0.89f);
                    Player.DefenseEffectiveness *= (Player.DefenseEffectiveness.Value * 0.95f) / Player.DefenseEffectiveness.Value;
                    Player.moveSpeed += 0.05f;
                    break;
            }
        }
    }
}