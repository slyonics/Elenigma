using Elenigma.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elenigma.SceneObjects.Particles
{
    public delegate void FrameFollowup();

    public enum AnimationType
    {
        Exclamation,
        Smoke,
        Gust,
        Miasma
    }

    public class AnimationParticle : Particle
    {
        private static readonly Dictionary<string, Animation> PARTICLE_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { AnimationType.Exclamation.ToString(), new Animation(0, 0, 16, 16, 8, new int[] { 20, 20, 20, 20, 20, 20, 20, 350 }) },
            { AnimationType.Smoke.ToString(), new Animation(0, 0, 32, 32, 7, 50) },
            { AnimationType.Gust.ToString(), new Animation(0, 0, 32, 32, 5, 80) },
            { AnimationType.Miasma.ToString(), new Animation(0, 0, 32, 32, 9, 50) }
        };

        private List<Tuple<int, FrameFollowup>> frameEventList = new List<Tuple<int, FrameFollowup>>();

        public AnimationParticle(Scene iScene, Vector2 iPosition, AnimationType iAnimationType, bool iForeground = false)
            : base(iScene, iPosition, iForeground)
        {
            parentScene = iScene;
            position = iPosition;

            priorityLevel = PriorityLevel.CutsceneLevel;

            var animationName = "Particles_" + iAnimationType;
            var animationSprite = (GameSprite)Enum.Parse(typeof(GameSprite), animationName);
            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[animationSprite], PARTICLE_ANIMATIONS);
            animatedSprite.PlayAnimation(iAnimationType.ToString(), AnimationFinished);

            if (Foreground) position.Y += SpriteBounds.Height / 2;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Tuple<int, FrameFollowup> frameEvent = frameEventList.Find(x => x.Item1 == animatedSprite.Frame);
            if (frameEvent != null)
            {
                frameEvent.Item2();
                frameEventList.Remove(frameEvent);
            }
        }

        public void AnimationFinished()
        {
            Terminate();
        }

        public void AddFrameEvent(int frame, FrameFollowup frameEvent)
        {
            frameEventList.Add(new Tuple<int, FrameFollowup>(frame, frameEvent));
        }
    }
}
