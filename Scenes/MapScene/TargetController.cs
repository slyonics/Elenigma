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
    public class TargetController : Controller
    {
        private MapScene mapScene;
        private Hero player;
        private Spirit summon;

        private bool ignoreFirstConfirmKey = true;

        private TargetOverlay targetOverlay;


        public TargetController(MapScene iMapScene, Hero iPlayer, Spirit iSummon, TargetOverlay iTargetOverlay)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            player = iPlayer;
            summon = iSummon;
            targetOverlay = iTargetOverlay;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            base.PreUpdate(gameTime);

            summon.RefreshLifespan();

            InputFrame inputFrame = Input.CurrentInput;

            if (ignoreFirstConfirmKey && inputFrame.CommandPressed(Command.Confirm)) return;
            else ignoreFirstConfirmKey = false;

            if (inputFrame.CommandPressed(Command.Cancel))
            {
                Terminate();
                targetOverlay.Terminate();

                return;
            }
            
            if (inputFrame.CommandPressed(Command.Confirm))
            {
                Terminate();
                targetOverlay.Terminate();

                //mapScene.Summon(summonOverlay.SummonSelection);

                return;
            }

            Vector2 movement = new Vector2(inputFrame.AxisX, inputFrame.AxisY);
            if (movement.Length() > 0.0001f) movement.Normalize();
            if (Input.LeftMouseState == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
                Input.MousePosition.X >= 0 && Input.MousePosition.Y >= 0 && Input.MousePosition.X < CrossPlatformGame.ScreenWidth && Input.MousePosition.Y < CrossPlatformGame.ScreenHeight)
            {
                movement = Input.MousePosition + mapScene.Camera.Position - player.Position + new Vector2(mapScene.Camera.CenteringOffsetX, mapScene.Camera.CenteringOffsetY);
                if (movement.Length() > 1.0f) movement.Normalize();
                else movement = Vector2.Zero;
            }
            if (inputFrame.CommandDown(Command.Left)) movement.X -= 1.0f;
            if (inputFrame.CommandDown(Command.Right)) movement.X += 1.0f;
            if (inputFrame.CommandDown(Command.Up)) movement.Y -= 1.0f;
            if (inputFrame.CommandDown(Command.Down)) movement.Y += 1.0f;

            targetOverlay.Move(movement);
        }

        public override void PostUpdate(GameTime gameTime)
        {
            base.PostUpdate(gameTime);
        }
    }
}
