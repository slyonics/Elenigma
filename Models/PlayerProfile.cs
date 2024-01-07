using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    public enum SummonType
    {
        Slyph,
        Undine,
        Salamander,
        Gnome
    }

    [Serializable]
    public class PlayerProfile
    {
        public PlayerProfile()
        {

        }

        public List<SummonType> AvailableSummons { get; set; } = new List<SummonType>();
        public ModelProperty<long> Money { get; set; } = new ModelProperty<long>(13);
    }
}
