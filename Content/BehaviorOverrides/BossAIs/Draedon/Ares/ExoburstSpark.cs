﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class ExoburstSpark : ModProjectile
    {
        // public override void SetStaticDefaults() => DisplayName.SetDefault("Exoburst Spark");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 360;
            Projectile.Opacity = 0f;
            Projectile.hide = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.Opacity = Clamp(Projectile.Opacity + 0.1f, 0f, 1f);
            Projectile.rotation += Projectile.velocity.X * 0.025f;

            if (Projectile.timeLeft > 240f)
            {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                Projectile.velocity = (Projectile.velocity * 24f + Projectile.SafeDirectionTo(target.Center) * 14.5f) / 25f;
            }
            else if (Projectile.velocity.Length() < 36f)
                Projectile.velocity *= 1.032f;

            // Emit dust.
            for (int i = 0; i < 2; i++)
            {
                Dust fire = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowMk2);
                fire.scale *= 0.8f;
                fire.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.84f);
                fire.velocity = fire.velocity * 0.4f + Main.rand.NextVector2Circular(0.4f, 0.4f);
                fire.fadeIn = 0.4f;
                fire.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 48) * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * i * 12f;
                Vector2 afterimageDrawPosition = Projectile.Center + drawOffset - Main.screenPosition;
                Color backAfterimageColor = Projectile.GetAlpha(lightColor) * ((4f - i) / 4f);
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, null, backAfterimageColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            Color frontAfterimageColor = Projectile.GetAlpha(lightColor) * 0.2f;
            for (int i = 0; i < 9; i++)
            {
                Vector2 drawOffset = (TwoPi * i / 9f + Projectile.rotation - PiOver2).ToRotationVector2() * 2f;
                Vector2 afterimageDrawPosition = Projectile.Center + drawOffset - Main.screenPosition;
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, null, frontAfterimageColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI, List<int> overWiresUI)
        {
            drawCacheProjsBehindProjectiles.Add(index);
        }
    }
}
