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
    public class ItemViewModel : ViewModel
    {
        StatusScene statusScene;

        ViewModel childViewModel;

        public ModelCollection<ItemModel> AvailableItems { get => GameProfile.Inventory; }

        public ItemViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            LoadView(GameView.StatusScene_ItemView);

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

        public void UpdateDescription(int slot)
        {
            if (Description.Value != AvailableItems[slot].ItemRecord.Description)
            {
                Description.Value = AvailableItems[slot].ItemRecord.Description;
            }
        }

        public ModelProperty<string> Description { get; set; } = new ModelProperty<string>("");
    }
}
