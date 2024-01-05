using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Elenigma.Scenes.MapScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.CrystalScene
{
    public class ClassEntry
    {
        public ClassType Class { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public class ClassViewModel : ViewModel
    {
        CrystalScene statusScene;

        RadioBox classBox;


        public HeroModel Hero { get; set; }

        public ClassViewModel(CrystalScene iScene, HeroModel heroModel)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            Hero = heroModel;

            foreach (var classProfile in heroModel.ClassProfiles)
            {
                ClassEntry classEntry = new ClassEntry()
                {
                    Class = classProfile.Value.Class,
                    Enabled = classProfile.Value.Upgrades.Count > 0
                };

                AvailableClasses.Add(classEntry);
            }

            LoadView(GameView.CrystalScene_ClassView);

            classBox = GetWidget<RadioBox>("ClassBox");
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

        public void SelectClass(object parameter)
        {
            ClassType classType;

            if (parameter is IModelProperty)
            {
                classType = (ClassType)((IModelProperty)parameter).GetValue();
            }
            else classType = (ClassType)parameter;

            Hero.ChangeClass(classType);
            var hero = MapScene.MapScene.Instance.Party.First(x => x.HeroModel == Hero);
            hero.UpdateSprite();

            Close();
        }

        public ModelCollection<ClassEntry> AvailableClasses { get; set; } = new ModelCollection<ClassEntry>();
    }
}
