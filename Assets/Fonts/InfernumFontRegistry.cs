﻿using CalamityMod.UI;
using InfernumMode.Common.DataStructures;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernumMode.Assets.Fonts
{
    public class InfernumFontRegistry : ModSystem
    {
        public static LocalizedSpriteFont BossIntroScreensFont
        {
            get;
            private set;
        }

        public static LocalizedSpriteFont HPBarFont
        {
            get;
            private set;
        }

        public static LocalizedSpriteFont ProfanedTextFont
        {
            get;
            private set;
        }

        public override void Load()
        {
            BossIntroScreensFont = new LocalizedSpriteFont(BossHealthBarManager.HPBarFont)
                .WithLanguage(GameCulture.CultureName.Chinese, ModContent.Request<DynamicSpriteFont>("InfernumMode/Assets/Fonts/BossIntroScreensFont", AssetRequestMode.ImmediateLoad).Value)
                .WithLanguage(GameCulture.CultureName.Russian, ModContent.Request<DynamicSpriteFont>("InfernumMode/Assets/Fonts/BossIntroScreensFontRussian", AssetRequestMode.ImmediateLoad).Value);

            HPBarFont = new LocalizedSpriteFont(ModContent.Request<DynamicSpriteFont>("InfernumMode/Assets/Fonts/HPBarFont", AssetRequestMode.ImmediateLoad).Value, GameCulture.CultureName.German,
                GameCulture.CultureName.Italian, GameCulture.CultureName.French, GameCulture.CultureName.Spanish, GameCulture.CultureName.Russian, GameCulture.CultureName.Chinese,
                GameCulture.CultureName.Portuguese, GameCulture.CultureName.Polish);

            ProfanedTextFont = new LocalizedSpriteFont(ModContent.Request<DynamicSpriteFont>("InfernumMode/Assets/Fonts/ProfanedText", AssetRequestMode.ImmediateLoad).Value)
                .WithLanguage(GameCulture.CultureName.Russian, ModContent.Request<DynamicSpriteFont>("InfernumMode/Assets/Fonts/ProfanedTextRussian", AssetRequestMode.ImmediateLoad).Value)
                .WithLanguage(GameCulture.CultureName.Chinese, ModContent.Request<DynamicSpriteFont>("InfernumMode/Assets/Fonts/ProfanedTextChinese", AssetRequestMode.ImmediateLoad).Value);
        }

        public override void Unload()
        {
            BossIntroScreensFont = null;
            HPBarFont = null;
            ProfanedTextFont = null;
        }
    }
}
