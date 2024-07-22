﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Cultist
{
    public class CultistFireBeamTelegraph : ModProjectile
    {
        public ref float TargetIndex => ref Projectile.ai[0];
        public Player Target => Main.player[(int)TargetIndex];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        // public override void SetStaticDefaults() => DisplayName.SetDefault("Telegraph");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 85;
            Projectile.penetrate = -1;
            
        }

        public override void AI()
        {
            // Determine an initial target.
            if (Main.netMode != NetmodeID.MultiplayerClient && Projectile.localAI[0] == 0f)
            {
                TargetIndex = Player.FindClosest(Projectile.Center, 1, 1);
                Projectile.localAI[0] = 1f;
                Projectile.netUpdate = true;
            }

            Projectile.scale = Utils.GetLerpValue(0f, 10f, Projectile.timeLeft, true) * Utils.GetLerpValue(85f, 75f, Projectile.timeLeft, true);
            Projectile.scale = SmoothStep(0.04f, 4f, Projectile.scale);

            // Try to aim at the target.
            if (Projectile.timeLeft > 32f)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(Target.Center), 0.15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Vector2 aimDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + aimDirection * FireBeam.LaserLength, Color.Orange, Color.OrangeRed, Projectile.scale);
            Main.spriteBatch.ResetBlendState();
            return false;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Zombie104, Target.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            float aimDirection = (WrapAngle(Projectile.AngleTo(Target.Center) - Projectile.velocity.ToRotation()) > 0f).ToDirectionInt();
            Vector2 beamDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Utilities.NewProjectileBetter(Projectile.Center, beamDirection, ModContent.ProjectileType<FireBeam>(), CultistBehaviorOverride.FireBeamDamage, 0f, -1, 0f, aimDirection * 0.0277f);
        }
    }
}
