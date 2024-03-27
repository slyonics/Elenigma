using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    public enum HeroType
    {
        TheMage,
        TheCleric,
        TheWarrior,
        TheTank
    }

    public enum BattleCommand
    {
        Fight,
        Defend,
        Item,
        Throw,
        Magic,
        Scope,
        Song,
        Sword,
        Pact,
        Avatar
    }

    public enum ElementType
    {
        None,

        Blunt,
        Sharp,
        Ranged,


        Ice,
        Fire,
        Thunder,
        Earth,
        Poison,
        Life,
        Dark,
        Holy
    }

    public enum AilmentType
    {
        Healthy,

        Fear,
        Death,
        Confusion,
        Poison,
        Stone
    }

    public enum BuffType
    {
        AutoRevive
    }

    public class HeroRecord
    {
        public HeroType Name { get; set; }
        public Dictionary<string, string> Sprites { get; set; }
        public Dictionary<string, string> Portraits { get; set; }

        public int StrengthModifier { get; set; }
        public int AgilityModifier { get; set; }
        public int VitalityModifier { get; set; }
        public int MagicModifier { get; set; }

        public static List<HeroRecord> HEROES { get; set; }
    }
}
