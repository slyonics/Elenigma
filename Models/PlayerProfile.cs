using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    [Serializable]
    public class PlayerProfile
    {
        public PlayerProfile()
        {

        }

        [field: NonSerialized]
        public HeroModel StoredHero { get; set; }

        public ModelCollection<HeroModel> Party { get; set; } = new ModelCollection<HeroModel>();
        public ModelProperty<long> Money { get; set; } = new ModelProperty<long>(13);
    }
}
