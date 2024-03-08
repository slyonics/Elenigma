using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects;
using Elenigma.SceneObjects.Maps;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Elenigma.Scenes.TitleScene
{
    public class TitleScene : Scene
    {
        private Texture2D backgroundColorSprite = AssetCache.SPRITES[GameSprite.Background_Splash];
        private Texture2D titleSprite = AssetCache.SPRITES[GameSprite.Background_Title];

        private TitleViewModel titleMenuViewModel;

        private float blinkProgress = 0.0f;

        private bool savesAvailable;

        public TitleScene()
            : base()
        {
            savesAvailable = GameProfile.SaveList.Count > 0;
        }

        public override void BeginScene()
        {
            base.BeginScene();

            Audio.PlayMusic(GameMusic.Elenigma);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (titleMenuViewModel == null)
            {
                blinkProgress += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

                if (Input.CurrentInput.AnythingPressed())
                {
                    titleMenuViewModel = AddView<TitleViewModel>(new TitleViewModel(this, GameView.TitleScene_TitleView));
                }
            }

        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundColorSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), new Rectangle(0, 0, 1, 1), Color.Black, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(titleSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);

            if (titleMenuViewModel == null && (int)blinkProgress % 2 == 0 && priorityLevel != PriorityLevel.TransitionLevel)
            {
                Text.DrawCenteredText(spriteBatch, new Vector2(CrossPlatformGame.ScreenWidth / 2, CrossPlatformGame.ScreenHeight / 2 + 30), GameFont.Main, "- Press Any Button -");
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
