using Elenigma.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class SummonOverlay : Overlay
    {
        private class SummonEntry
        {
            public SummonType Summon { get; set; }
            public NinePatch Textbox { get; set; }
            public float Angle;
        }

        private List<SummonEntry> Summons = new List<SummonEntry>();

        public SummonOverlay(List<SummonType> availableSummons)
        {
            int i = 0;
            foreach (SummonType summon in availableSummons)
            {
                Summons.Add(new SummonEntry()
                {
                    Summon = summon,
                    Textbox = new NinePatch("DarkFrame", 0.05f),
                    Angle = (i > 0) ? (360f / availableSummons.Count) * i : 0
                });
                i++;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (SummonEntry entry in Summons)
            {
                entry.Textbox.Bounds = new Rectangle();
            }
        }
    }
}
