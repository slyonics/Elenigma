using Elenigma.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elenigma.Scenes.CreditsScene
{
    public class CreditsScene : Scene
    {
        private Texture2D splashSprite = AssetCache.SPRITES[GameSprite.Background_Splash];

        private CreditsViewModel creditsViewModel;

        public CreditsScene()
            : base()
        {
            creditsViewModel = new CreditsViewModel(this, GameView.CreditsScene_CreditsView);
            AddOverlay(creditsViewModel);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(splashSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), new Rectangle(0, 0, 1, 1), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(splashSprite, new Rectangle((CrossPlatformGame.ScreenWidth - splashSprite.Width) / 2, (CrossPlatformGame.ScreenHeight - splashSprite.Height) / 2, splashSprite.Width, splashSprite.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
        }
    }
}
