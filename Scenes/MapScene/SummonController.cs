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
        private SummonOverlay summonOverlay;


        public SummonController(MapScene iMapScene, SummonOverlay iSummonOverlay)
            : base (PriorityLevel.MenuLevel)
        {
            mapScene = iMapScene;
            summonOverlay = iSummonOverlay;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            base.PreUpdate(gameTime);

            InputFrame inputFrame = Input.CurrentInput;

            if (!inputFrame.CommandDown(Command.Summon))
            {
                Terminate();
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
