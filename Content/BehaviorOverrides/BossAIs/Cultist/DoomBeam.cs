﻿using System.Collections.Generic;
using CalamityMod;
using InfernumMode.Assets.Effects;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Common.Graphics.Interfaces;
using InfernumMode.Common.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Cultist
{
    public class DoomBeam : ModProjectile, IPixelPrimitiveDrawer
    {
        internal PrimitiveTrailCopy BeamDrawer;

        public ref float Time => ref Projectile.ai[0];

        public const int Lifetime = 105;

        public const float LaserLength = 4000f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // public override void SetStaticDefaults() => DisplayName.SetDefault("Death Beam");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 48;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 255;
            Projectile.Calamity().DealsDefenseDamage = true;
            
        }

        public override void AI()
        {
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // Fade in.
            Projectile.alpha = Utils.Clamp(Projectile.alpha - 25, 0, 255);

            Projectile.scale = LumUtils.Convert01To010(Time / Lifetime) * 3f;
            if (Projectile.scale > 1f)
                Projectile.scale = 1f;

            // And create bright light.
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 1.4f);

            CreateDustAtBeginning();
            Projectile.Center -= Projectile.velocity;

            Time++;
        }

        public void CreateDustAtBeginning()
        {
            for (int i = 0; i < 4; i++)
            {
                Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), 223);
                fire.velocity = -Vector2.UnitY * Main.rand.NextFloat(2.5f, 5.25f);
                fire.scale = 1f + fire.velocity.Length() * 0.1f;
                fire.color = Color.Lerp(Color.White, Color.OrangeRed, Main.rand.NextFloat());
                fire.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = Projectile.width * 0.8f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + Projectile.velocity * (LaserLength - 80f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public float WidthFunction(float completionRatio)
        {
            float squeezeInterpolant = Utils.GetLerpValue(0f, 0.03f, completionRatio, true) * Utils.GetLerpValue(1f, 0.97f, completionRatio, true);
            return SmoothStep(2f, Projectile.width, squeezeInterpolant) * Clamp(Projectile.scale, 0.04f, 1f);
        }

        public Color ColorFunction(float completionRatio)
        {
            Color color = Color.Lerp(Color.DarkViolet, new(117, 255, 160), Sin(TwoPi * completionRatio * 10f - Main.GlobalTimeWrappedHourly * 1.37f) * 0.5f + 0.5f);
            return color * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            BeamDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.FireVertexShader);

            InfernumEffectsRegistry.FireVertexShader.UseSaturation(1.4f);
            InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);

            List<float> originalRotations = [];
            List<Vector2> points = [];
            for (int i = 0; i <= 8; i++)
            {
                points.Add(Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, i / 8f));
                originalRotations.Add(PiOver2);
            }

            if (Time >= 2f)
                BeamDrawer.DrawPixelated(points, Projectile.Size * 0.5f - Main.screenPosition, 67);
        }
    }
}
