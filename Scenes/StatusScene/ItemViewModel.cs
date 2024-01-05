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

        ItemController itemController;

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

        public void SelectCommand(object parameter)
        {
            UseItem(GetWidget<RadioBox>("ItemList").Selection);
        }

        public void UseItem(int itemSlot)
        {
            ItemModel itemModel = AvailableItems[itemSlot];

            if (!itemModel.FieldUsable || AvailableItems[itemSlot].Quantity.Value < 0)
            {
                Audio.PlaySound(GameSound.Error);
                return;
            }

            Audio.PlaySound(GameSound.Selection);

            if ((AvailableItems[itemSlot].ItemRecord as ItemRecord).ItemType == ItemType.Medicine)
            {
                GetWidget<RadioBox>("ItemList").Enabled = false;
                childViewModel = statusScene.AddView(new SelectionViewModel(statusScene, this, itemSlot));
            }
            else
            {
                itemController = statusScene.AddController(new ItemController(statusScene, itemModel.ItemRecord.FieldScript, null, new Vector2()));
                itemController.OnTerminated += new TerminationFollowup(() =>
                {
                    ConsumeItem(itemSlot);
                });
            }
        }

        public bool ConsumeItem(int itemSlot)
        {
            int slot = GetWidget<RadioBox>("ItemList").Selection;

            if (AvailableItems[itemSlot].Quantity.Value > 0)
            {
                AvailableItems[itemSlot].Quantity.Value = AvailableItems[itemSlot].Quantity.Value - 1;
                if (AvailableItems[itemSlot].Quantity.Value == 0)
                {
                    AvailableItems.RemoveAt(itemSlot);
                    AvailableItems.ModelList = AvailableItems.ModelList;

                    if (slot >= AvailableItems.Count()) slot = AvailableItems.Count() - 1;
                    if (slot != -1)
                    {
                        var itemGrid = GetWidget<RadioBox>("ItemList");
                        if (!Input.MOUSE_MODE)
                        {
                            (itemGrid.ChildList[slot] as RadioButton).RadioSelect();
                            Description.Value = AvailableItems[slot].ItemRecord.Description;
                        }
                        else
                        {
                            Description.Value = "";
                            slot = -1;
                        }

                        if (!itemGrid.IsChildVisible(itemGrid.ChildList[slot])) itemGrid.ScrollUp();

                    }
                    else Description.Value = "";

                    return true;
                }
            }

            return false;
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
