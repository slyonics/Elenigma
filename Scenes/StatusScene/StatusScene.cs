using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.StatusScene
{
    public class StatusScene : Scene
    {
        public StatusViewModel StatusViewModel { get; private set; }

        public bool Saved { get; set; } = false;

        public StatusScene(string locationName, bool canSave)
            : base()
        {
            StatusViewModel = AddView(new StatusViewModel(this, locationName, canSave));
            StatusViewModel.OnTerminated += new Action(() => EndScene());
        }

        public override void BeginScene()
        {
            sceneStarted = true;
        }

        public override void ResumeScene()
        {
            base.ResumeScene();

            TransitionController transitionController = new TransitionController(TransitionDirection.In, 600);
            ColorFade colorFade = new ColorFade(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
            transitionController.FinishTransition += new Action<TransitionDirection>(t => colorFade.Terminate());
            AddController(transitionController);
            CrossPlatformGame.TransitionShader = colorFade;
        }
    }
}
