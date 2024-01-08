using Elenigma.SceneObjects.Maps;
using Elenigma.SceneObjects.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class SummonController : Controller
    {
        private MapScene mapScene;
        private Hero player;
        private SummonOverlay summonOverlay;


        public SummonController(MapScene iMapScene, Hero iPlayer, SummonOverlay iSummonOverlay)
            : base (PriorityLevel.MenuLevel)
        {
            mapScene = iMapScene;
            player = iPlayer;
            summonOverlay = iSummonOverlay;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            base.PreUpdate(gameTime);

            InputFrame inputFrame = Input.CurrentInput;

            if (!inputFrame.CommandDown(Command.Summon))
            {
                Terminate();
                summonOverlay.Terminate();

                Tile closestEmptyTile = mapScene.Tilemap.GetTile(player.Center).NeighborList.First(x => !x.Blocked);

                Hero followerHero = new Spirit(mapScene, mapScene.Tilemap, closestEmptyTile.Center, GameSprite.Actors_Slyph);
                FollowerController followerController = new FollowerController(mapScene, followerHero, player);
                mapScene.AddEntity(followerHero);
                mapScene.AddController(followerController);
                mapScene.AddParticle(new AnimationParticle(mapScene, followerHero.Position, AnimationType.Smoke, true));

                return;
            }

            if (inputFrame.CommandPressed(Command.Left))
            {

            }
            else if (inputFrame.CommandPressed(Command.Right))
            {

            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            base.PostUpdate(gameTime);
        }
    }
}
