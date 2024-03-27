using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    public enum ClassType
    {
        Cleric,
        Warrior,
        Mage,
        Tank,

        Monster,
        Undead,
        Beast,
        Boss
    }

    [Serializable]
    public class ClassProfile
    {
        public ClassType Class { get; set; }
        public List<string> Upgrades { get; set; } = new List<string>();
        public List<BattleCommand> Commands { get; set; } = new List<BattleCommand>();
        public List<string> Abilities { get; set; } = new List<string>();
        public List<string> Passives { get; set; } = new List<string>();
        public int BonusStrength { get; set; }
        public int BonusAgility { get; set; }
        public int BonusVitality { get; set; }
        public int BonusMagic { get; set; }

    }

    public class LearnableAbility
    {
        public string Ability;
        public int Level;
    }

    public class ClassRecord
    {
        public ClassType Name { get; set; }
        public int BaseStrength { get; set; }
        public int BaseAgility { get; set; }
        public int BaseVitality { get; set; }
        public int BaseMagic { get; set; }

        public static List<ClassRecord> CLASSES { get; set; }
    }
}
