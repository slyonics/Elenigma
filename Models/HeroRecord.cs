using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    public class HeroRecord
    {
        public string Name { get; set; }
        public string Sprite { get; set; }
        public string Portrait { get; set; }

        public static List<HeroRecord> HEROES { get; set; }
    }
}
