﻿using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Projectiles.Rogue;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.DukeFishron
{
    public class RedirectingBubble : ModNPC
    {
        public Player Target => Main.player[NPC.target];

        public ref float Time => ref NPC.ai[0];

        public const float InitialSpeed = 0.3f;

        public const float RedirectSpeed = 11f;

        public override string Texture => $"Terraria/Images/NPC_{NPCID.DetonatingBubble}";

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            // DisplayName.SetDefault("Bubble");
            Main.npcFrameCount[NPC.type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 1f;
            NPC.aiStyle = AIType = -1;
            NPC.damage = 70;
            NPC.width = NPC.height = 36;
            NPC.lifeMax = 200;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */ => NPC.life = 1300;

        public override void AI()
        {
            float redirectSpeed = RedirectSpeed * (BossRushEvent.BossRushActive ? 2f : 1f);
            if (Time < 45 && NPC.velocity.Length() < redirectSpeed)
                NPC.velocity *= Pow(redirectSpeed / InitialSpeed, 1f / 45f);
            else if (Time >= 45f)
                NPC.velocity = NPC.velocity.RotateTowards(NPC.AngleTo(Target.Center), ToRadians(2.4f));

            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height) || NPC.WithinRange(Target.Center, 40f))
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }

            if (Collision.WetCollision(NPC.position, NPC.width, NPC.height))
            {
                NPC.life -= 4;
                if (NPC.life <= 0f)
                    NPC.active = false;
            }

            if (Time >= 180f)
            {
                NPC.velocity *= 0.96f;
                if (NPC.velocity.Length() < 0.5f)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                }
                NPC.scale = Lerp(1f, 1.6f, Utils.GetLerpValue(1.8f, 0.7f, NPC.velocity.Length(), true));
            }

            Time++;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = frameHeight * (NPC.whoAmI % 2);
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.type == ModContent.ProjectileType<Corrocloud1>() || projectile.type == ModContent.ProjectileType<Corrocloud2>() || projectile.type == ModContent.ProjectileType<Corrocloud3>())
                modifiers.FinalDamage.Base *= 0.225f;
        }

        public override bool CheckActive() => false;

    }
}
