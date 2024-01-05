using Elenigma.Main;
using Elenigma.SceneObjects.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elenigma.Scenes.BattleScene
{
    public class WeaponParticle : Particle
    {
        private static readonly Dictionary<string, Animation> PARTICLE_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Dagger", new Animation(0, 0, 32, 21, 3, new int[] { 100, 100, 300 }) },
            { "Sword", new Animation(0, 1, 32, 21, 3, new int[] { 100, 100, 300 }) },
            { "Machete", new Animation(0, 2, 32, 21, 3, new int[] { 100, 100, 300 }) },
            { "Axe", new Animation(0, 3, 32, 21, 3, new int[] { 100, 100, 300 }) },
            { "Staff", new Animation(0, 4, 32, 21, 3, new int[] { 100, 100, 300 }) },
            { "Whip", new Animation(0, 5, 32, 21, 3, new int[] { 100, 100, 300 }) },
            { "Bow", new Animation(3, 0, 32, 21, 3, new int[] { 500, 50, 200 }) },
            { "Mace", new Animation(3, 2, 32, 21, 3, new int[] { 100, 100, 300 }) },
            { "Club", new Animation(3, 4, 32, 21, 3, new int[] { 100, 100, 300 }) }
        };

        BattlePlayer host;
        int lastFrame = 0;
        string animationType;

        public WeaponParticle(Scene iScene, BattlePlayer iHost, string iAnimationType, bool iForeground = false)
            : base(iScene, iHost.TopLeft + new Vector2(8, 10), iForeground)
        {
            parentScene = iScene;
            host = iHost;
            animationType = iAnimationType;
            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[GameSprite.Particles_Weapons], PARTICLE_ANIMATIONS);
            animatedSprite.PlayAnimation(iAnimationType.ToString(), AnimationFinished);

            if (Foreground) position.Y += SpriteBounds.Height / 2;

            if (animationType == "Bow")
            {
                position += new Vector2(-16, 0);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (animationType == "Bow") return;

            if (animatedSprite.Frame == 1 && lastFrame == 0)
            {
                position += new Vector2(-16, 0);
                lastFrame = 1;
            }
            else if (animatedSprite.Frame == 2 && lastFrame == 1)
            {
                position += new Vector2(1, 0);
                lastFrame = 2;
            }
        }

        public void AnimationFinished()
        {
            Terminate();
        }
    }
}
