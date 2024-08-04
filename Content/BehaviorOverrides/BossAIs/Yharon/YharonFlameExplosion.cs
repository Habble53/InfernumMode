﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Yharon
{
    public class YharonFlameExplosion : ModProjectile
    {
        public override string Texture => "CalamityMod/Skies/XerocLight";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = Projectile.MaxUpdates * 210;
            Projectile.scale = 0.15f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.scale += 0.16f;
            Projectile.Opacity = Utils.GetLerpValue(Projectile.MaxUpdates * 300f, Projectile.MaxUpdates * 265f, Projectile.timeLeft, true) * Utils.GetLerpValue(0f, Projectile.MaxUpdates * 50f, Projectile.timeLeft, true);
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;

            if (Projectile.velocity.Length() < 18f)
                Projectile.velocity *= 1.02f;

            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3());
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Color explosionColor = Color.Lerp(Color.Orange, Color.Yellow, 0.5f);
            explosionColor = Color.Lerp(explosionColor, Color.White, Projectile.Opacity * 0.2f);
            explosionColor *= Projectile.Opacity * 0.25f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.SetBlendState(BlendState.Additive);
            for (int i = 0; i < (int)Lerp(3f, 6f, Projectile.Opacity); i++)
                Main.spriteBatch.Draw(texture, drawPosition, null, explosionColor, 0f, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.ResetBlendState();
            return false;
        }
    }
}
