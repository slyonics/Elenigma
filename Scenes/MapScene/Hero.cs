using Elenigma.SceneObjects.Maps;


using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class Hero : Actor
    {
        protected enum HeroAnimation
        {
            IdleDown,
            IdleLeft,
            IdleRight,
            IdleUp,
            WalkDown,
            WalkLeft,
            WalkRight,
            WalkUp,
            RunDown,
            RunLeft,
            RunRight,
            RunUp
        }

        public const int HERO_WIDTH = 32;
        public const int HERO_HEIGHT = 32;

        public static readonly Rectangle HERO_BOUNDS = new Rectangle(-7, -8, 13, 7);

        private static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.IdleDown.ToString(), new Animation(1, 0, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleLeft.ToString(), new Animation(1, 3, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleRight.ToString(), new Animation(1, 2, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleUp.ToString(), new Animation(1, 1, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.WalkDown.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkLeft.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkRight.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkUp.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.RunDown.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 120) },
            { HeroAnimation.RunLeft.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 4, 120) },
            { HeroAnimation.RunRight.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 4, 120) },
            { HeroAnimation.RunUp.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 4, 120) }
        };

        private MapScene mapScene;

        // private SceneObjects.Shaders.Light light;

        public Hero(MapScene iMapScene, Tilemap iTilemap, Vector2 iPosition, GameSprite gameSprite, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, iPosition, AssetCache.SPRITES[gameSprite], HERO_ANIMATIONS, HERO_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            /*
            if (mapScene.SceneShader != null && mapScene.SceneShader is SceneObjects.Shaders.DayNight)
            {
                light = new SceneObjects.Shaders.Light(position - new Vector2(0, 6), 0.0f);
                light.Color = Color.AntiqueWhite;
                light.Intensity = 50;
                (mapScene.SceneShader as SceneObjects.Shaders.DayNight).Lights.Add(light);
            }
            */
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void CenterOn(Vector2 destination)
        {
            base.CenterOn(destination);
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            base.Draw(spriteBatch, camera);

            if (Settings.GetProgramSetting<bool>("DebugMode"))
                Debug.DrawBox(spriteBatch, InteractionZone);
        }

        public void ChangeSprite(Texture2D newSprite)
        {
            AnimatedSprite.SpriteTexture = newSprite;
        }

        public Rectangle InteractionZone;

        public bool Running { get; set; }

        public Tile HostTile { get; set; }
    }
}
