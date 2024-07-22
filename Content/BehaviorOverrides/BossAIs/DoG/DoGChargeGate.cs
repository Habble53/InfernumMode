﻿using System.IO;
using CalamityMod.NPCs.DevourerofGods;
using InfernumMode.Assets.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.DoG
{
    public class DoGChargeGate : ModProjectile
    {
        public int Time;

        public bool TelegraphShouldAim = true;

        public Vector2 Destination;

        public bool IsGeneralPortalIndex;

        public bool IsChargePortalIndex;

        public ref float TelegraphDelay => ref Projectile.ai[0];

        public bool NoTelegraph
        {
            get => Projectile.localAI[0] == 1f;
            set => Projectile.localAI[0] = value.ToInt();
        }

        public ref float TelegraphTotalTime => ref Projectile.ai[1];

        public ref float Lifetime => ref Projectile.localAI[1];

        public const int DefaultLifetime = 225;

        public const int FadeoutTime = 45;

        public const float TelegraphFadeTime = 18f;

        public const float TelegraphWidth = 6400f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // public override void SetStaticDefaults() => DisplayName.SetDefault("Portal");

        public override void SetDefaults()
        {
            Projectile.width = 580;
            Projectile.height = 580;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NoTelegraph);
            writer.Write(IsGeneralPortalIndex);
            writer.Write(IsChargePortalIndex);
            writer.Write(Time);
            writer.Write(Lifetime);
            writer.Write(TelegraphShouldAim);
            writer.Write(Projectile.scale);
            writer.WriteVector2(Destination);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NoTelegraph = reader.ReadBoolean();
            IsGeneralPortalIndex = reader.ReadBoolean();
            IsChargePortalIndex = reader.ReadBoolean();
            Time = reader.ReadInt32();
            Lifetime = reader.ReadSingle();
            TelegraphShouldAim = reader.ReadBoolean();
            Projectile.scale = reader.ReadSingle();
            Destination = reader.ReadVector2();
        }

        public override void AI()
        {
            // Use fallbacks for the telegraph time and lifetime if nothing specific is defined.
            if (TelegraphTotalTime == 0f)
                TelegraphTotalTime = 75f;
            if (Lifetime == 0f)
                Lifetime = DefaultLifetime;

            if (!NoTelegraph && !NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>()))
            {
                Projectile.Kill();
                return;
            }

            if (Time < Lifetime - FadeoutTime)
            {
                if (IsGeneralPortalIndex)
                    DoGPhase1HeadBehaviorOverride.GeneralPortalIndex = Projectile.whoAmI;
                if (IsChargePortalIndex)
                    DoGPhase1HeadBehaviorOverride.ChargePortalIndex = Projectile.whoAmI;
            }

            if (Time >= Lifetime)
                Projectile.Kill();

            TelegraphDelay++;

            // Make the portal dissipate once ready.
            if (TelegraphDelay > TelegraphTotalTime)
                Projectile.alpha = Utils.Clamp(Projectile.alpha - 25, 0, 255);

            // Adjust the aim destination such that it approaches the closest player. This stops right before the telegraph line dissipates.
            if (TelegraphDelay < TelegraphTotalTime * 0.8f && TelegraphShouldAim)
            {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                Vector2 idealDestination = target.Center + target.velocity * new Vector2(36f, 27f);
                if (Destination == Vector2.Zero)
                    Destination = idealDestination;
                Destination = Vector2.Lerp(Destination, idealDestination, 0.1f).MoveTowards(idealDestination, 5f);
            }
            Time++;
        }

        public override void OnKill(int timeLeft)
        {
            if (IsGeneralPortalIndex)
                DoGPhase1HeadBehaviorOverride.GeneralPortalIndex = -1;
            if (IsChargePortalIndex)
                DoGPhase1HeadBehaviorOverride.ChargePortalIndex = -1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float fade = Utils.GetLerpValue(0f, 35f, Time, true);
            if (Time >= Lifetime - FadeoutTime)
                fade = Utils.GetLerpValue(Lifetime, Lifetime - FadeoutTime, Time, true);

            Texture2D noiseTexture = InfernumTextureRegistry.VoronoiShapes.Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin2 = noiseTexture.Size() * 0.5f;
            if (NoTelegraph)
            {
                Main.spriteBatch.EnterShaderRegion();

                GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(fade);
                GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(new Color(0.2f, 1f, 1f, 0f));
                GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(new Color(1f, 0.2f, 1f, 0f));
                GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

                Main.spriteBatch.Draw(noiseTexture, drawPosition, null, Color.White, 0f, origin2, Projectile.scale * 3.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.ExitShaderRegion();
                return false;
            }

            Texture2D laserTelegraph = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/LaserWallTelegraphBeam").Value;
            float yScale = Utils.GetLerpValue(0f, 12f, TelegraphDelay, true) * Utils.GetLerpValue(TelegraphTotalTime, TelegraphTotalTime - 12f, TelegraphDelay, true) * 4f;

            Vector2 scaleInner = new(TelegraphWidth / laserTelegraph.Width, yScale);
            Vector2 origin = laserTelegraph.Size() * new Vector2(0f, 0.5f);
            Vector2 scaleOuter = scaleInner * new Vector2(1f, 1.9f);

            Color colorOuter = Color.Lerp(Color.Cyan, Color.Purple, TelegraphDelay / TelegraphTotalTime * 4f % 1f); // Iterate through purple and cyan once and then flash.
            Color colorInner = Color.Lerp(colorOuter, Color.White, 0.85f);

            colorOuter *= 0.7f;
            colorInner *= 0.7f;
            colorInner.A = 72;

            Main.spriteBatch.Draw(laserTelegraph, Projectile.Center - Main.screenPosition, null, colorInner, Projectile.AngleTo(Destination), origin, scaleInner, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(laserTelegraph, Projectile.Center - Main.screenPosition, null, colorOuter, Projectile.AngleTo(Destination), origin, scaleOuter, SpriteEffects.None, 0f);

            Main.spriteBatch.EnterShaderRegion();

            GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(fade);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(Color.Cyan);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(Color.Fuchsia);
            GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

            Main.spriteBatch.Draw(noiseTexture, drawPosition, null, Color.White, 0f, origin2, 2.7f, SpriteEffects.None, 0f);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
