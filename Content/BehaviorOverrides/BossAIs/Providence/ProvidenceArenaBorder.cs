﻿using System;
using System.Collections.Generic;
using InfernumMode.Core.GlobalInstances.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Providence
{
    public class ProvidenceArenaBorder : ModProjectile
    {
        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 99999999;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.alpha = 255;
            Projectile.hide = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (InfernumMode.ProvidenceArenaTimer <= 0)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float arenaFallCompletion = Clamp(InfernumMode.ProvidenceArenaTimer / 120f, 0f, 1f);
            Vector2 top = WorldSaveSystem.ProvidenceArena.TopLeft() * 16f + new Vector2(8f, 32f);
            Vector2 bottom = WorldSaveSystem.ProvidenceArena.TopLeft() + Vector2.UnitY * 2f;
            for (int i = 0; i < 200; i++)
            {
                if (Framing.GetTileSafely((int)bottom.X, (int)bottom.Y).HasTile)
                    break;
                bottom.Y++;
            }

            bottom = bottom * 16f + new Vector2(8f, 52f);
            float distanceToBottom = Distance(top.Y, bottom.Y);
            float distancePerSegment = MathF.Max(texture.Height, 8f) * Projectile.scale;
            for (float y = 0f; y < distanceToBottom; y += distancePerSegment)
            {
                Rectangle frame = texture.Frame();
                if (y + frame.Height >= distanceToBottom)
                    frame.Height = (int)(distanceToBottom - y);

                Vector2 drawPosition = new(top.X, top.Y + y + (1f - arenaFallCompletion) * distanceToBottom);
                Color color = Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16));
                color = Color.Lerp(color, Color.White, 0.6f);
                drawPosition -= Main.screenPosition;
                Main.spriteBatch.Draw(texture, drawPosition, frame, color, 0f, Vector2.Zero, Projectile.scale, 0, 0f);
            }

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            ScreenOverlaysSystem.DrawCacheProjsOverSignusBlackening.Add(index);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
