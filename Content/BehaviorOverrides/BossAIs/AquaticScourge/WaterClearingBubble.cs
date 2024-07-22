﻿using System;
using System.Collections.Generic;
using CalamityMod.Systems;
using InfernumMode.Assets.Effects;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Assets.Sounds;
using InfernumMode.Common.Graphics.Interfaces;
using InfernumMode.Common.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class WaterClearingBubble : ModProjectile, IPixelPrimitiveDrawer
    {
        public PrimitiveTrailCopy WaterDrawer;

        public ref float Time => ref Projectile.ai[0];

        public ref float Lifetime => ref Projectile.ai[1];

        public static float Radius => 120f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // public override void SetStaticDefaults() => DisplayName.SetDefault("Acid Bubble");

        public override void Load() => Terraria.GameContent.Liquid.On_LiquidRenderer.DrawNormalLiquids += PrepareWater;

        public override void Unload() => Terraria.GameContent.Liquid.On_LiquidRenderer.DrawNormalLiquids -= PrepareWater;

        public static void PrepareWater(Terraria.GameContent.Liquid.On_LiquidRenderer.orig_DrawNormalLiquids orig, Terraria.GameContent.Liquid.LiquidRenderer self, SpriteBatch spriteBatch, Vector2 drawOffset, int waterStyle, float globalAlpha, bool isBackgroundDraw)
        {
            ClaimAllBubbles();
            orig(self, spriteBatch, drawOffset, waterStyle, globalAlpha, isBackgroundDraw);
        }

        public static void ClaimAllBubbles()
        {
            // Make the nearby water clear.
            SulphuricWaterSafeZoneSystem.NearbySafeTiles.Clear();
            foreach (Projectile bubble in Utilities.AllProjectilesByID(ModContent.ProjectileType<WaterClearingBubble>()))
            {
                if (bubble.Opacity <= 0f || !bubble.WithinRange(Main.LocalPlayer.Center, 2000f))
                    continue;

                Point p = bubble.Center.ToTileCoordinates();
                float power = 0f;
                if (SulphuricWaterSafeZoneSystem.NearbySafeTiles.TryGetValue(p, out float s))
                    power = s;

                SulphuricWaterSafeZoneSystem.NearbySafeTiles[p] = MathF.Max(power, bubble.scale);
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = (int)Radius;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.timeLeft = 7200;
            
        }

        public override void AI()
        {
            // Initialize the lifetime of the bubble if nothing is inputted.
            if (Lifetime <= 0f)
            {
                Lifetime = 240f;
                Projectile.netUpdate = true;
            }

            Projectile.Opacity = LumUtils.Convert01To010(Time / Lifetime) * 4f;
            if (Projectile.Opacity > 1f)
                Projectile.Opacity = 1f;
            Projectile.scale = Projectile.Opacity;

            // Release positive golden/cyan particles.
            Dust positiveParticle = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(220f, 220f) * Projectile.scale, 261);
            positiveParticle.color = Main.rand.NextBool() ? Color.Gold : Color.Cyan;
            positiveParticle.velocity = -Vector2.UnitY * Main.rand.NextFloat(0.5f, 3f);
            positiveParticle.scale = 1.5f;
            positiveParticle.noGravity = true;

            // Randomly emit bubbles.
            Vector2 bubbleSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(250f, 250f) * Projectile.scale;
            bubbleSpawnPosition += Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(PiOver2) * Main.rand.NextFloatDirection() * 14f;
            if (!Main.rand.NextBool(3))
            {
                for (int i = 0; i < 7; i++)
                {
                    Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), bubbleSpawnPosition, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f) * 0.75f, 411);
                    bubble.timeLeft = Main.rand.Next(8, 14);
                    bubble.scale = Main.rand.NextFloat(0.8f, 1.2f);
                    bubble.type = Main.rand.NextBool(3) ? 412 : 411;
                }
            }

            Time++;
            if (Time >= Lifetime)
                Projectile.Kill();
        }

        public float WidthFunction(float completionRatio) => Radius * Projectile.scale * LumUtils.Convert01To010(completionRatio);

        public Color ColorFunction(float completionRatio)
        {
            float colorInterpolant = Pow(Math.Abs(Sin(completionRatio * Pi + Main.GlobalTimeWrappedHourly)), 3f) * 0.5f;
            return Color.Lerp(new Color(103, 218, 224), new Color(144, 114, 166), colorInterpolant) * Projectile.Opacity * 0.3f;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(InfernumSoundRegistry.BubblePop with { Pitch = -0.3f, Volume = 1.3f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bubble = InfernumTextureRegistry.Bubble.Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color bubbleColor = Projectile.GetAlpha(Color.Lerp(Color.DeepSkyBlue, Color.Wheat, 0.4f)) * 0.9f;
            Vector2 bubbleScale = Vector2.One * (Projectile.scale * 0.8f + Cos(Main.GlobalTimeWrappedHourly * 1.1f + Projectile.identity) * 0.04f);

            // Make the bubble scale squish a bit in one of the four cardinal directions for more a fluid aesthetic.
            Vector2 scalingDirection = -Vector2.UnitY.RotatedBy(Projectile.identity % 4 / 4f * TwoPi);
            bubbleScale += scalingDirection * (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 3.1f + Projectile.identity) * 0.5f + 0.5f) * 0.16f;

            Main.EntitySpriteDraw(bubble, drawPosition, null, bubbleColor with { A = 0 }, Projectile.rotation, bubble.Size() * 0.5f, bubbleScale, 0, 0);
            return false;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            WaterDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.DukeTornadoVertexShader);

            InfernumEffectsRegistry.DukeTornadoVertexShader.UseImage1("Images/Misc/Perlin");
            List<Vector2> drawPoints = [];

            for (float offsetAngle = -PiOver2; offsetAngle <= PiOver2; offsetAngle += Pi / 6f)
            {
                drawPoints.Clear();

                float adjustedAngle = offsetAngle + Main.GlobalTimeWrappedHourly * 1.2f;
                Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
                Vector2 radius = Vector2.One * Radius;
                radius.Y *= Lerp(1f, 2f, Math.Abs(Cos(Main.GlobalTimeWrappedHourly * 1.1f)));

                for (int i = 0; i <= 8; i++)
                {
                    drawPoints.Add(Vector2.Lerp(Projectile.Center - offsetDirection * radius * 0.8f, Projectile.Center + offsetDirection * radius * 0.8f, i / 8f));
                }

                WaterDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 12, adjustedAngle);
            }
        }
    }
}
