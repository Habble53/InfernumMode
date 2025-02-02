﻿using System.Threading;
using InfernumMode.Content.Items.Placeables;
using InfernumMode.Core.GlobalInstances.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace InfernumMode.Content.Credits
{
    public class CreditManager : ModSystem
    {
        private enum CreditState
        {
            LoadingTextures,
            Playing,
            FinalScene,
            FinalizingDisposing
        }

        public static bool CreditsPlaying
        {
            get;
            private set;
        }

        public static int CreditsTimer
        {
            get;
            internal set;
        }

        private static int ActiveGifIndex;

        private static CreditAnimationObject[] CreditGIFs;

        private static CreditState CurrentState = CreditState.LoadingTextures;

        private static readonly string[] Names = [Programmers, Musicians, Artists, Testers1, Testers2, Testers3, Testers4, Translators, Supporters];

        private static string[] Headers => [Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.ProgramHeader"),
            Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.MusicHeader"),
            Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.ArtHeader"),
            Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.TestHeader"),
            Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.TestHeader"),
            Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.TestHeader"),
            Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.TestHeader"),
            Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.TranslateHeader"),
            Language.GetTextValue("Mods.InfernumMode.CreditsHeaders.SupportHeader")];

        private static readonly Color[] HeaderColors =
        [
            new(212, 56, 34),
            new(143, 11, 139),
            new(80, 105, 185),
            new(0, 148, 75),
            new(0, 148, 75),
            new(0, 148, 75),
            new(0, 148, 75),
            new(246, 188, 49),
            new(243, 165, 5)
        ];

        public static float FinalSceneOpacity
        {
            get
            {
                float maxTime = 540f;
                float fadeInTime = 45f;
                float fadeOutTime = maxTime - fadeInTime;

                float opacity = 1f;

                if (CreditsTimer <= fadeInTime)
                    opacity = Utils.GetLerpValue(0f, fadeInTime, CreditsTimer, true);
                else if (CreditsTimer >= fadeOutTime)
                    opacity = 1f - Utils.GetLerpValue(fadeOutTime, maxTime, CreditsTimer, true);

                return opacity;
            }
        }

        public const int TotalGIFs = 9;

        public const string Artists = "Arix\nFreeman\nIbanPlay\nPengolin\nReika\nSpicySpaceSnake";

        public const string Musicians = "Pinpin";

        public const string Programmers = "Lucille\nImogen\nNycro\nJavyz";

        public const string Testers1 = "Blast\nBronze\nCataclysmic\nEin\nGamerXD";

        public const string Testers2 = "Gonk\nHabble\nIan\nJareto\nJoey";

        public const string Testers3 = "LGL\nNutella\nMatthionine\nMyra\nPiky";

        public const string Testers4 = "PurpleMattik\nSmh\nShade\nShadine\nTeiull";

        public const string Translators = "Dimension Translate Group\nIndeperevod Team";

        public const string Supporters = "Borb9834\nOptrix\n-Runaway-\nyoshi\nThat1Blade\nConnor";

        public override void Load()
        {
            Main.OnPreDraw += CreditFinalScene.PreparePortraitTarget;
            Main.OnPostDraw += DrawCredits;

            InfernumPlayer.LoadDataEvent += (InfernumPlayer player, TagCompound tag) =>
            {
                player.SetValue<bool>("CreditsHavePlayed", tag.GetBool("CreditsHavePlayed"));
            };

            InfernumPlayer.SaveDataEvent += (InfernumPlayer player, TagCompound tag) =>
            {
                tag["CreditsHavePlayed"] = player.GetValue<bool>("CreditsHavePlayed");
            };
        }

        public override void Unload()
        {
            Main.OnPreDraw -= CreditFinalScene.PreparePortraitTarget;
            Main.OnPostDraw -= DrawCredits;
        }

        public override void PostUpdateDusts() => UpdateCredits();

        public static void BeginCredits()
        {
            // Return if the credits are already playing, or have completed for this player.
            if (CreditsPlaying || Main.LocalPlayer.Infernum().GetValue<bool>("CreditsHavePlayed"))
                return;

            // Else, mark them as playing.
            CurrentState = CreditState.LoadingTextures;
            CreditsTimer = 0;
            ActiveGifIndex = 0;
            CreditsPlaying = true;
            CreditFinalScene.SetupObjects();
        }

        public static void StopAbruptly()
        {
            CurrentState = CreditState.FinalizingDisposing;
            CreditsTimer = 0;
        }

        public static bool ShouldPause() => Main.mapFullscreen || Main.inFancyUI; //|| Main.gamePaused;

        private static void UpdateCredits()
        {
            if (!CreditsPlaying || ShouldPause())
                return;

            float gifTime = 360f * 3f / 1.5f;
            float disposeTime = 1f;
            float fadeInTime = 60f;
            float fadeOutTime = gifTime - fadeInTime;

            switch (CurrentState)
            {
                case CreditState.LoadingTextures:
                    if (CreditsTimer == 0)
                        Main.RunOnMainThread(() => SetupObjects(0));

                    if (CreditsTimer >= 240f)
                    {
                        CurrentState = CreditState.Playing;
                        CreditsTimer = 0;
                    }
                    break;

                case CreditState.Playing:
                    if (CreditsTimer <= gifTime)
                    {
                        if (CreditGIFs.IndexInRange(ActiveGifIndex))
                            CreditGIFs[ActiveGifIndex]?.Update();

                        if (CreditsTimer == disposeTime)
                        {
                            Main.RunOnMainThread(() => SetupObjects(ActiveGifIndex + 1));
                            if (CreditGIFs.IndexInRange(ActiveGifIndex - 1))
                                CreditGIFs[ActiveGifIndex - 1] = null;
                        }

                        if (CreditsTimer >= gifTime)
                        {
                            if (ActiveGifIndex < TotalGIFs)
                            {
                                ActiveGifIndex++;
                                CreditsTimer = 0;
                                return;
                            }
                            else
                            {
                                CreditsTimer = 0;
                                CurrentState = CreditState.FinalScene;
                                CreditsPlaying = true;
                                return;
                            }
                        }
                    }
                    break;

                case CreditState.FinalScene:
                    float maxTime = 720f;
                    CreditFinalScene.Update(CreditsTimer);

                    if (CreditsTimer >= maxTime)
                    {
                        if (Main.netMode != NetmodeID.Server)
                            Main.LocalPlayer.QuickSpawnItem(new EntitySource_WorldEvent(), ModContent.ItemType<CreditPainting>());
                        CreditsTimer = 0;
                        CurrentState = CreditState.FinalizingDisposing;
                        return;
                    }
                    break;

                case CreditState.FinalizingDisposing:
                    if (CreditsTimer >= disposeTime)
                    {
                        // Dispose of all the final textures.
                        if (CreditGIFs.IndexInRange(ActiveGifIndex))
                            CreditGIFs[ActiveGifIndex] = null;
                        // Mark the credits as completed.
                        Main.LocalPlayer.Infernum().SetValue<bool>("CreditsHavePlayed", true);
                        CreditsPlaying = false;
                    }
                    break;
            }

            CreditsTimer++;
        }

        private static void DrawCredits(GameTime gameTime)
        {
            // Only draw if the credits are playing.
            if (!CreditsPlaying || ShouldPause())
                return;

            // This is already ended.
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            if (CurrentState is CreditState.Playing)
            {
                float gifTime = 360f * 3f / 1.5f;
                float fadeInTime = 60f;
                float fadeOutTime = gifTime - fadeInTime;

                if (CreditsTimer <= gifTime)
                {
                    float opacity = 1f;

                    if (CreditsTimer <= fadeInTime)
                        opacity = Utils.GetLerpValue(0f, fadeInTime, CreditsTimer, true);
                    else if (CreditsTimer >= fadeOutTime)
                        opacity = 1f - Utils.GetLerpValue(fadeOutTime, gifTime, CreditsTimer, true);

                    if (CreditGIFs.IndexInRange(ActiveGifIndex))
                        CreditGIFs[ActiveGifIndex]?.DrawNames(opacity);
                }
            }
            else if (CurrentState is CreditState.FinalScene)
                CreditFinalScene.Draw(FinalSceneOpacity);
            Main.spriteBatch.End();
        }

        private static void SetupObjects(int index)
        {
            if (index is 0)
                CreditGIFs = new CreditAnimationObject[TotalGIFs];

            // Leave if the index is out of the range.
            if (!CreditGIFs.IndexInRange(index))
                return;

            new Thread(() =>
            {
                CreditGIFs[index] = new CreditAnimationObject(-Vector2.UnitY * 0.075f, Headers[index], Names[index], HeaderColors[index], index % 2 == 1);
            }).Start();
        }
    }
}
