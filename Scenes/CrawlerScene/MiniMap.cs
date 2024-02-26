using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.CrawlerScene
{
    public class MiniMap : Overlay
    {
        NinePatch patch;

        public MiniMap()
        {
            patch = new NinePatch(AssetCache.SPRITES[GameSprite.Widgets_ThinPanel], 0.8f);
            patch.Bounds = new Rectangle(CrossPlatformGame.ScreenWidth - 132, 12, 120, 120);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            patch.Draw(spriteBatch, Vector2.Zero);
        }
    }
}
