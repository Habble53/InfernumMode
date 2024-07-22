﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.GreatSandShark
{
    public class GreatSandBlast : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Sand Blast");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 360;
            Projectile.alpha = 255;
            
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
            Projectile.Opacity = Clamp(Projectile.Opacity + 0.125f, 0f, 1f);
            if (Projectile.timeLeft < 240)
                Projectile.velocity.Y = Clamp(Projectile.velocity.Y + 0.2f, -8f, 10f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = 0; i < 6; i++)
            {
                Vector2 drawOffset = (TwoPi * i / 6f).ToRotationVector2() * 3f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, Projectile.GetAlpha(new Color(1f, 1f, 1f, 0f)) * 0.65f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
