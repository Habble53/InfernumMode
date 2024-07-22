﻿using CalamityMod;
using CalamityMod.Events;
using InfernumMode.Assets.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Signus
{
    public class CosmicKunai : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Cosmic Kunai");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.Infernum().FadesAwayWhenManuallyKilled = true;
            
        }

        public override void AI()
        {
            Projectile.Opacity = Utils.GetLerpValue(0f, 6f, Time, true) * Utils.GetLerpValue(0f, 6f, Projectile.timeLeft, true);

            Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            if (Time < 26f)
            {
                float spinSlowdown = Utils.GetLerpValue(18f, 5f, Time, true);
                Projectile.velocity *= 0.7f;
                Projectile.rotation += (Projectile.velocity.X > 0f).ToDirectionInt() * spinSlowdown * 0.3f;
                if (spinSlowdown < 1f)
                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.AngleTo(closestPlayer.Center) + PiOver2, (1f - spinSlowdown) * 0.6f);
            }

            if (Time == 26f)
            {
                Projectile.velocity = Projectile.SafeDirectionTo(closestPlayer.Center) * 18f;
                if (BossRushEvent.BossRushActive)
                    Projectile.velocity *= 1.75f;
                SoundEngine.PlaySound(InfernumSoundRegistry.SignusWeaponFireSound with { Volume = 0.45f }, Projectile.Center);
            }
            if (Time > 26f && Projectile.velocity.Length() < (BossRushEvent.BossRushActive ? 50f : 26f))
                Projectile.velocity *= BossRushEvent.BossRushActive ? 1.03f : 1.021f;

            Lighting.AddLight(Projectile.Center, Vector3.One * Projectile.Opacity * 0.4f);
            Time++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(new Color(198, 118, 204, 0), lightColor, Utils.GetLerpValue(8f, 30f, Time, true)) * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // Draw afterimages.
            if (Projectile.velocity.Length() > 2.5f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 afterimageOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * i * -20f;
                    Color afterimageColor = new Color(198, 118, 204, 0) * (1f - i / 5f) * 0.7f;
                    Main.spriteBatch.Draw(texture, drawPosition + afterimageOffset, null, Projectile.GetAlpha(afterimageColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
                }
            }
            // Draw backglow effects.
            for (int i = 0; i < 12; i++)
            {
                Vector2 afterimageOffset = (TwoPi * i / 12f).ToRotationVector2() * 4f;
                Color afterimageColor = new Color(1f, 1f, 1f, 0f) * 0.7f;
                Main.spriteBatch.Draw(texture, drawPosition + afterimageOffset, null, Projectile.GetAlpha(afterimageColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of false */ => Projectile.alpha < 20;
    }
}
