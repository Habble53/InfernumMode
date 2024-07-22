﻿using System;
using CalamityMod;
using InfernumMode.Assets.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class SulphuricGas : ModProjectile
    {
        public ref float LightPower => ref Projectile.ai[0];

        public ref float IdealScale => ref Projectile.ai[1];

        public static int Lifetime => 120;

        public override string Texture => "InfernumMode/Assets/ExtraTextures/GreyscaleObjects/NebulaGas1";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 50;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.scale = 0.03f;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.Infernum().FadesAwayWhenManuallyKilled = true;
            
        }

        public override void AI()
        {
            // Decide scale and initial rotation on the first frame this projectile exists.
            if (IdealScale == 0f)
            {
                IdealScale = Main.rand.NextFloat(4f, 5.5f);
                Projectile.rotation = Main.rand.NextFloat(TwoPi);
                Projectile.netUpdate = true;
            }

            // Grow in scale.
            float idealScale = IdealScale + Utils.Remap(Projectile.timeLeft, 60f, 0f, 0f, 10f);
            Projectile.scale = Lerp(Projectile.scale, idealScale, 0.064f);

            if (Main.netMode != NetmodeID.Server)
            {
                // Calculate light power. This checks below the position of the fog to check if this fog is underground.
                // Without this, it may render over the fullblack that the game renders for obscured tiles.
                float lightPowerBelow = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16 + 6).ToVector3().Length() / Sqrt(3f);
                if (Framing.GetTileSafely((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16).LiquidAmount >= 25)
                    lightPowerBelow = 1f;

                LightPower = Lerp(LightPower, lightPowerBelow, 0.15f);
            }

            Projectile.Opacity = Utils.GetLerpValue(Lifetime, Lifetime - 20f, Projectile.timeLeft, true) * Utils.GetLerpValue(0f, 40f, Projectile.timeLeft, true) * 0.675f;
            Projectile.rotation += Projectile.velocity.X * 0.002f;
            Projectile.velocity *= 0.985f;
        }

        public override bool? CanDamage() => Projectile.Opacity > 0.56f;

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 screenArea = new(Main.screenWidth, Main.screenHeight);
            Rectangle screenRectangle = Utils.CenteredRectangle(Main.screenPosition + screenArea * 0.5f, screenArea * 1.33f);

            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 20;
            if (!Projectile.Hitbox.Intersects(screenRectangle))
                return false;

            // Decide which gas texture to use.
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            if (Projectile.identity % 2 == 1)
                texture = InfernumTextureRegistry.Cloud2.Value;

            // Calculate drawing variables for the mist.
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float opacity = Utils.GetLerpValue(0f, 0.08f, LightPower, true) * Projectile.Opacity;

            int b = 160 + (int)(Math.Sin(Pi * Projectile.identity / 8f + Main.GlobalTimeWrappedHourly * 10f) * 80f);
            Color drawColor = new Color(141, 255, b) * opacity;
            Vector2 scale = Vector2.One * 50f / texture.Size() * Projectile.scale * 1.35f;
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Main.spriteBatch.Draw(texture, drawPosition, null, drawColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.ResetBlendState();
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Utilities.CircularCollision(Projectile.Center, targetHitbox, Projectile.scale * 30f);
    }
}
