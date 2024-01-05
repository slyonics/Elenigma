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
    public class SkillViewModel : ViewModel
    {
        StatusScene statusScene;

        ViewModel childViewModel;

        ItemController itemController;

        HeroModel heroModel;

        public HeroModel HeroModel { get => heroModel; }
        public ModelProperty<int> CurrentMP { get => heroModel.MP; }

        public ModelCollection<AbilityRecord> AvailableItems { get => heroModel.Abilities; }

        public SkillViewModel(StatusScene iScene, HeroModel iHeroModel)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            heroModel = iHeroModel;

            LoadView(GameView.StatusScene_SkillView);

            if (AvailableItems.Count() > 0) UpdateDescription(0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (childViewModel != null)
            {
                if (childViewModel.Terminated)
                {
                    childViewModel = null;
                    GetWidget<RadioBox>("ItemList").Enabled = true;
                }
                else return;
            }

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Close();
            }
        }

        public void SelectCommand(object parameter)
        {
            UseAbility(GetWidget<RadioBox>("ItemList").Selection);
        }

        public void UseAbility(int itemSlot)
        {
            AbilityRecord itemModel = AvailableItems[itemSlot];

            if (itemModel.FieldScript == null || AvailableItems[itemSlot].Cost > heroModel.MP.Value)
            {
                Audio.PlaySound(GameSound.Error);
                return;
            }

            Audio.PlaySound(GameSound.Selection);

            GetWidget<RadioBox>("ItemList").Enabled = false;
            childViewModel = statusScene.AddView(new SelectionViewModel(statusScene, this, heroModel, itemSlot));
        }

        public bool ConsumeItem(int itemSlot)
        {
            int slot = GetWidget<RadioBox>("ItemList").Selection;

            return heroModel.MP.Value < AvailableItems[itemSlot].Cost;
        }

        public void UpdateDescription(int slot)
        {
            Description.Value = AvailableItems[slot].Description;
        }

        public ModelProperty<string> Description { get; set; } = new ModelProperty<string>("");
    }
}
