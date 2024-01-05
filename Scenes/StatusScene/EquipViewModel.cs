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
    public class EquipViewModel : ViewModel
    {
        StatusScene statusScene;

        ViewModel childViewModel;

        HeroModel heroModel;

        public HeroModel HeroModel { get => heroModel; }

        public ModelCollection<ItemRecord> Equipment { get; private set; } = new ModelCollection<ItemRecord>();

        public EquipViewModel(StatusScene iScene, HeroModel iHeroModel)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            heroModel = iHeroModel;

            if (heroModel.Weapon.Value == null) Equipment.Add(new ItemRecord() { Icon = "Blank", Name = "- EMPTY -", ItemType = ItemType.Weapon });
            else Equipment.Add(heroModel.Weapon.Value);

            if (heroModel.Shield.Value == null) Equipment.Add(new ItemRecord() { Icon = "Blank", Name = "- EMPTY -", ItemType = ItemType.Shield });
            else Equipment.Add(heroModel.Shield.Value);

            if (heroModel.Armor.Value == null) Equipment.Add(new ItemRecord() { Icon = "Blank", Name = "- EMPTY -", ItemType = ItemType.Armor });
            else Equipment.Add(heroModel.Armor.Value);

            if (heroModel.Accessory.Value == null) Equipment.Add(new ItemRecord() { Icon = "Blank", Name = "- EMPTY -", ItemType = ItemType.Accessory });
            else Equipment.Add(heroModel.Accessory.Value);

            if (!Input.MOUSE_MODE) UpdateDescription(0);

            LoadView(GameView.StatusScene_EquipView);
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
                    UpdateStats(GetWidget<RadioBox>("ItemList").Selection);
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
            var currentEquipment = (ItemRecord)((IModelProperty)parameter).GetValue();

            GetWidget<RadioBox>("ItemList").Enabled = false;

            childViewModel = statusScene.AddView(new SwapViewModel(statusScene, this, heroModel, currentEquipment));
        }

        public void UpdateEquipment()
        {
            int slot = GetWidget<RadioBox>("ItemList").Selection;

            List<ModelProperty<ItemRecord>> newEquipment = new List<ModelProperty<ItemRecord>>();

            if (heroModel.Weapon.Value == null) newEquipment.Add(new ModelProperty<ItemRecord>(new ItemRecord() { Icon = "Blank", Name = "- EMPTY -", ItemType = ItemType.Weapon }));
            else newEquipment.Add(new ModelProperty<ItemRecord>(heroModel.Weapon.Value));

            if (heroModel.Shield.Value == null) newEquipment.Add(new ModelProperty<ItemRecord>(new ItemRecord() { Icon = "Blank", Name = "- EMPTY -", ItemType = ItemType.Shield }));
            else newEquipment.Add(new ModelProperty<ItemRecord>(heroModel.Shield.Value));

            if (heroModel.Armor.Value == null) newEquipment.Add(new ModelProperty<ItemRecord>(new ItemRecord() { Icon = "Blank", Name = "- EMPTY -", ItemType = ItemType.Armor }));
            else newEquipment.Add(new ModelProperty<ItemRecord>(heroModel.Armor.Value));

            if (heroModel.Accessory.Value == null) newEquipment.Add(new ModelProperty<ItemRecord>(new ItemRecord() { Icon = "Blank", Name = "- EMPTY -", ItemType = ItemType.Accessory }));
            else newEquipment.Add(new ModelProperty<ItemRecord>(heroModel.Accessory.Value));

            Equipment.ModelList = newEquipment;

            UpdateDescription(slot);
            ((RadioButton)GetWidget<RadioBox>("ItemList").ChildList[slot]).RadioSelect();
        }


        public void UpdateDescription(int slot)
        {
            if (slot == -1) Description.Value = "";

            string description = Equipment[slot].Description;
            if (string.IsNullOrEmpty(description)) description = "";
            Description.Value = description;

            UpdateStats(slot);
        }

        public void UpdateStats(int slot)
        {
            switch (slot)
            {
                case -1:
                case 3:
                    StatLabel1.Value = "";
                    StatLabel2.Value = "";
                    StatLabel3.Value = "";
                    StatValue1.Value = "";
                    StatValue2.Value = "";
                    StatValue3.Value = "";
                    break;

                case 0:
                    StatLabel1.Value = "Attack";
                    StatLabel2.Value = "Hit %";
                    StatLabel3.Value = "Weight";
                    StatValue1.Value = (heroModel.Weapon.Value != null) ? heroModel.Weapon.Value.Attack.ToString() : "1";
                    StatValue2.Value = (heroModel.Weapon.Value != null) ? heroModel.Weapon.Value.Hit.ToString() : "90";
                    StatValue3.Value = (heroModel.Weapon.Value != null) ? heroModel.Weapon.Value.Weight.ToString() : "0";
                    break;

                case 1:
                    StatLabel1.Value = "Evade";
                    StatLabel2.Value = "M. Evade";
                    StatLabel3.Value = "Weight";
                    StatValue1.Value = (heroModel.Shield.Value != null) ? heroModel.Shield.Value.Evade.ToString() : "0";
                    StatValue2.Value = (heroModel.Shield.Value != null) ? heroModel.Shield.Value.MagicEvade.ToString() : "0";
                    StatValue3.Value = (heroModel.Shield.Value != null) ? heroModel.Shield.Value.Weight.ToString() : "0";
                    break;

                case 2:
                    StatLabel1.Value = "Defense";
                    StatLabel2.Value = "M. Defense";
                    StatLabel3.Value = "Weight";
                    StatValue1.Value = (heroModel.Armor.Value != null) ? heroModel.Armor.Value.Defense.ToString() : "0";
                    StatValue2.Value = (heroModel.Armor.Value != null) ? heroModel.Armor.Value.MagicDefense.ToString() : "0";
                    StatValue3.Value = (heroModel.Armor.Value != null) ? heroModel.Armor.Value.Weight.ToString() : "0";
                    break;
            }

            StatColor1.Value = Color.White;
            StatColor2.Value = Color.White;
            StatColor3.Value = Color.White;
        }

        public ModelProperty<string> Description { get; set; } = new ModelProperty<string>("");

        public ModelProperty<string> StatLabel1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> StatLabel2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> StatLabel3 { get; set; } = new ModelProperty<string>("");

        public ModelProperty<string> StatValue1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> StatValue2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> StatValue3 { get; set; } = new ModelProperty<string>("");

        public ModelProperty<Color> StatColor1 { get; set; } = new ModelProperty<Color>(Color.White);
        public ModelProperty<Color> StatColor2 { get; set; } = new ModelProperty<Color>(Color.White);
        public ModelProperty<Color> StatColor3 { get; set; } = new ModelProperty<Color>(Color.White);
    }
}
