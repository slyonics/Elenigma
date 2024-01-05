using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Elenigma.Scenes.StatusScene;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.ShopScene
{
    public class QuantityViewModel : ViewModel
    {
        VoucherRecord voucherRecord;
        public ItemRecord itemRecord;

        int confirmCooldown = 100;

        public QuantityViewModel(Scene iScene, VoucherRecord iVoucherRecord)
            : base(iScene, PriorityLevel.MenuLevel, GameView.ShopScene_QuantityView)
        {
            voucherRecord = iVoucherRecord;
            itemRecord = ItemRecord.ITEMS.First(x => x.Name.ToString() == voucherRecord.Name);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Terminate();
            }
            else if (Input.CurrentInput.CommandPressed(Command.Up)) CursorUp();
            else if (Input.CurrentInput.CommandPressed(Command.Down)) CursorDown();
            else if (Input.CurrentInput.CommandPressed(Command.Left)) CursorLeft();
            else if (Input.CurrentInput.CommandPressed(Command.Right)) CursorRight();
            else if (Input.CurrentInput.CommandPressed(Command.Confirm) && confirmCooldown <= 0)
            {
                if (CanAfford())
                {
                    Audio.PlaySound(GameSound.menu_select);
                    Proceed();
                }
                else Audio.PlaySound(GameSound.Error);
            }

            if (confirmCooldown > 0) confirmCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void Proceed()
        {
            Confirmed = true;
            Terminate();

            int quantity = Quantity.Value;
            GameProfile.PlayerProfile.Money.Value = GameProfile.PlayerProfile.Money.Value - quantity * voucherRecord.Price;

            ItemRecord item = new ItemRecord(ItemRecord.ITEMS.First(x => x.Name == voucherRecord.Name));

            if (item.ItemType == ItemType.Medicine || item.ItemType == ItemType.Consumable) GameProfile.AddInventory(voucherRecord.Name, quantity);
            else
            {
                for (int i = 0; i < quantity; i++) GameProfile.AddInventory(voucherRecord.Name, -1);
            }
        }

        private bool CanAfford()
        {
            if (Quantity.Value < 1) return false;

            return GameProfile.PlayerProfile.Money.Value >= Quantity.Value * voucherRecord.Price;
        }

        private bool CanAfford(int count)
        {
            if (count < 1) return false;

            return GameProfile.PlayerProfile.Money.Value >= count * voucherRecord.Price;
        }

        public void Cancel()
        {
            Terminate();
        }

        private void CursorUp()
        {
            int quantity = Quantity.Value;
            if (CanAfford(quantity + 1)) quantity++;

            if (quantity < 1) quantity = 1;
            else if (quantity > 99) quantity = 99;
            else
            {
                Audio.PlaySound(GameSound.Cursor);
            }

            Quantity.Value = quantity;
        }

        private void CursorDown()
        {
            int quantity = Quantity.Value;
            quantity--;

            if (quantity < 1) quantity = 1;
            else
            {
                Audio.PlaySound(GameSound.Cursor);
            }

            Quantity.Value = quantity;
        }

        private void CursorRight()
        {
            int quantity = Quantity.Value;
            if (CanAfford(quantity + 10)) quantity += 10;

            if (quantity < 1) quantity = 1;
            else if (quantity > 99) quantity = 99;
            else
            {
                Audio.PlaySound(GameSound.Cursor);
            }

            Quantity.Value = quantity;
        }

        private void CursorLeft()
        {
            int quantity = Quantity.Value;
            quantity -= 10;

            if (quantity < 1) quantity = 1;
            else
            {
                Audio.PlaySound(GameSound.Cursor);
            }

            Quantity.Value = quantity;
        }

        public ModelProperty<int> Quantity { get; set; } = new ModelProperty<int>(1);

        public bool Confirmed { get; set; } = false;
    }
}
