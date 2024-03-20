using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    [Serializable]
    public class AbilityRecord : CommandRecord
    {
        public AbilityRecord()
        {

        }

        public AbilityRecord(AbilityRecord clone)
            : base(clone)
        {
            CommandCategory = clone.CommandCategory;
            Cost = clone.Cost;
            Hit = clone.Hit;
            Power = clone.Power;
        }

        public BattleCommand CommandCategory { get; set; }

        public int Cost { get; set; }
        public int Hit { get; set; }
        public int Power { get; set; }

        public bool FieldUsable { get => FieldScript != null; }

        public static List<AbilityRecord> ABILITIES { get; set; }
    }
}
