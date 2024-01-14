using Elenigma.Models;
using Elenigma.SceneObjects;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class TargetOverlay : Overlay
    {

        private MapScene mapScene;
        private Vector2 position;

        public TargetOverlay(MapScene iMapScene, Vector2 iPosition)
        {
            mapScene = iMapScene;
            position = iPosition;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(AssetCache.SPRITES[GameSprite.Target], position - mapScene.Camera.Position + new Vector2(-8, -8), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.04f);

        }

        public void Move(Vector2 movement)
        {
            position += movement;
        }

    }
}
