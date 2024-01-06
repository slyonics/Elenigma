using Elenigma.Scenes.ConversationScene;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    [Serializable]
    public class HeroModel
    {
        public HeroModel(HeroRecord heroRecord)
            : base()
        {
            Name.Value = heroRecord.Name;


            Sprite.Value = "Actors_" + heroRecord.Sprite;
            Portrait.Value = "Portraits_" + heroRecord.Sprite;
            ShadowSprite.Value = GameSprite.Actors_BattlerShadow.ToString();
        }

        public ModelProperty<string> Name { get; set; } = new ModelProperty<string>("AdultMC");
        public ModelProperty<string> Sprite { get; set; } = new ModelProperty<string>(GameSprite.Actors_AdultMC.ToString());
        public ModelProperty<string> Portrait { get; set; } = new ModelProperty<string>(GameSprite.Portraits_AdultMC.ToString());
        public ModelProperty<string> ShadowSprite { get; set; } = new ModelProperty<string>();

        public ModelProperty<long> Exp { get; set; } = new ModelProperty<long>(0);
        public ModelProperty<long> NextLevel { get; set; } = new ModelProperty<long>(0);
    }
}
