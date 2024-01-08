using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Elenigma.Scenes.TitleScene
{
    public class TitleScene : Scene
    {
        private Texture2D backgroundColorSprite = AssetCache.SPRITES[GameSprite.Background_Splash];
        private Texture2D backgroundSprite = AssetCache.SPRITES[GameSprite.Background_Title];
        private Texture2D titleSprite = AssetCache.SPRITES[GameSprite.Title];
        private Texture2D cloudSprite = AssetCache.SPRITES[GameSprite.Clouds];
        private AnimatedSprite airshipSprite;

        private TitleViewModel titleMenuViewModel;

        private float scrollProgress = 0.0f;
        private float flashProgress = 0.0f;
        private float blinkProgress = 0.0f;

        float cloudOffset;

        private Effect flashEffect;

        private bool skipMenu;
        private bool savesAvailable;
        private bool done;

        public TitleScene()
            : base()
        {

            flashEffect = AssetCache.EFFECTS[GameShader.BattleEnemy].Clone();

            savesAvailable = GameProfile.SaveList.Count > 0;
        }

        public TitleScene(string iSkipMenu)
            : base()
        {
            skipMenu = bool.Parse(iSkipMenu);

            flashEffect = AssetCache.EFFECTS[GameShader.BattleEnemy].Clone();

            savesAvailable = GameProfile.SaveList.Count > 0;
        }

        public override void BeginScene()
        {
            base.BeginScene();

            Audio.PlayMusic(GameMusic.BlastingThroughtheSky);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            airshipSprite.Update(gameTime);

            if (priorityLevel == PriorityLevel.TransitionLevel || done) return;

            if (scrollProgress < 1.0f)
            {
                scrollProgress += gameTime.ElapsedGameTime.Milliseconds / 5000.0f;
                if (scrollProgress >= 1.0f)
                {
                    scrollProgress = 1.0f;
                    flashEffect.Parameters["destroyInterval"].SetValue(1.1f);
                    flashEffect.Parameters["flashColor"].SetValue(Color.White.ToVector4());
                    flashEffect.Parameters["flashInterval"].SetValue(1.0f - flashProgress);
                }
            }
            else if (flashProgress < 1.0f)
            {
                flashProgress += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (flashProgress >= 1.0f) flashProgress = 1.0f;
                flashEffect.Parameters["flashInterval"].SetValue(1.0f - flashProgress);
            }
            else if (titleMenuViewModel == null)
            {
                blinkProgress += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            }

            float cloudSpeed = MathHelper.SmoothStep(0.0f, 20.0f, scrollProgress);
            cloudOffset += gameTime.ElapsedGameTime.Milliseconds / 1000.0f * cloudSpeed;
            while (cloudOffset >= cloudSprite.Width) { cloudOffset -= cloudSprite.Width; }

            if (Input.CurrentInput.CommandPressed(Command.Confirm) && titleMenuViewModel == null)
            {
                titleMenuViewModel = AddView<TitleViewModel>(new TitleViewModel(this, GameView.TitleScene_TitleView));

                scrollProgress = flashProgress = 1.0f;
                flashEffect.Parameters["destroyInterval"].SetValue(1.1f);
                flashEffect.Parameters["flashColor"].SetValue(Color.White.ToVector4());
                flashEffect.Parameters["flashInterval"].SetValue(1.0f - flashProgress);
            }
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundColorSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), new Rectangle(0, 0, 1, 1), Color.Black, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);

            int scrollOffset = (int)MathHelper.SmoothStep(0, 208, scrollProgress);
            spriteBatch.Draw(backgroundSprite, new Vector2(0, scrollOffset - 208), null, Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.8f);

            spriteBatch.Draw(cloudSprite, new Vector2(0 + cloudOffset, scrollOffset - 61), null, Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.7f);
            spriteBatch.Draw(cloudSprite, new Vector2(-cloudSprite.Width + cloudOffset, scrollOffset - 61), null, Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.7f);

            airshipSprite.Draw(spriteBatch, new Vector2(290 - scrollOffset / 1.9f, scrollOffset - 94), null, 0.5f);

            if (scrollProgress >= 1.0f)
            {
                if (flashProgress >= 1.0f && titleMenuViewModel == null && (int)blinkProgress % 2 == 0 && priorityLevel != PriorityLevel.TransitionLevel)
                {
                    Text.DrawCenteredText(spriteBatch, new Vector2(CrossPlatformGame.ScreenWidth / 2, 128), GameFont.Main, "- Press Start");
                    Text.DrawCenteredText(spriteBatch, new Vector2(CrossPlatformGame.ScreenWidth / 2, 128), GameFont.Main, "  or Spacebar -", 1);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, flashEffect, null);
                spriteBatch.Draw(titleSprite, new Vector2(8, 4), null, Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.3f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            }
        }

        public void ResetSettings()
        {
            titleMenuViewModel.Terminate();
            titleMenuViewModel = new TitleViewModel(this, GameView.TitleScene_TitleView);
            AddOverlay(titleMenuViewModel);
            titleMenuViewModel.SettingsMenu();
        }
    }
}
