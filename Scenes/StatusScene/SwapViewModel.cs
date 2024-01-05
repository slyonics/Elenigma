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
    public class SwapViewModel : ViewModel
    {
        private static readonly Color INCREASE_COLOR = new Color(96, 160, 255);
        private static readonly Color DECREASE_COLOR = new Color(224, 80, 0);

        StatusScene statusScene;

        ViewModel childViewModel;

        HeroModel heroModel;
        ItemRecord currentEquipment;

        public HeroModel HeroModel { get => heroModel; }

        EquipViewModel equipViewModel;

        public ModelCollection<ItemModel> AvailableItems { get; private set; } = new ModelCollection<ItemModel>();

        public SwapViewModel(StatusScene iScene, EquipViewModel iEquipViewModel, HeroModel iHeroModel, ItemRecord iCurrentEquipment)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            equipViewModel = iEquipViewModel;
            heroModel = iHeroModel;
            currentEquipment = iCurrentEquipment;

            AvailableItems.ModelList = GameProfile.Inventory.ModelList.FindAll(x =>
            ((ItemRecord)x.Value.ItemRecord).ItemType == currentEquipment.ItemType && ((ItemRecord)x.Value.ItemRecord).UsableBy.Contains(heroModel.Class.Value));

            if (currentEquipment.Name != "- EMPTY -")
            {
                var dummyRecord = new ItemRecord()
                {
                    Icon = "Blank",
                    Name = "- REMOVE -",
                    Description = "Remove this piece of equipment."
                };

                if (currentEquipment.ItemType == ItemType.Weapon)
                {
                    dummyRecord.Attack = 1;
                    dummyRecord.Hit = 90;
                }

                AvailableItems.ModelList.Add(new ModelProperty<ItemModel>(new ItemModel(dummyRecord)));
            }

            if (!Input.MOUSE_MODE) UpdateDescription(0);

            LoadView(GameView.StatusScene_SwapView);

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

        public void SelectEquipment(object parameter)
        {
            ItemModel itemEntry = (ItemModel)((IModelProperty)parameter).GetValue();
            ItemRecord itemRecord = (ItemRecord)itemEntry.ItemRecord;

            if (itemEntry.ItemRecord.Name == "- REMOVE -")
            {
                heroModel.Remove(currentEquipment.ItemType);
                GameProfile.Inventory.Add(new ItemModel(currentEquipment));
            }
            else
            {
                heroModel.Equip(itemRecord);

                if (currentEquipment.Name == "- EMPTY -")
                {
                    var modelProperty = GameProfile.Inventory.First(x => x.Value == itemEntry);
                    GameProfile.Inventory.Remove(modelProperty);
                }
                else itemEntry.ItemRecord = currentEquipment;
            };

            equipViewModel.UpdateEquipment();

            Terminate();
        }

        public void UpdateDescription(int slot)
        {
            if (slot >= AvailableItems.Count()) Description.Value = "";
            else
            {
                Description.Value = AvailableItems[slot].ItemRecord.Description;

                ItemRecord itemRecord = (ItemRecord)AvailableItems[slot].ItemRecord;
                switch (currentEquipment.ItemType)
                {
                    case ItemType.Weapon:
                        equipViewModel.StatLabel1.Value = "Attack";
                        equipViewModel.StatLabel2.Value = "Hit %";
                        equipViewModel.StatLabel3.Value = "Weight";
                        equipViewModel.StatValue1.Value = itemRecord.Attack.ToString();
                        equipViewModel.StatValue2.Value = itemRecord.Hit.ToString();
                        equipViewModel.StatValue3.Value = itemRecord.Weight.ToString();

                        if (itemRecord.Attack > (heroModel.Weapon.Value != null ? heroModel.Weapon.Value.Attack : 1)) equipViewModel.StatColor1.Value = INCREASE_COLOR;
                        else if (itemRecord.Attack < (heroModel.Weapon.Value != null ? heroModel.Weapon.Value.Attack : 1)) equipViewModel.StatColor1.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor1.Value = Color.White;

                        if (itemRecord.Hit > (heroModel.Weapon.Value != null ? heroModel.Weapon.Value.Hit : 90)) equipViewModel.StatColor2.Value = INCREASE_COLOR;
                        else if (itemRecord.Hit < (heroModel.Weapon.Value != null ? heroModel.Weapon.Value.Hit : 90)) equipViewModel.StatColor2.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor2.Value = Color.White;

                        if (itemRecord.Weight < (heroModel.Weapon.Value != null ? heroModel.Weapon.Value.Weight : 0)) equipViewModel.StatColor3.Value = INCREASE_COLOR;
                        else if (itemRecord.Weight > (heroModel.Weapon.Value != null ? heroModel.Weapon.Value.Weight : 0)) equipViewModel.StatColor3.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor3.Value = Color.White;

                        break;

                    case ItemType.Shield:
                        equipViewModel.StatLabel1.Value = "Evade";
                        equipViewModel.StatLabel2.Value = "M. Evade";
                        equipViewModel.StatLabel3.Value = "Weight";
                        equipViewModel.StatValue1.Value = itemRecord.Evade.ToString();
                        equipViewModel.StatValue2.Value = itemRecord.MagicEvade.ToString();
                        equipViewModel.StatValue3.Value = itemRecord.Weight.ToString();

                        if (itemRecord.Evade > (heroModel.Shield.Value != null ? heroModel.Shield.Value.Evade : 0)) equipViewModel.StatColor1.Value = INCREASE_COLOR;
                        else if (itemRecord.Evade < (heroModel.Shield.Value != null ? heroModel.Shield.Value.Evade : 0)) equipViewModel.StatColor1.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor1.Value = Color.White;

                        if (itemRecord.MagicEvade > (heroModel.Shield.Value != null ? heroModel.Shield.Value.MagicEvade : 0)) equipViewModel.StatColor2.Value = INCREASE_COLOR;
                        else if (itemRecord.MagicEvade < (heroModel.Shield.Value != null ? heroModel.Shield.Value.MagicEvade : 0)) equipViewModel.StatColor2.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor2.Value = Color.White;

                        if (itemRecord.Weight < (heroModel.Shield.Value != null ? heroModel.Shield.Value.Weight : 0)) equipViewModel.StatColor3.Value = INCREASE_COLOR;
                        else if (itemRecord.Weight > (heroModel.Shield.Value != null ? heroModel.Shield.Value.Weight : 0)) equipViewModel.StatColor3.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor3.Value = Color.White;

                        break;

                    case ItemType.Armor:
                        equipViewModel.StatLabel1.Value = "Defense";
                        equipViewModel.StatLabel2.Value = "M. Defense";
                        equipViewModel.StatLabel3.Value = "Weight";
                        equipViewModel.StatValue1.Value = itemRecord.Defense.ToString();
                        equipViewModel.StatValue2.Value = itemRecord.MagicDefense.ToString();
                        equipViewModel.StatValue3.Value = itemRecord.Weight.ToString();

                        if (itemRecord.Defense > (heroModel.Armor.Value != null ? heroModel.Armor.Value.Defense : 0)) equipViewModel.StatColor1.Value = INCREASE_COLOR;
                        else if (itemRecord.Defense < (heroModel.Armor.Value != null ? heroModel.Armor.Value.Defense : 0)) equipViewModel.StatColor1.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor1.Value = Color.White;

                        if (itemRecord.MagicDefense > (heroModel.Armor.Value != null ? heroModel.Armor.Value.MagicDefense : 0)) equipViewModel.StatColor2.Value = INCREASE_COLOR;
                        else if (itemRecord.MagicDefense < (heroModel.Armor.Value != null ? heroModel.Armor.Value.MagicDefense : 0)) equipViewModel.StatColor2.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor2.Value = Color.White;

                        if (itemRecord.Weight < (heroModel.Armor.Value != null ? heroModel.Armor.Value.Weight : 0)) equipViewModel.StatColor3.Value = INCREASE_COLOR;
                        else if (itemRecord.Weight > (heroModel.Armor.Value != null ? heroModel.Armor.Value.Weight : 0)) equipViewModel.StatColor3.Value = DECREASE_COLOR;
                        else equipViewModel.StatColor3.Value = Color.White;

                        break;
                }
            }
        }

        public ModelProperty<string> Description { get; set; } = new ModelProperty<string>("");
    }
}
