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
    public class CrystalViewModel : ViewModel
    {
        List<ViewModel> SubViews { get; set; } = new List<ViewModel>();

        CrystalScene statusScene;

        RadioBox commandBox;

        public ViewModel ChildViewModel { get; set; }

        public HeroModel Hero { get; set; }

        public CrystalViewModel(CrystalScene iScene, HeroModel heroModel)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            Hero = heroModel;

            AvailableCommands.Add("Reclass");
            AvailableCommands.Add("Upgrade");

            Narration.Value = Hero.Name + " gazes into the crystal and sees";

            LoadView(GameView.CrystalScene_CrystalView);

            commandBox = GetWidget<RadioBox>("CommandBox");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ChildViewModel != null)
            {
                if (ChildViewModel.Terminated)
                {
                    commandBox.Enabled = true;
                    ChildViewModel = null;
                }
                return;
            }

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);

                if (commandBox.Enabled) Close();
                else
                {
                    commandBox.Enabled = true;
                }
            }
        }

        public void SelectCommand(object parameter)
        {
            string command;

            if (parameter is IModelProperty)
            {
                command = (string)((IModelProperty)parameter).GetValue();
            }
            else command = (string)parameter;

            switch (command)
            {
                case "Reclass":
                    commandBox.Enabled = false;
                    ChildViewModel = statusScene.AddView(new ClassViewModel(statusScene, Hero));
                    break;
            }
        }

        public ModelCollection<string> AvailableCommands { get; set; } = new ModelCollection<string>();

        public ModelProperty<string> Narration { get; set; } = new ModelProperty<string>();

        public string Header { get => Hero.Name + "'s Crystal"; }
    }
}
