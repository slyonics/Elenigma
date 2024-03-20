using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    public enum TargetType
    {
        OneEnemy,
        AnyEnemy,
        AllEnemy,
        OneAlly,
        AnyAlly,
        AllAlly,
        All,
        Any,
        Self,
    }

    [Serializable]
    public class CommandRecord
    {
        public CommandRecord()
        {

        }

        public CommandRecord(CommandRecord clone)
        {
            Name = clone.Name;
            Animation = clone.Animation;
            Description = clone.Description;
            Icon = clone.Icon;
            ChargesLeft = clone.ChargesLeft;
            Charges = clone.Charges;
            Targetting = clone.Targetting;
            TargetDead = clone.TargetDead;
            if (clone.Conditions != null) Conditions = (string)clone.Conditions.Clone();
            if (clone.FieldScript != null) FieldScript = (string[])clone.FieldScript.Clone();
            if (clone.BattleScript != null) BattleScript = (string[])clone.BattleScript.Clone();
            if (clone.ThrowScript != null) ThrowScript = (string[])clone.ThrowScript.Clone();
        }

        public string Name { get; set; }
        public string Animation { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int ChargesLeft { get; set; } = -1;
        public int Charges { get; set; } = -1;
        public bool ShowCharges { get => Charges >= 0; }
        public virtual bool Usable { get => ChargesLeft != 0; }
        public TargetType Targetting { get; set; }
        public bool TargetDead { get; set; } // true if this can target dead allies
        public string Conditions { get; set; }
        public string[] BattleScript { get; set; }
        public string[] FieldScript { get; set; }
        public string[] ThrowScript { get; set; }
    }
}
