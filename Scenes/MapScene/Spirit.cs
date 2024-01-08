using Elenigma.Models;
using Elenigma.SceneObjects.Maps;
using Elenigma.SceneObjects.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class Spirit : Hero
    {
        private static readonly Dictionary<string, Animation> SLYPH_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.IdleDown.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.IdleLeft.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.IdleRight.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.IdleUp.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkDown.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkLeft.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkRight.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkUp.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
        };

        private int lifespan = 3000;
        public SummonType SummonType { get; set; }


        public Spirit(MapScene iMapScene, Tilemap iTilemap, Vector2 iPosition, SummonType summonType, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, iPosition, (GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + summonType), summonType == SummonType.Slyph ? SLYPH_ANIMATIONS : HERO_ANIMATIONS, iOrientation)
        {
            SummonType = summonType;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            lifespan -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lifespan < 0)
            {
                Terminate();
                mapScene.AddParticle(new AnimationParticle(mapScene, this.Position, AnimationType.Smoke, true));
            }
        }

        public void RefreshLifespan()
        {
            lifespan = 3000;
        }

        
    }
}
