﻿using System.Collections.Generic;
using System.IO;
using CalamityMod.NPCs;
using InfernumMode.Assets.Effects;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Content.BehaviorOverrides.BossAIs.DoG;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class RealitySlice : ModProjectile
    {
        public bool Cosmilite;

        public Vector2 Start;

        public Vector2 End;

        public List<Vector2> TrailCache = [];

        public int Lifetime => Cosmilite ? 248 : 84;

        public ref float Time => ref Projectile.ai[0];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // public override void SetStaticDefaults() => DisplayName.SetDefault("Reality Tear");

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.MaxUpdates = 2;
            
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Cosmilite);
            writer.WriteVector2(Start);
            writer.WriteVector2(End);
            writer.Write(TrailCache.Count);
            for (int i = 0; i < TrailCache.Count; i++)
                writer.WriteVector2(TrailCache[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Cosmilite = reader.ReadBoolean();
            Start = reader.ReadVector2();
            End = reader.ReadVector2();
            int trailCount = reader.ReadInt32();
            for (int i = 0; i < trailCount; i++)
                TrailCache[i] = reader.ReadVector2();
        }

        public override void AI()
        {
            // Disappear if neither the Ceaseless Void nor DoG not present.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.voidBoss) && !Main.npc.IndexInRange(CalamityGlobalNPC.DoGHead))
            {
                Projectile.Kill();
                return;
            }

            if (DoGPhase1HeadBehaviorOverride.GeneralPortalIndex != -1 && Time <= 2f)
            {
                Vector2 oldCenter = Projectile.Center;
                Projectile.Center = Main.projectile[DoGPhase1HeadBehaviorOverride.GeneralPortalIndex].Center;

                Start += Projectile.Center - oldCenter;
                End += Projectile.Center - oldCenter;
            }

            float sliceInterpolant = Pow(Utils.GetLerpValue(0f, 27f, Time, true), 1.6f);
            Projectile.Center = Vector2.Lerp(Start, End, sliceInterpolant);
            if (Time <= 27f)
                TrailCache.Add(Projectile.Center);

            // Fade in.
            float disappearInterpolant = Utils.GetLerpValue(0f, 16f, Projectile.timeLeft / Projectile.MaxUpdates, true);
            float scaleGrowInterpolant = Pow(Utils.GetLerpValue(0f, 15f, Time, true), 1.72f);
            Projectile.Opacity = Utils.GetLerpValue(0f, 24f, Time / Projectile.MaxUpdates, true) * disappearInterpolant;
            Projectile.scale = Lerp(0.24f, 1f, scaleGrowInterpolant) * disappearInterpolant;
            Time++;
        }

        #region Drawing
        internal float WidthFunction(float completionRatio)
        {
            float width = Cosmilite ? 80f : 40f;
            return LumUtils.Convert01To010(completionRatio) * Projectile.scale * width;
        }

        internal Color ColorFunction(float completionRatio)
        {
            Color baseColor = Color.White;
            if (Cosmilite)
                baseColor = (Projectile.localAI[0] == 0f ? Color.Cyan : Color.Fuchsia) with { A = 0 };

            float opacity = LumUtils.Convert01To010(completionRatio) * 1.4f;
            if (opacity >= 1f)
                opacity = 1f;
            opacity *= Projectile.Opacity;
            return baseColor * opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.GraphicsDevice.Textures[1] = InfernumTextureRegistry.Stars.Value;
            InfernumEffectsRegistry.RealityTearVertexShader.TrySetParameter("useOutline", true);

            Projectile.localAI[0] = 0f;
            PrimitiveSettings settings = new(WidthFunction, ColorFunction, _ => Projectile.Size * 0.5f, Shader: InfernumEffectsRegistry.RealityTearVertexShader);
            PrimitiveRenderer.RenderTrail(TrailCache, settings, 50);
            if (Cosmilite)
            {
                Projectile.localAI[0] = 1f;
                PrimitiveRenderer.RenderTrail(TrailCache, settings, 50);
            }
            return false;
        }
        #endregion
    }
}
