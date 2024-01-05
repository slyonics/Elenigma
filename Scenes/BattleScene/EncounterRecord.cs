using Elenigma.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.BattleScene
{
    public class EncounterEnemy
    {
        public string Name { get; set; }
        public int BattleOffsetX { get; set; }
        public int BattleOffsetY { get; set; }
    }

    public class EncounterRecord
    {
        public string Name { get; set; }
        public string Background { get; set; }
        public GameMusic Music { get; set; } = GameMusic.None;
        public EncounterEnemy[] Enemies { get; set; }
        public string[] Script { get; set; }
        public string Intro { get; set; }

        public static List<EncounterRecord> ENCOUNTERS { get; set; }
    }
}
