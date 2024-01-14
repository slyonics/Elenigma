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

        private TitleViewModel titleMenuViewModel;

        private bool savesAvailable;

        public TitleScene()
            : base()
        {
            savesAvailable = GameProfile.SaveList.Count > 0;
        }

        public override void BeginScene()
        {
            base.BeginScene();

            // Audio.PlayMusic(GameMusic.BlastingThroughtheSky);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundColorSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), new Rectangle(0, 0, 1, 1), Color.Black, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);


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
