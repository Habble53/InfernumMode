﻿using System;
using CalamityMod;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Common.Graphics.Interfaces;
using InfernumMode.Common.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class SuicideBomberDemonHostile : ModProjectile, IPixelPrimitiveDrawer
    {
        public bool HasDamagedSomething
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value.ToInt();
        }

        public ref float Time => ref Projectile.ai[1];

        public Player Owner => Main.player[Projectile.owner];

        public PrimitiveTrailCopy FlameTrailDrawer;

        public const int RiseTime = 45;

        public const int ScreamDelay = 30;

        public const int ChargePreparationTime = 45;

        public const int AttackDuration = 420;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Demon");
            Main.projFrames[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 11;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 48;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.Opacity = 0f;
            Projectile.timeLeft = AttackDuration + 45;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Time++;

            // Decide an owner if necessary.
            if (Projectile.owner == 255)
                Projectile.owner = Player.FindClosest(Projectile.Center, 1, 1);

            // Rapidly fade in.
            Projectile.Opacity = Clamp(Projectile.Opacity + 0.1f, 0f, 1f);

            // Anti-clumping behavior.
            float pushForce = 0.08f;
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile otherProj = Main.projectile[k];

                // Short circuits to make the loop as fast as possible.
                if (!otherProj.active || otherProj.type != Projectile.type || k == Projectile.whoAmI)
                    continue;

                // If the other projectile is indeed the same owned by the same player and they're too close, nudge them away.
                bool sameProjType = otherProj.type == Projectile.type;
                float taxicabDist = Distance(Projectile.position.X, otherProj.position.X) + Distance(Projectile.position.Y, otherProj.position.Y);
                if (sameProjType && taxicabDist < 60f)
                {
                    if (Projectile.position.X < otherProj.position.X)
                        Projectile.velocity.X -= pushForce;
                    else
                        Projectile.velocity.X += pushForce;

                    if (Projectile.position.Y < otherProj.position.Y)
                        Projectile.velocity.Y -= pushForce;
                    else
                        Projectile.velocity.Y += pushForce;
                }
            }

            Entity target = Owner;
            float attackFlySpeed = 26.5f;
            float flyInertia = 32f;

            // Nullify the target value if they're a dead player.
            if (!Owner.active || Owner.dead)
                target = null;

            int oldSpriteDirection = Projectile.spriteDirection;

            // Rise upward somewhat slowly and flap wings.
            if (Time < RiseTime)
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, -Vector2.UnitY * 5f, 0.06f);
                if (Projectile.frameCounter >= 6)
                {
                    Projectile.frame = (Projectile.frame + 1) % 5;
                    Projectile.frameCounter = 0;
                }
            }
            else if (Time < RiseTime + ChargePreparationTime)
            {
                // Slow down.
                Projectile.velocity = Projectile.velocity.MoveTowards(Vector2.Zero, 0.4f) * 0.95f;

                // Handle frames.
                Projectile.frame = (int)Math.Round(Lerp(5f, 10f, Utils.GetLerpValue(45f, 90f, Time, true)));

                // And look at the target.
                float idealAngle = target is null ? 0f : Projectile.AngleTo(target.Center);
                Projectile.spriteDirection = target is null ? 1 : (target.Center.X > Projectile.Center.X).ToDirectionInt();
                if (Projectile.spriteDirection != oldSpriteDirection)
                    Projectile.rotation += Pi;

                if (Projectile.spriteDirection == -1)
                    idealAngle += Pi;
                Projectile.rotation = Projectile.rotation.AngleTowards(idealAngle, 0.3f).AngleLerp(idealAngle, 0.08f);

                if (Time == RiseTime + ScreamDelay)
                    SoundEngine.PlaySound(SoundID.DD2_WyvernScream, Projectile.Center);

                // Reset the oldPos array in anticipation of a trail being drawn during the fly phase.
                Projectile.oldPos = new Vector2[Projectile.oldPos.Length];
            }
            else
            {
                if (Time == RiseTime + ChargePreparationTime)
                    SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, Projectile.Center);

                Projectile.frame = Main.projFrames[Projectile.type] - 1;

                // Fly away if no valid target was found.
                if (target is null)
                    Projectile.velocity = -Vector2.UnitY * 18f;

                // Otherwise fly towards the target.
                else
                    Projectile.velocity = (Projectile.velocity * (flyInertia - 1f) + Projectile.SafeDirectionTo(target.Center) * attackFlySpeed) / flyInertia;

                Projectile.spriteDirection = (Projectile.velocity.X > 0f).ToDirectionInt();
                Projectile.rotation = LumUtils.WrapAngle90Degrees(Projectile.velocity.ToRotation());
                if (target is null)
                {
                    Projectile.spriteDirection = 1;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                }

                // Die if something has been hit and the target is either gone or really close.
                if (HasDamagedSomething && (target is null || target == Owner || Projectile.WithinRange(target.Center, target.Size.Length() * 0.4f)))
                    Projectile.Kill();
            }

            if (target != null && !HasDamagedSomething && Projectile.Center.ManhattanDistance(target.Center) < target.height)
            {
                HasDamagedSomething = true;
                Projectile.netUpdate = true;
            }

            // Die when the attack is ready to end.
            if (Time >= AttackDuration)
                Projectile.Kill();

            Projectile.frameCounter++;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, Projectile.Center);
            for (int i = 0; i < 40; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowMk2);
                explosion.velocity = Main.rand.NextVector2Circular(4f, 4f);
                explosion.color = Color.Red;
                explosion.scale = 1.35f;
                explosion.fadeIn = 0.45f;
                explosion.noGravity = true;

                if (Main.rand.NextBool(3))
                    explosion.scale *= 1.45f;

                if (Main.rand.NextBool(6))
                {
                    explosion.scale *= 1.75f;
                    explosion.fadeIn += 0.4f;
                }
            }
        }

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of false */ => Projectile.Opacity >= 1f;

        public static float FlameTrailWidthFunction(float completionRatio) => SmoothStep(21f, 8f, completionRatio);

        public static Color FlameTrailColorFunction(float completionRatio)
        {
            float trailOpacity = Utils.GetLerpValue(0.8f, 0.27f, completionRatio, true) * Utils.GetLerpValue(0f, 0.067f, completionRatio, true);
            Color startingColor = Color.Lerp(Color.Cyan, Color.White, 0.4f);
            Color middleColor = Color.Lerp(Color.Orange, Color.Yellow, 0.3f);
            Color endColor = Color.Lerp(Color.Orange, Color.Red, 0.67f);
            return LumUtils.MulticolorLerp(completionRatio, startingColor, middleColor, endColor) * trailOpacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();

            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/SuicideBomberDemon").Value;
            Texture2D glowmask = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/SuicideBomberDemonGlowmask").Value;
            Texture2D orbTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/SuicideBomberDemonOrb").Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Draw the base sprite and glowmask.
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (TwoPi * i / 8f).ToRotationVector2() * 6f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, frame, Projectile.GetAlpha(Color.Yellow) with { A = 0 }, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0);
            }
            Main.spriteBatch.Draw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0);
            Main.spriteBatch.Draw(glowmask, drawPosition, frame, Projectile.GetAlpha(Color.White), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0);

            // Draw the flame trail and flame orb once ready.
            if (Time >= 90f)
            {
                float flameOrbGlowIntensity = Utils.GetLerpValue(90f, 98f, Time, true);
                for (int i = 0; i < 12; i++)
                {
                    Color flameOrbColor = Color.LightCyan * flameOrbGlowIntensity * 0.125f;
                    flameOrbColor.A = 0;
                    Vector2 flameOrbDrawOffset = (TwoPi * i / 12f + Main.GlobalTimeWrappedHourly * 2f).ToRotationVector2();
                    flameOrbDrawOffset *= flameOrbGlowIntensity * 3f;
                    Main.spriteBatch.Draw(orbTexture, drawPosition + flameOrbDrawOffset, frame, Projectile.GetAlpha(flameOrbColor), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0);
                }
            }

            Main.spriteBatch.ExitShaderRegion();

            return false;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // Initialize the flame trail drawer.
            FlameTrailDrawer ??= new(FlameTrailWidthFunction, FlameTrailColorFunction, null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);

            // Prepare the flame trail shader with its map texture.
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(InfernumTextureRegistry.StreakFaded);
            if (Time >= 90f)
            {
                Vector2 trailOffset = Projectile.Size * 0.5f;
                trailOffset += (Projectile.rotation + PiOver2).ToRotationVector2() * 20f;
                FlameTrailDrawer.DrawPixelated(Projectile.oldPos, trailOffset - Main.screenPosition, 61);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            HasDamagedSomething = true;
            Projectile.netUpdate = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            HasDamagedSomething = true;
            Projectile.netUpdate = true;
        }
    }
}
