﻿using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class ShadowFlameBlast : ModProjectile
    {
        public const int Lifetime = 32;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Shadow Blast");
            Main.projFrames[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 52;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.Opacity = 0f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.Opacity = Clamp(Projectile.Opacity + 0.2f, 0f, 1f);

            Projectile.velocity *= 0.995f;
            Projectile.frameCounter++;
            Projectile.frame = (int)Math.Ceiling((1f - Projectile.timeLeft / (float)Lifetime) * 4f);
            Projectile.rotation = Projectile.velocity.ToRotation() - PiOver2;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item104, Projectile.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 6; i++)
            {
                Vector2 shadowSparkVelocity = (TwoPi * i / 6f).ToRotationVector2() * 7f;
                Utilities.NewProjectileBetter(Projectile.Center, shadowSparkVelocity, ModContent.ProjectileType<ShadowSpark>(), SupremeCalamitasBehaviorOverride.ShadowBoltDamage, 0f);
            }
        }
    }
}
