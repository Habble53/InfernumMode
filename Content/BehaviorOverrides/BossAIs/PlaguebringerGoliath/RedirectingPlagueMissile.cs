﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.PlaguebringerGoliath
{
    public class RedirectingPlagueMissile : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];
        public Player Target => Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
        // public override void SetStaticDefaults() => DisplayName.SetDefault("Missile");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 210;
            
        }

        public override void AI()
        {
            Projectile.Opacity = Utils.GetLerpValue(0f, 12f, Time, true);
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
            if (Projectile.Hitbox.Intersects(Target.Hitbox))
                Projectile.Kill();

            // Emit smoke effects.
            EmitSmoke(Projectile);

            if (Time < 30f)
                Projectile.velocity *= 1.01f;

            if (Time >= 30f)
            {
                float newSpeed = Clamp(Projectile.velocity.Length() * 1.003f, 11f, 18f);
                Projectile.velocity = (Projectile.velocity * 29f + Projectile.SafeDirectionTo(Target.Center) * newSpeed) / 30f;
            }

            Projectile.tileCollide = Time > 105f;
            Time++;
        }

        public static void EmitSmoke(Projectile projectile)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Vector2 currentDirection = projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 endOfRocket = projectile.Center - currentDirection * 36f;
            Vector2 smokeVelocity = -currentDirection.RotatedByRandom(0.93f) * Main.rand.NextFloat(1f, 7f);

            Dust smokeDust = Dust.NewDustPerfect(endOfRocket, 31);
            smokeDust.velocity = smokeVelocity * Main.rand.NextFloat(0.3f, 1.1f) + projectile.velocity;
            smokeDust.scale *= 0.92f;
            smokeDust.fadeIn = -0.2f;
            smokeDust.noGravity = true;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Utilities.NewProjectileBetter(Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LargePlagueExplosion>(), PlaguebringerGoliathBehaviorOverride.ExplosionDamage, 0f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowmask = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/PlaguebringerGoliath/PlagueMissileGlowmask").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // Draw afterimages.
            for (int i = 0; i < 3; i++)
            {
                Vector2 afterimageOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * i * -16f;
                Color afterimageColor = Color.Lime * (1f - i / 3f) * 0.7f;
                afterimageColor.A = 0;
                Main.spriteBatch.Draw(texture, drawPosition + afterimageOffset, null, Projectile.GetAlpha(afterimageColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
            }

            Projectile.DrawProjectileWithBackglowTemp(Color.White with { A = 0 }, lightColor, 4f);
            Main.spriteBatch.Draw(glowmask, drawPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor) => lightColor * Projectile.Opacity;

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of false */ => Projectile.Opacity >= 0.8f;
    }
}
