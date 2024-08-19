﻿using System.Collections.Generic;
using InfernumMode.Assets.Effects;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class AresEnergySlash : ModProjectile
    {
        public Vector2[] ControlPoints;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Exo Energy Slash");
        }

        public override void SetDefaults()
        {
            Projectile.width = 180;
            Projectile.height = 180;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.Opacity = Utils.GetLerpValue(0f, 56f, Projectile.timeLeft, true);
            Projectile.velocity *= 1.06f;

            if (Projectile.timeLeft >= 30)
                Projectile.scale *= 1.033f;
        }

        public float SlashWidthFunction(float completionRatio) => Utils.GetLerpValue(0f, 0.35f, completionRatio, true) * Utils.GetLerpValue(1f, 0.65f, completionRatio, true) * Projectile.scale * 35f;

        public Color SlashColorFunction(float completionRatio) => Color.Red with { A = 0 } * Utils.GetLerpValue(0.04f, 0.27f, completionRatio, true) * Projectile.Opacity * Projectile.localAI[1];

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw the slash effect.
            Main.spriteBatch.EnterShaderRegion();

            List<Vector2> points = [];
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 perpendicularDirection = direction.RotatedBy(PiOver2);
            Vector2 left = Projectile.Center - perpendicularDirection * Projectile.height * Projectile.scale * 0.5f;
            Vector2 right = Projectile.Center + perpendicularDirection * Projectile.height * Projectile.scale * 0.5f;
            Vector2 middle = Projectile.Center + direction * Projectile.height / Projectile.scale * 2f;
            for (int i = 0; i < 15; i++)
                points.Add(Utilities.QuadraticBezier(left, middle, right, i / 14f));

            Main.instance.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes").Value;
            Main.instance.GraphicsDevice.Textures[2] = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SwordSlashTexture").Value;

            for (Projectile.localAI[1] = 1f; Projectile.localAI[1] > 0f; Projectile.localAI[1] -= 0.33f)
                PrimitiveRenderer.RenderTrail(points, new(SlashWidthFunction, SlashColorFunction, _ => direction * -60f, Shader: InfernumEffectsRegistry.AresEnergySlashShader), 43);

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size());
    }
}
