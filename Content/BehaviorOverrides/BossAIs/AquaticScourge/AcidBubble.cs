﻿using System;
using System.Collections.Generic;
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
    public class AcidBubble : ModProjectile, IPixelPrimitiveDrawer
    {
        public PrimitiveTrailCopy WaterDrawer;

        public ref float Time => ref Projectile.ai[0];

        public static int Lifetime => 240;

        public static float Radius => 60f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = (int)Radius;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.Infernum().FadesAwayWhenManuallyKilled = true;
            
        }

        public override void AI()
        {
            Projectile.Opacity = LumUtils.Convert01To010(Projectile.timeLeft / (float)Lifetime) * 3.6f;
            if (Projectile.Opacity > 1f)
                Projectile.Opacity = 1f;
            Projectile.scale = Projectile.Opacity * Lerp(0.6f, 1f, Projectile.identity * Pi % 1f);

            // Randomly emit bubbles.
            Vector2 bubbleSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(120f, 120f) * Projectile.scale;
            bubbleSpawnPosition += Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(PiOver2) * Main.rand.NextFloatDirection() * 14f;
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), bubbleSpawnPosition, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f) * 0.75f, 411);
                    bubble.timeLeft = Main.rand.Next(8, 14);
                    bubble.scale = Main.rand.NextFloat(0.5f, 0.5f);
                    bubble.type = Main.rand.NextBool(3) ? 422 : 421;
                }
            }

            Time++;
        }

        public float WidthFunction(float completionRatio) => Radius * Projectile.scale * LumUtils.Convert01To010(completionRatio);

        public Color ColorFunction(float completionRatio)
        {
            float colorInterpolant = Pow(Math.Abs(Sin(completionRatio * Pi + Main.GlobalTimeWrappedHourly)), 3f) * 0.5f;
            return Color.Lerp(new Color(140, 234, 87), new Color(144, 114, 166), colorInterpolant) * Projectile.Opacity * 0.3f;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(InfernumSoundRegistry.BubblePop, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bubble = InfernumTextureRegistry.Bubble.Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color bubbleColor = Projectile.GetAlpha(Color.Lerp(Color.YellowGreen, Color.Wheat, 0.75f)) * 0.7f;
            Vector2 bubbleScale = Vector2.One * (Projectile.scale * 0.3f + Cos(Main.GlobalTimeWrappedHourly * 1.1f + Projectile.identity) * 0.025f);

            // Make the bubble scale squish a bit in one of the four cardinal directions for more a fluid aesthetic.
            Vector2 scalingDirection = -Vector2.UnitY.RotatedBy(Projectile.identity % 4 / 4f * TwoPi);
            bubbleScale += scalingDirection * (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 3.1f + Projectile.identity) * 0.5f + 0.5f) * 0.07f;
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Main.EntitySpriteDraw(bubble, drawPosition, null, bubbleColor, Projectile.rotation, bubble.Size() * 0.5f, bubbleScale, 0, 0);
            Main.spriteBatch.ResetBlendState();
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

                float adjustedAngle = offsetAngle + Main.GlobalTimeWrappedHourly * 2.2f;
                Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
                Vector2 radius = Vector2.One * Radius;
                radius.Y *= Lerp(1f, 2f, Math.Abs(Cos(Main.GlobalTimeWrappedHourly * 1.9f)));

                for (int i = 0; i <= 8; i++)
                    drawPoints.Add(Vector2.Lerp(Projectile.Center - offsetDirection * radius * 0.8f, Projectile.Center + offsetDirection * radius * 0.8f, i / 8f));

                WaterDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 15, adjustedAngle);
            }
        }
    }
}
