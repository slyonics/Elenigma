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
        Miasma, 

        Slash,
        Tornado,
        Thunderbolt,
        Stone,
        Bash,
        Freeze,
        Icefall,
        Fireburst,
        Eruption,
        Darkness,
        Cure1,
        Cure2,
        BlueHeal,
        GreenHeal,
        BlackHole,
        Skull,
        Sonic
    }

    public class AnimationParticle : Particle
    {
        private static readonly Dictionary<string, Animation> PARTICLE_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { AnimationType.Exclamation.ToString(), new Animation(0, 0, 16, 16, 8, new int[] { 20, 20, 20, 20, 20, 20, 20, 350 }) },
            { AnimationType.Smoke.ToString(), new Animation(0, 0, 32, 32, 7, 50) },
            { AnimationType.Gust.ToString(), new Animation(0, 0, 32, 32, 5, 80) },
            { AnimationType.Miasma.ToString(), new Animation(0, 0, 32, 32, 9, 50) },

            { AnimationType.Tornado.ToString(), new Animation(0, 0, 64, 64, 6, 100) },
            { AnimationType.Thunderbolt.ToString(), new Animation(0, 0, 64, 64, 3, 100) },
            { AnimationType.Stone.ToString(), new Animation(0, 0, 64, 64, 9, new int[] { 150, 150, 150, 80, 80, 80, 80, 80, 80 }) },
            { AnimationType.Slash.ToString(), new Animation(0, 0, 64, 64, 4, 80) },
            { AnimationType.Bash.ToString(), new Animation(0, 0, 64, 64, 3, 100) },
            { AnimationType.Freeze.ToString(), new Animation(0, 0, 64, 64, 3, new int[] { 150, 450, 300 }) },
            { AnimationType.Icefall.ToString(), new Animation(0, 0, 64, 64, 6, new int[] { 100, 100, 100, 100, 100, 300 }) },
            { AnimationType.Fireburst.ToString(), new Animation(0, 0, 64, 64, 5, 150) },
            { AnimationType.Eruption.ToString(), new Animation(0, 0, 64, 64, 5, new int[] { 300, 200, 100, 100, 100 }) },
            { AnimationType.Darkness.ToString(), new Animation(0, 0, 64, 64, 7, new int[] { 60, 110, 160, 210, 80, 80, 80 }) },
            { AnimationType.Cure1.ToString(), new Animation(0, 0, 64, 64, 5, new int[] { 250, 200, 150, 100, 50 }) },
            { AnimationType.Cure2.ToString(), new Animation(0, 0, 64, 64, 6, new int[] { 200, 150, 100, 100, 100, 100 }) },
            { AnimationType.BlueHeal.ToString(), new Animation(0, 0, 64, 64, 4, 120) },
            { AnimationType.GreenHeal.ToString(), new Animation(0, 0, 64, 64, 4, 120) },
            { AnimationType.BlackHole.ToString(), new Animation(0, 0, 64, 64, 14, 100) },
            { AnimationType.Skull.ToString(), new Animation(0, 0, 64, 64, 3, 400) },
            { AnimationType.Sonic.ToString(), new Animation(0, 0, 64, 64, 6, 140) },
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
