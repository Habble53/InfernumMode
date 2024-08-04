﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.BrimstoneElemental
{
    public class BrimstonePetal2 : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];

        public override string Texture => "InfernumMode/Content/BehaviorOverrides/BossAIs/BrimstoneElemental/BrimstonePetal";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Brimstone Petal");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;

        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 420;
            Projectile.Infernum().FadesAwayWhenManuallyKilled = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.Opacity = Utils.GetLerpValue(0f, 15f, Time, true) * Utils.GetLerpValue(0f, 15f, Projectile.timeLeft, true);
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;

            Lighting.AddLight(Projectile.Center, Projectile.Opacity * 0.9f, 0f, 0f);

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 8)
            {
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
                Projectile.frameCounter = 0;
            }

            Time++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);

            for (int i = 0; i < 6; i++)
            {
                Color magicAfterimageColor = Color.White * Projectile.Opacity * 0.3f;
                magicAfterimageColor.A = 0;

                Vector2 drawPosition = Projectile.Center - Main.screenPosition + (TwoPi * i / 6f).ToRotationVector2() * Projectile.Opacity * 4f;
                Main.spriteBatch.Draw(texture, drawPosition, sourceRectangle, magicAfterimageColor, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
