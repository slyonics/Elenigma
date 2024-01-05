using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Elenigma.Scenes.StatusScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.ShopScene
{
    public class ShopViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) },
        };

        ShopScene shopScene;
        ShopRecord shopRecord;

        QuantityViewModel childViewModel;

        int Slot { get => GetWidget<RadioBox>("EntryList").Selection; }

        public ShopViewModel(ShopScene iScene, ShopRecord iShopRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            shopScene = iScene;
            shopRecord = iShopRecord;

            foreach (var voucher in shopRecord.Vouchers)
            {
                AvailableEntries.Add(voucher);
            }


            LoadView(GameView.ShopScene_ShopView);

            Narration.Value = shopRecord.Intro;

            UpdateDescription(0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var input = Input.CurrentInput;
            if (childViewModel == null)
            {
                if (input.CommandPressed(Command.Cancel))
                {
                    Audio.PlaySound(GameSound.Back);
                    Terminate();
                }
            }
            else
            {
                if (childViewModel.Terminated)
                {
                    if (childViewModel.Confirmed)
                    {
                        Narration.Value = "Thank you for your purchase!";
                    }
                    childViewModel = null;
                    GetWidget<RadioBox>("EntryList").Enabled = true;
                }
            }
        }

        public void SelectEntry(object parameter)
        {
            VoucherRecord record;
            if (parameter is IModelProperty)
            {
                record = (VoucherRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (VoucherRecord)parameter;

            IsAffordable.Value = record.Affordable;

            if (record.Affordable)
            {
                Audio.PlaySound(GameSound.Selection);
                GetWidget<RadioBox>("EntryList").Enabled = false;
                childViewModel = shopScene.AddView(new QuantityViewModel(shopScene, record));
            }
            else Audio.PlaySound(GameSound.Error);
        }

        public void UpdateDescription(int slot)
        {
            VoucherRecord record = AvailableEntries[slot];
            ItemRecord itemRecord = ItemRecord.ITEMS.First(x => x.Name == record.Name);
            if (record.Description != null)
            {
                if (Description.Value != record.Description) Description.Value = record.Description;
            }
            else
            {
                Description.Value = itemRecord.Description;
            }

            if (itemRecord.UsableBy != null)
            {
                UsableBy.ModelList.Clear();
                List<ModelProperty<ClassType>> usables = new List<ModelProperty<ClassType>>();
                foreach (ClassType classType in itemRecord.UsableBy) usables.Add(new ModelProperty<ClassType>(classType));
                UsableBy.ModelList = usables;
            }
            else UsableBy.RemoveAll();

            switch (itemRecord.ItemType)
            {
                case ItemType.Weapon:
                    StatLabel1.Value = "Attack";
                    StatLabel2.Value = "Hit %";
                    StatLabel3.Value = "Weight";
                    StatValue1.Value = itemRecord.Attack.ToString();
                    StatValue2.Value = itemRecord.Hit.ToString();
                    StatValue3.Value = itemRecord.Weight.ToString();
                    break;

                case ItemType.Shield:
                    StatLabel1.Value = "Evade";
                    StatLabel2.Value = "M. Evade";
                    StatLabel3.Value = "Weight";
                    StatValue1.Value = itemRecord.Evade.ToString();
                    StatValue2.Value = itemRecord.MagicEvade.ToString();
                    StatValue3.Value = itemRecord.Weight.ToString();
                    break;

                case ItemType.Armor:
                    StatLabel1.Value = "Defense";
                    StatLabel2.Value = "M. Defense";
                    StatLabel3.Value = "Weight";
                    StatValue1.Value = itemRecord.Defense.ToString();
                    StatValue2.Value = itemRecord.MagicDefense.ToString();
                    StatValue3.Value = itemRecord.Weight.ToString();
                    break;

                default:
                    StatLabel1.Value = "";
                    StatLabel2.Value = "";
                    StatLabel3.Value = "";
                    StatValue1.Value = "";
                    StatValue2.Value = "";
                    StatValue3.Value = "";
                    break;
            }
        }

        public ModelCollection<VoucherRecord> AvailableEntries { get; set; } = new ModelCollection<VoucherRecord>();

        public ModelProperty<bool> IsAffordable { get; set; } = new ModelProperty<bool>(true);

        public ModelProperty<string> Narration { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description { get; set; } = new ModelProperty<string>("");


        public ModelProperty<string> StatLabel1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> StatLabel2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> StatLabel3 { get; set; } = new ModelProperty<string>("");

        public ModelProperty<string> StatValue1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> StatValue2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> StatValue3 { get; set; } = new ModelProperty<string>("");

        public ModelCollection<ClassType> UsableBy { get; set; } = new ModelCollection<ClassType>();
    }
}
