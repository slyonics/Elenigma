using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Elenigma.Scenes.ConversationScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.StatusScene
{
    public class StatsViewModel : ViewModel
    {
        StatusScene statusScene;

        HeroModel heroModel;

        public HeroModel Hero { get => heroModel; }

        public ModelCollection<ItemRecord> Equipment { get; private set; } = new ModelCollection<ItemRecord>();

        public StatsViewModel(StatusScene iScene, HeroModel iHeroModel)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            heroModel = iHeroModel;

            LoadView(GameView.StatusScene_StatsView);
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
