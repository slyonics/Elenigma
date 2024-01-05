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
    public class SellViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) },
        };

        ShopScene shopScene;

        SellCountViewModel childViewModel;

        int Slot { get => GetWidget<RadioBox>("EntryList").Selection; }

        public SellViewModel(ShopScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            shopScene = iScene;

            AvailableEntries.ModelList = GameProfile.Inventory.ModelList;

            LoadView(GameView.ShopScene_SellView);

            Narration.Value = "What do you want to sell?";

            if (AvailableEntries.Count() > 0) UpdateDescription(0);
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
                        Narration.Value = "Nice doing business with you!";
                        AvailableEntries.ModelList = GameProfile.Inventory.ModelList;
                        if (AvailableEntries.Count() > 0)
                        {
                            var listbox = GetWidget<RadioBox>("EntryList");
                            (listbox.ChildList[listbox.Selection] as RadioButton).RadioSelect();
                        }
                    }
                    childViewModel = null;
                    GetWidget<RadioBox>("EntryList").Enabled = true;
                }
            }
        }

        public void SelectEntry(object parameter)
        {
            ItemModel record;
            if (parameter is IModelProperty)
            {
                record = (ItemModel)((IModelProperty)parameter).GetValue();
            }
            else record = (ItemModel)parameter;


            Audio.PlaySound(GameSound.Selection);
            GetWidget<RadioBox>("EntryList").Enabled = false;
            childViewModel = shopScene.AddView(new SellCountViewModel(shopScene, record));
        }

        public void UpdateDescription(int slot)
        {
            ItemModel record = AvailableEntries[slot];
            ItemRecord itemRecord = record.ItemRecord as ItemRecord;
            Description.Value = itemRecord.Description;

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
                    StatLabel1.Value = "ATT";
                    StatLabel2.Value = "HIT";
                    StatLabel3.Value = "WGHT";
                    StatValue1.Value = itemRecord.Attack.ToString();
                    StatValue2.Value = itemRecord.Hit.ToString();
                    StatValue3.Value = itemRecord.Weight.ToString();
                    break;

                case ItemType.Shield:
                    StatLabel1.Value = "BLK";
                    StatLabel2.Value = "MBLK";
                    StatLabel3.Value = "WGHT";
                    StatValue1.Value = itemRecord.Evade.ToString();
                    StatValue2.Value = itemRecord.MagicEvade.ToString();
                    StatValue3.Value = itemRecord.Weight.ToString();
                    break;

                case ItemType.Armor:
                    StatLabel1.Value = "DEF";
                    StatLabel2.Value = "MDEF";
                    StatLabel3.Value = "WGHT";
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

        public ModelCollection<ItemModel> AvailableEntries { get; set; } = new ModelCollection<ItemModel>();

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
