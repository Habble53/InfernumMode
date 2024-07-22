﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.MoonLord
{
    public class MoonLordExplosionCinder : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];
        public ref float Lifetime => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Cinder");
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 4;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            
        }

        public override void AI()
        {
            // Decide a frame to use on the first frame this projectile exists.
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.frame = Main.rand.Next(Main.projFrames[Projectile.type]);
                Projectile.localAI[0] = 1f;
            }

            // Make a decision for the lifetime for the cinder if one has not yet been made.
            if (Lifetime == 0f)
            {
                Lifetime = Main.rand.Next(40, 75);
                Projectile.netUpdate = true;
            }

            // Calculate scale of the cinder.
            else
            {
                Projectile.scale = Utils.GetLerpValue(0f, 20f, Time, true) * Utils.GetLerpValue(Lifetime, Lifetime - 20f, Time, true);
                Projectile.scale *= Lerp(0.8f, 1.6f, Projectile.identity % 6f / 6f);
            }

            if (Time >= Lifetime)
                Projectile.Kill();

            Time++;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;
    }
}
