using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MordhauProgression.Content;

public class BearTrap : ModProjectile
{
    public override string Texture => $"MordhauProgression/Common/Assets/{Name}";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 3;
    }

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 16;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Projectile.ai[0] = 1;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }

    public override void AI()
    {
        Projectile.velocity.Y += 0.3f;

        Projectile.frameCounter = (Projectile.frameCounter + 1) % 4;
        if (Projectile.ai[0] == 1)
        {
            if (Projectile.frameCounter == 0)
                Projectile.frame++;

            if (Projectile.frame == 2)
                Projectile.ai[0] = 2;
        }
        else if (Projectile.ai[0] == 2)
        {
            if (Projectile.frameCounter == 0)
                Projectile.frame--;

            if (Projectile.frame == 0)
                Projectile.ai[0] = 0;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type];

        var frame = texture.Frame(1, 3, 0, Projectile.frame);

        Main.spriteBatch.Draw(texture.Value, Projectile.Center - Main.screenPosition + new Vector2(0, 2), frame, lightColor, Projectile.rotation, frame.Size() / 2, Projectile.scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);

        return false;
    }
}