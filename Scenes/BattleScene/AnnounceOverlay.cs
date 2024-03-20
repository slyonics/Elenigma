using Elenigma.Scenes.MapScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.BattleScene
{
    public class AnnounceOverlay : Overlay
    {
        private NinePatch textbox;

        private Color color = new Color(252, 224, 168);

        string message;

        int expireTimer = 1000;

        public AnnounceOverlay(string iMessage)
        {
            message = iMessage;

            int boxLength = Text.GetStringLength(GameFont.Main, message) + 16;

            textbox = new NinePatch("BattleWindow", 0.05f);
            textbox.Bounds = new Rectangle((CrossPlatformGame.ScreenWidth - boxLength) / 2, 2, boxLength, 22);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            expireTimer -= gameTime.ElapsedGameTime.Milliseconds;
            if (expireTimer <= 0) Terminate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            textbox.Draw(spriteBatch, Vector2.Zero);
                
            Text.DrawCenteredText(spriteBatch, new Vector2(textbox.Bounds.Center.X + 1, textbox.Bounds.Center.Y), GameFont.Main, message, color, 0.03f);
        }
    }
}
