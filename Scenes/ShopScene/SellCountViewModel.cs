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
    public class SellCountViewModel : ViewModel
    {
        ItemModel itemModel;

        VoucherRecord voucherRecord;
        public ItemRecord itemRecord;

        int confirmCooldown = 100;

        public SellCountViewModel(Scene iScene, ItemModel iItemModel)
            : base(iScene, PriorityLevel.MenuLevel, GameView.ShopScene_SellCountView)
        {
            //voucherRecord = iVoucherRecord;
            itemModel = iItemModel;
            itemRecord = itemModel.ItemRecord as ItemRecord;
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
                if (CanSell(Quantity.Value))
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
            GameProfile.PlayerProfile.Money.Value = GameProfile.PlayerProfile.Money.Value + quantity * itemRecord.SellPrice;

            GameProfile.RemoveInventory(itemModel, -quantity);

            /*if (Quantity.Value == Math.Max(1, itemModel.Quantity.Value))
            {
                var itemsLeftOver = GameProfile.Inventory.Remove
            }
            else itemModel.Quantity.Value = itemModel.Quantity.Value - quantity;*/
        }

        private bool CanSell(int newQuantity)
        {
            if (itemModel.Quantity.Value < 1 && newQuantity != 1) return false;

            if (newQuantity > Math.Max(1, itemModel.Quantity.Value)) return false;

            return true;
        }

        public void Cancel()
        {
            Terminate();
        }

        private void CursorUp()
        {
            int quantity = Quantity.Value;
            if (CanSell(quantity + 1)) quantity++;

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
            if (CanSell(quantity + 10)) quantity += 10;

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
