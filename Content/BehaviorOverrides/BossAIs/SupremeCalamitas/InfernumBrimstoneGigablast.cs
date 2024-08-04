﻿using CalamityMod;
using CalamityMod.Dusts;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class InfernumBrimstoneGigablast : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Brimstone Gigablast");
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.Opacity = 0f;
            Projectile.Calamity().DealsDefenseDamage = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];
            Projectile.Opacity = Utils.GetLerpValue(0f, 20f, Time, true) * Utils.GetLerpValue(0f, 20f, Projectile.timeLeft, true);
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;

            Lighting.AddLight(Projectile.Center, Projectile.Opacity * 0.9f, 0f, 0f);

            if (Projectile.localAI[0] == 0f)
            {
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
                Projectile.localAI[0] = 1f;
            }

            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            float flySpeed = Projectile.velocity.Length();
            Projectile.velocity = (Projectile.velocity * 24f + Projectile.SafeDirectionTo(target.Center) * flySpeed) / 25f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * flySpeed;
            Time++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int drawStart = frameHeight * Projectile.frame;
            lightColor.R = (byte)(255 * Projectile.Opacity);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle frame = new(0, drawStart, texture.Width, frameHeight);
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, 0, 0);
            return false;
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity == 1f;

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SCalBrimstoneGigablast.ImpactSound, Projectile.Center);

            if (Projectile.owner == Main.myPlayer)
            {
                int barrageCount = 45;
                if (Projectile.ai[1] >= 2f)
                    barrageCount = (int)Projectile.ai[1];

                for (int i = 0; i < barrageCount; i++)
                {
                    Vector2 dartVelocity = (TwoPi * i / barrageCount + Projectile.AngleTo(Main.player[Projectile.owner].Center)).ToRotationVector2() * 5f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dartVelocity, ModContent.ProjectileType<BrimstoneBarrageOld>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 1f);
                }
            }

            for (int j = 0; j < 2; j++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f, 50, default, 1f);
            }
            for (int k = 0; k < 20; k++)
            {
                int redFire = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f, 0, default, 1.5f);
                Main.dust[redFire].noGravity = true;
                Main.dust[redFire].velocity *= 3f;
                redFire = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f, 50, default, 1f);
                Main.dust[redFire].velocity *= 2f;
                Main.dust[redFire].noGravity = true;
            }
        }
    }
}
