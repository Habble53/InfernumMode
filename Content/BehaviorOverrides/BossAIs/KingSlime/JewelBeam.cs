﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.KingSlime
{
    public class JewelBeam : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Beam");
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 12;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            
        }

        public override void AI()
        {
            // Decide frames.
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;

            if (Main.dedServ)
                return;

            // Release a burst of circular dust on the first frame.
            if (Projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 36; i++)
                {
                    Dust gleamingBurst = Dust.NewDustPerfect(Projectile.Center, 264);
                    gleamingBurst.velocity = (TwoPi * i / 36f).ToRotationVector2() * 2.5f;
                    gleamingBurst.color = Color.Red;
                    gleamingBurst.noLight = true;
                    gleamingBurst.noGravity = true;
                    gleamingBurst.fadeIn = 0.4f;
                    gleamingBurst.scale = 1.2f;
                }
                Projectile.localAI[0] = 1f;
            }

            Dust gleamingRed = Dust.NewDustPerfect(Projectile.Center, 182);
            gleamingRed.velocity = Vector2.Zero;
            gleamingRed.noGravity = true;
            gleamingRed.scale = 0.5f;

            for (int direction = -1; direction <= 1; direction += 2)
            {
                gleamingRed = Dust.NewDustPerfect(Projectile.Center, 182);
                gleamingRed.velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(Pi + direction * 0.53f).RotatedByRandom(0.06f) * Main.rand.NextFloat(0.9f, 1.1f) * 3f;
                gleamingRed.noGravity = true;
                gleamingRed.scale = Main.rand.NextFloat(0.8f, 0.96f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawProjectileWithBackglowTemp(Color.White with { A = 0 }, lightColor, 2f);
            return false;
        }
    }
}
