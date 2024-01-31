using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    [Serializable]
    public class ItemRecord
    {
        public ItemRecord()
        {

        }

        public string Name { get; set; }
        public string Label { get; set; }
        public string Sprite { get; set; }
        public string Description { get; set; }

        public static List<ItemRecord> ITEMS { get; set; }
    }
}
