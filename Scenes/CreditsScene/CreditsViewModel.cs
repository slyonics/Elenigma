using Elenigma.Main;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elenigma.Scenes.CreditsScene
{
    public class CreditsViewModel : ViewModel
    {
        public CreditsViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Close();
            }
        }
    }
}
