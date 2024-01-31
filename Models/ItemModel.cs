using Elenigma.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    [Serializable]
    public class ItemModel
    {
        public ItemModel(string itemName, int quantity)
        {
            ItemRecord = ItemRecord.ITEMS.First(x => x.Name == itemName);
            Quantity.Value = quantity;
        }

        public ItemRecord ItemRecord { get; set; }
        public ModelProperty<int> Quantity { get; set; } = new ModelProperty<int>(1);

        public bool Consumable { get => Quantity.Value > 0; }
    }
}

