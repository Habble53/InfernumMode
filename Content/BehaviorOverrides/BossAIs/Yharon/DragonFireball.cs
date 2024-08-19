﻿using System.IO;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Yharon
{
    public class DragonFireball : ModProjectile
    {
        public ref float Timer => ref Projectile.ai[1];

        private readonly int Lifetime = 180;

        public float LifetimeRatio => Timer / Lifetime;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Fireball");
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.Opacity = 0f;
            Projectile.Calamity().DealsDefenseDamage = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.tileCollide);

        public override void ReceiveExtraAI(BinaryReader reader) => Projectile.tileCollide = reader.ReadBoolean();

        public override void AI()
        {
            // Create a fire sound on the first frame.
            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                SoundEngine.PlaySound(SoundID.Item34, Projectile.position);
            }

            // Fade in and determine rotation.
            Projectile.alpha = Utils.Clamp(Projectile.alpha - 40, 0, 255);
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Emit light.
            Lighting.AddLight(Projectile.Center, 1.1f, 0.9f, 0.4f);

            // Create fire and smoke dust effects.
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] % 12f == 11f)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 fireSpawnOffset = Vector2.UnitX * Projectile.width * -0.5f;
                    fireSpawnOffset += -Vector2.UnitY.RotatedBy(TwoPi * i / 12f) * new Vector2(8f, 16f);
                    fireSpawnOffset = fireSpawnOffset.RotatedBy((double)(Projectile.rotation - 1.57079637f));
                    Dust fire = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, 0f, 0f, 160, default, 1f);
                    fire.scale = 1.1f;
                    fire.noGravity = true;
                    fire.position = Projectile.Center + fireSpawnOffset;
                    fire.velocity = Projectile.velocity * 0.1f;
                    fire.velocity = Vector2.Normalize(Projectile.Center - Projectile.velocity * 3f - fire.position) * 1.25f;
                }
            }
            if (Main.rand.NextBool(4))
            {
                Vector2 offsetDirection = -Vector2.UnitX.RotatedByRandom(Pi / 12f).RotatedBy(Projectile.velocity.ToRotation());
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
                smoke.velocity *= 0.1f;
                smoke.position = Projectile.Center + offsetDirection * Projectile.width / 2f;
                smoke.fadeIn = 0.9f;
            }
            if (Main.rand.NextBool(32))
            {
                Vector2 offsetDirection = -Vector2.UnitX.RotatedByRandom(Pi / 8f).RotatedBy(Projectile.velocity.ToRotation());
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 155, default, 0.8f);
                smoke.velocity *= 0.3f;
                smoke.position = Projectile.Center + offsetDirection * Projectile.width / 2f;
                if (Main.rand.NextBool(2))
                    smoke.fadeIn = 1.4f;
            }
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offsetDirection = -Vector2.UnitX.RotatedByRandom(PiOver4).RotatedBy((double)Projectile.velocity.ToRotation());
                    Dust fire = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, default, 1.2f);
                    fire.velocity *= 0.3f;
                    fire.noGravity = true;
                    fire.position = Projectile.Center + offsetDirection * Projectile.width / 2f;
                    if (Main.rand.NextBool(2))
                        fire.fadeIn = 1.4f;
                }
            }

            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 3 % Main.projFrames[Projectile.type];
            Timer++;
        }
        public override bool? CanDamage() => LifetimeRatio is > 0.1f and < 0.9f;
    }
}
