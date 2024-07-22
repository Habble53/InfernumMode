﻿using System;
using CalamityMod.Events;
using CalamityMod.Particles;
using InfernumMode.Common.Graphics.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Cryogen
{
    public class AuroraSpirit : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Aurora Spirit");
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 28;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.Opacity = 0f;
            Projectile.extraUpdates = BossRushEvent.BossRushActive ? 1 : 0;
            
        }

        public override void AI()
        {
            // Handle frames.
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Projectile.type];

            Projectile.rotation = Projectile.velocity.ToRotation() + Pi;
            Projectile.spriteDirection = (Math.Cos(Projectile.rotation) < 0f).ToDirectionInt();
            if (Projectile.spriteDirection == 1)
                Projectile.rotation -= Pi;

            if (Time == 0f)
            {
                Time = Main.rand.Next(1, 46);
                Projectile.netUpdate = true;
            }

            // Periodically descend via a squashed, downward-only sine.
            // After enough time has passed this stops in favor of horizontal acceleration.
            if (Projectile.ai[1] == 0f)
            {
                if (Time < 110f)
                    Projectile.velocity.Y = Pow(Sin(Time / 29f), 10f) * 14.5f;
                else
                    Projectile.velocity.Y *= 0.93f;
            }

            // Accelerate after enough time has passed.
            if (Time > 60f && Math.Abs(Projectile.velocity.X) < 19.5f)
                Projectile.velocity.X *= 1.0065f;

            if (Time % 10 == 0)
            {
                // Leave a trail of particles.
                Particle iceParticle = new SnowyIceParticle(Projectile.Center, Projectile.velocity * 0.5f, Color.White, Main.rand.NextFloat(0.75f, 0.95f), 30);
                GeneralParticleHandler.SpawnParticle(iceParticle);
            }


            // Fade in.
            Projectile.Opacity = Clamp(Projectile.Opacity + 0.02f, 0f, 1f);

            Time++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color endColor = Color.Lerp(lightColor * Projectile.Opacity, Color.White, 0.55f);
            return Color.Lerp(new Color(128, 88, 160, 0) * 0.45f, endColor, Projectile.Opacity);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(Projectile, Color.White * Projectile.Opacity, ProjectileID.Sets.TrailingMode[Projectile.type]);
            return false;
        }
    }
}
