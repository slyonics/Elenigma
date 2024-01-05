using Elenigma.Main;
using Elenigma.SceneObjects.Maps;
using Elenigma.Scenes.MapScene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elenigma.SceneObjects.Particles
{
    public class EmoteParticle : AnimationParticle
    {
        private Actor host;

        public EmoteParticle(Scene iScene, AnimationType iAnimationType, Actor iHost)
            : base(iScene, iHost.Center + new Vector2(0, -16), iAnimationType, true)
        {
            host = iHost;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            position = host.Center + new Vector2(0, -16 + SpriteBounds.Height / 2);
        }
    }
}
