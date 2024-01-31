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
    public class HeroModel
    {
        public HeroModel(string heroName, string heroPortrait, string heroSprite)
        {
            Name.Value = heroName;
            Portrait.Value = heroPortrait;
            Sprite.Value = heroSprite;
        }

        public ModelProperty<string> Name { get; set; } = new ModelProperty<string>();
        public ModelProperty<string> Portrait { get; set; } = new ModelProperty<string>();
        public ModelProperty<string> Sprite { get; set; } = new ModelProperty<string>();
    }
}

