﻿using CalamityMod;
using InfernumMode.Assets.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ArtemisBasicShotLaser : ModProjectile
    {
        public NPC ThingToAttachTo => Main.npc.IndexInRange((int)Projectile.ai[0]) ? Main.npc[(int)Projectile.ai[0]] : null;

        public const int Lifetime = 30;

        public const float LaserLength = 2300f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 4;

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.MaxUpdates = 5;
            Projectile.Calamity().DealsDefenseDamage = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.scale = LumUtils.Convert01To010(Projectile.timeLeft / (float)Lifetime) * 1.2f;
            if (Projectile.scale > 1f)
                Projectile.scale = 1f;
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;

            // Stick to Artemis.
            float positionOffset = ExoMechManagement.ExoTwinsAreInSecondPhase ? 102f : 70f;
            Projectile.Center = ThingToAttachTo.Center + (ThingToAttachTo.rotation - PiOver2).ToRotationVector2() * positionOffset;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, Projectile.width * Projectile.scale, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw the telegraph line.
            Vector2 start = Projectile.Center - Main.screenPosition;
            Texture2D line = InfernumTextureRegistry.BloomLine.Value;
            Main.spriteBatch.SetBlendState(BlendState.Additive);

            Vector2 beamOrigin = new(line.Width / 2f, line.Height);
            Vector2 beamScale = new(Projectile.scale * Projectile.width / line.Width * 1.5f, LaserLength / line.Height);
            Main.spriteBatch.Draw(line, start, null, Color.Orange, Projectile.rotation, beamOrigin, beamScale, 0, 0f);
            Main.spriteBatch.Draw(line, start, null, Color.Red, Projectile.rotation, beamOrigin, beamScale * new Vector2(0.7f, 1f), 0, 0f);
            Main.spriteBatch.Draw(line, start, null, Color.Lerp(Color.OrangeRed, Color.White, 0.6f), Projectile.rotation, beamOrigin, beamScale * new Vector2(0.3f, 1f), 0, 0f);

            // Draw the energy focus at the start.
            Texture2D energyFocusTexture = InfernumTextureRegistry.LaserCircle.Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(energyFocusTexture, drawPosition, null, Color.White * Projectile.scale, Projectile.rotation, energyFocusTexture.Size() * 0.5f, 0.7f, 0, 0f);
            Main.spriteBatch.ResetBlendState();
            return false;
        }
    }
}
