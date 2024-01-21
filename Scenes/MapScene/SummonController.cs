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

        private bool ignoreFirstSummonKey = true;


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

            if (ignoreFirstSummonKey && inputFrame.CommandPressed(Command.Summon)) return;
            else ignoreFirstSummonKey = false;

            if (summonOverlay.Scrolling) return;

            if (inputFrame.CommandPressed(Command.Cancel) || inputFrame.CommandPressed(Command.Summon))
            {
                Terminate();
                summonOverlay.Terminate();

                return;
            }
            
            if (inputFrame.CommandPressed(Command.Confirm))
            {
                Terminate();
                summonOverlay.Terminate();

                mapScene.Summon(summonOverlay.SummonSelection);

                return;
            }

            if (inputFrame.CommandPressed(Command.Left))
            {
                summonOverlay.ScrollLeft();
            }
            else if (inputFrame.CommandPressed(Command.Right))
            {
                summonOverlay.ScrollRight();
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            base.PostUpdate(gameTime);
        }
    }
}
