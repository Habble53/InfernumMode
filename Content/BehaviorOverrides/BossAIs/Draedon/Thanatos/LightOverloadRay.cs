﻿using System.Collections.Generic;
using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.Assets.Effects;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Common.Graphics.Interfaces;
using InfernumMode.Common.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Draedon.Thanatos
{
    public class LightOverloadRay : ModProjectile, IPixelPrimitiveDrawer
    {
        public PrimitiveTrailCopy LaserDrawer;
        public static NPC Thanatos => Main.npc[CalamityGlobalNPC.draedonExoMechWorm];
        public Vector2 StartingPosition => Thanatos.Center - (Thanatos.rotation - PiOver2).ToRotationVector2() * Projectile.Opacity * 5f;

        // This is only used in drawing to represent increments as a semi-hack. Don't mess with it.
        public float RayHue;

        public const int Lifetime = 45;
        public ref float Time => ref Projectile.ai[0];
        public ref float LaserSpread => ref Projectile.ai[1];
        public ref float LaserLength => ref Projectile.localAI[0];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Prismatic Overload Beam");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.hostile = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (CalamityGlobalNPC.draedonExoMechWorm == -1)
            {
                Projectile.Kill();
                return;
            }

            DelegateMethods.v3_1 = Vector3.One * 0.7f;
            Utils.PlotTileLine(StartingPosition, StartingPosition + (Thanatos.rotation - PiOver2).ToRotationVector2() * Projectile.Opacity * 400f, 8f, DelegateMethods.CastLight);

            Projectile.Opacity = Utils.GetLerpValue(0f, 10f, Time, true) * Utils.GetLerpValue(0f, 8f, Projectile.timeLeft, true);
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = StartingPosition;
            LaserLength = Lerp(LaserLength, 9000f, 0.1f);
            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.Opacity < 1f)
                return false;

            for (int i = 0; i < 45; i++)
            {
                float _ = 0f;
                float offsetAngle = Lerp(-LaserSpread, LaserSpread, i / 44f);
                Vector2 start = StartingPosition;
                Vector2 end = start + (Thanatos.rotation - PiOver2 + offsetAngle).ToRotationVector2() * LaserLength;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.scale * 50f, ref _))
                    return true;
            }
            return false;
        }

        public float LaserWidthFunction(float completionRatio) => Projectile.Opacity * Utils.GetLerpValue(0.96f, 0.8f, completionRatio, true) * 45f;

        public Color LaserColorFunction(float completionRatio)
        {
            float hue = (RayHue * 2.4f + Main.GlobalTimeWrappedHourly * 0.77f) % 1f;
            Color color = LumUtils.MulticolorLerp(hue, CalamityUtils.ExoPalette);
            color = Color.Lerp(color, Color.Wheat, 0.4f) * Projectile.Opacity;
            color *= Utils.GetLerpValue(0.96f, 0.8f, completionRatio, true);
            return color;
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            spriteBatch.SetBlendState(BlendState.Additive);

            LaserDrawer ??= new PrimitiveTrailCopy(LaserWidthFunction, LaserColorFunction, null, true, InfernumEffectsRegistry.FireVertexShader);

            InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.14f);
            InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);

            List<float> rotationPoints = [];
            List<Vector2> drawPoints = [];

            var oldBlendState = Main.instance.GraphicsDevice.BlendState;
            Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
            for (int i = 0; i < 45; i++)
            {
                RayHue = i / 44f;
                rotationPoints.Clear();
                drawPoints.Clear();

                float offsetAngle = Thanatos.rotation - PiOver2 + Lerp(-LaserSpread * Projectile.Opacity, LaserSpread * Projectile.Opacity, i / 44f);
                for (int j = 0; j < 8; j++)
                {
                    rotationPoints.Add(offsetAngle);
                    Vector2 start = StartingPosition;
                    Vector2 end = start + offsetAngle.ToRotationVector2() * LaserLength;
                    drawPoints.Add(Vector2.Lerp(start, end, j / 8f));
                }

                LaserDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 20);
                LaserDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 20);
            }
            Main.instance.GraphicsDevice.BlendState = oldBlendState;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(Projectile.Opacity, Projectile.Opacity, Projectile.Opacity, 0);
    }
}
