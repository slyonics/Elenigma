using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elenigma.Main
{
    public enum GameFont
    {
        Tooltip,
        Dialogue,
        Interface,
        Label,
        Main
    }

    public static class Text
    {
        public class GameFontData
        {
            public string fontFile;
            public float fontSize;
            public int fontHeight;
            public int heightOffset;
        }

        private const float TEXT_DEPTH = 0.1f;

        public static readonly Dictionary<GameFont, GameFontData> FONT_DATA = new Dictionary<GameFont, GameFontData>()
        {
            { GameFont.Dialogue, new GameFontData() { fontFile = "NanoPlus", fontSize = 17, fontHeight = 17, heightOffset = 0 } },
            { GameFont.Interface, new GameFontData() { fontFile = "Ridiculousdot-H6", fontSize = 6, fontHeight = 8 } },
            { GameFont.Label, new GameFontData() { fontFile = "Futuradot-H10", fontSize = 10, fontHeight = 8 } },
            { GameFont.Tooltip, new GameFontData() { fontFile = "Futuradot-H10", fontSize = 10, fontHeight = 10 } },
            { GameFont.Main, new GameFontData() { fontFile = "retro-pixel-thick", fontSize = 20.0f, fontHeight = 16, heightOffset = 1 } },
        };

        public static readonly Dictionary<GameFont, SpriteFont> GAME_FONTS = new Dictionary<GameFont, SpriteFont>();

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            Dictionary<string, byte[]> rawFonts = new Dictionary<string, byte[]>();
            List<Tuple<byte[], byte[]>> assetData = AssetCache.LoadAssetData("Fonts.jam");
            foreach (Tuple<byte[], byte[]> asset in assetData)
            {
                string fontName = Encoding.ASCII.GetString(asset.Item1);
                rawFonts.Add(fontName, asset.Item2);
            }

            foreach (KeyValuePair<GameFont, GameFontData> fontEntry in FONT_DATA)
            {
                TtfFontBakerResult bakedFont = TtfFontBaker.Bake(rawFonts[fontEntry.Value.fontFile], fontEntry.Value.fontSize, 1024, 1024, new[] { CharacterRange.BasicLatin });
                GAME_FONTS.Add(fontEntry.Key, bakedFont.CreateSpriteFont(graphicsDevice));
            }
        }

        public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, int row = 0)
        {
            Vector2 offset = new Vector2(0, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
        }

        public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, int row = 0)
        {
            Vector2 offset = new Vector2(0, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }

        public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, int row = 0)
        {
            Vector2 offset = new Vector2(0, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
        }

        public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, float depth, int row = 0)
        {
            Vector2 offset = new Vector2(0, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }

        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
        }

        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
        }

        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, float depth, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + FONT_DATA[font].heightOffset + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }


        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }

        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, Color color, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }

        public static int GetStringLength(GameFont font, string text)
        {
            return (int)GAME_FONTS[font].MeasureString(text).X;
        }

        public static int GetStringHeight(GameFont font)
        {
            return FONT_DATA[font].fontHeight;
        }
    }
}
