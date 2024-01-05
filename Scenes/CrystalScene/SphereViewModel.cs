using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.CrystalScene
{
    public class SphereViewModel : ViewModel
    {
        CrystalScene statusScene;

        public SphereViewModel(CrystalScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            LoadView(GameView.CrystalScene_SphereView);
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
