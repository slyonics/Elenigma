using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.IntroScene
{
    public class IntroScene : Scene, ISkippableWait
    {
        public IntroScene()
            : base()
        {
            AddController(new SkippableWaitController(PriorityLevel.GameLevel, this, true, 10000));

            AddView(new IntroViewModel(this, GameView.IntroScene_IntroView));
        }

        public void Notify(SkippableWaitController sender)
        {
            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));

            /*
            if (GameProfile.SaveList.Count > 0)
            {
                CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
            }
            else
            {
                NewGame();
            }
            */
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            base.DrawBackground(spriteBatch);
        }

        public static void NewGame()
        {
            GameProfile.NewState();

            CrossPlatformGame.Transition(typeof(MapScene.MapScene), GameMap.TechWorldIntro, 19, 33, Orientation.Down);
        }

        public bool Terminated { get => false; }
    }
}
