﻿using CalamityMod.Events;
using CalamityMod.NPCs;
using InfernumMode.Assets.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Signus
{
    public class CosmicMine : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Cosmic Mine");
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 34;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = BossRushEvent.BossRushActive ? 1 : 0;
            
        }

        public override void AI()
        {
            if (CalamityGlobalNPC.signus == -1)
            {
                Projectile.active = false;
                return;
            }

            Projectile.scale = Utils.GetLerpValue(30f, 60f, Time, true) + 1f;
            Time++;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, Projectile.alpha);

        public override bool PreDraw(ref Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(Projectile, lightColor, ProjectileID.Sets.TrailingMode[Projectile.type], 1);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(InfernumSoundRegistry.SignusKunaiExplosionSound, Projectile.Center);

            for (int i = 0; i < 50; i++)
            {
                Vector2 shootVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(18f, 85f);
                Utilities.NewProjectileBetter(Projectile.Center + shootVelocity * 3f, shootVelocity, ModContent.ProjectileType<CosmicKunai>(), SignusBehaviorOverride.KunaiDamage, 0f);
            }
        }
    }
}
