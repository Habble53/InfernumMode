﻿using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Golem
{
    public class GolemFistLeft : ModNPC
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.GolemFistRight}";

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            // DisplayName.SetDefault("Golem Fist");
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 1;
            NPC.defDamage = NPC.damage = 125;
            NPC.dontTakeDamage = true;
            NPC.width = 40;
            NPC.height = 40;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }

        public override bool PreAI() => DoFistAI(NPC);

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => DrawFist(NPC, Main.spriteBatch, drawColor, true);

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            
            return base.CanHitPlayer(target, ref cooldownSlot);
        }

        public static bool DoFistAI(NPC npc)
        {
            if (!Main.npc[(int)npc.ai[0]].active || Main.npc[(int)npc.ai[0]].type != NPCID.Golem)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    GolemBodyBehaviorOverride.DespawnNPC(npc.whoAmI);
                return false;
            }
            npc.damage = Main.npc[(int)npc.ai[0]].damage >= 1 ? npc.defDamage : 0;
            npc.dontTakeDamage = true;
            npc.chaseable = false;
            return false;
        }

        public static bool DrawFist(NPC npc, SpriteBatch spriteBatch, Color lightColor, bool leftFist)
        {
            if (npc.Opacity == 0f)
                return false;

            NPC body = Main.npc[(int)npc.ai[0]];
            Vector2 FistCenterPos = leftFist ? new Vector2(body.Left.X, body.Left.Y) : new Vector2(body.Right.X, body.Right.Y);
            float armRotation = npc.AngleFrom(FistCenterPos) + PiOver2;
            bool continueDrawing = true;
            while (continueDrawing)
            {
                int moveDistance = 16;
                if (npc.Distance(FistCenterPos) < moveDistance)
                {
                    moveDistance = (int)npc.Distance(FistCenterPos);
                    continueDrawing = false;
                }
                Color color = Lighting.GetColor((int)(FistCenterPos.X / 16f), (int)(FistCenterPos.Y / 16f));
                Texture2D armTexture = TextureAssets.Chain21.Value;
                Rectangle frame = new(0, 0, armTexture.Width, moveDistance);
                spriteBatch.Draw(armTexture, FistCenterPos - Main.screenPosition, frame, color, armRotation, armTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                FistCenterPos += (npc.Center - FistCenterPos).SafeNormalize(Vector2.Zero) * moveDistance;
            }

            SpriteEffects effect = leftFist ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.instance.LoadNPC(NPCID.GolemFistRight);
            Texture2D texture = TextureAssets.Npc[NPCID.GolemFistRight].Value;
            spriteBatch.Draw(texture, npc.Center - Main.screenPosition, npc.frame, lightColor * npc.Opacity, npc.rotation, npc.frame.Size() * 0.5f, 1f, effect, 0f);
            return false;
        }
    }
}
