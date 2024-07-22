﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Plantera
{
    public class Petal : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.SeedPlantera}";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Petal");
            Main.projFrames[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
