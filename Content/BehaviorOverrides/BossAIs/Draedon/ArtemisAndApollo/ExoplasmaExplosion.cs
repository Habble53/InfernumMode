﻿using System.Collections.Generic;
using System.IO;
using CalamityMod;
using InfernumMode.Assets.Effects;
using InfernumMode.Common.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ExoplasmaExplosion : ModProjectile
    {
        public float MaxRadius;

        public PrimitiveTrailCopy FireDrawer;

        public ref float Time => ref Projectile.ai[0];

        public ref float Radius => ref Projectile.ai[1];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // public override void SetStaticDefaults() => DisplayName.SetDefault("Exoplasma Explosion");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 84;
            Projectile.MaxUpdates = 3;
            Projectile.scale = 1f;
            Projectile.hide = true;
            Projectile.Calamity().DealsDefenseDamage = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(MaxRadius);

        public override void ReceiveExtraAI(BinaryReader reader) => MaxRadius = reader.ReadSingle();

        public override void AI()
        {
            Projectile.scale += 0.08f;
            Radius = Lerp(Radius, MaxRadius, 0.1f);
            Projectile.Opacity = Utils.GetLerpValue(8f, 42f, Projectile.timeLeft, true) * 0.55f;

            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Utilities.CircularCollision(targetHitbox.Center.ToVector2(), projHitbox, Radius * 0.8f);

        public float SunWidthFunction(float completionRatio) => Radius * Sin(Pi * completionRatio);

        public Color SunColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Lime, Color.White, Sin(Pi * completionRatio) * 0.5f + 0.3f) * Projectile.Opacity;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override bool? CanDamage() => Projectile.Opacity >= 0.37f;

        public override bool PreDraw(ref Color lightColor)
        {
            FireDrawer ??= new PrimitiveTrailCopy(SunWidthFunction, SunColorFunction, null, true, InfernumEffectsRegistry.FireVertexShader);

            InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.45f);
            InfernumEffectsRegistry.FireVertexShader.UseImage1("Images/Misc/Perlin");

            List<float> rotationPoints = [];
            List<Vector2> drawPoints = [];

            for (float offsetAngle = -PiOver2; offsetAngle <= PiOver2; offsetAngle += Pi / 10f)
            {
                rotationPoints.Clear();
                drawPoints.Clear();

                float adjustedAngle = offsetAngle + Pi * -0.2f;
                Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
                for (int i = 0; i < 16; i++)
                {
                    rotationPoints.Add(adjustedAngle);
                    drawPoints.Add(Vector2.Lerp(Projectile.Center - offsetDirection * Radius / 2f, Projectile.Center + offsetDirection * Radius / 2f, i / 16f));
                }

                FireDrawer.Draw(drawPoints, -Main.screenPosition, 14);
            }
            return false;
        }
    }
}
