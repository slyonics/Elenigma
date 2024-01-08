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
        private const int RING_RADIUS = 20;

        private class SummonEntry
        {
            public SummonType Summon { get; set; }
            public Texture2D Sprite { get; set; }
            public NinePatch Textbox { get; set; }
            public float Angle;
        }

        private MapScene mapScene;
        private Hero player;
        private List<SummonEntry> summons = new List<SummonEntry>();

        public SummonType SummonSelection { get => summons.First().Summon; }

        public SummonOverlay(MapScene iMapScene, Hero iPlayer, List<SummonType> availableSummons)
        {
            mapScene = iMapScene;
            player = iPlayer;

            int i = 0;
            foreach (SummonType summon in availableSummons)
            {
                var summonEntry = new SummonEntry()
                {
                    Summon = summon,
                    Sprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Widgets_Icons_" + summon)],
                    Textbox = new NinePatch("DarkFrame", 0.05f),
                    Angle = (i > 0) ? ((float)Math.PI * 2 / availableSummons.Count) * i : 0
                };
                summonEntry.Textbox.Bounds = new Rectangle(0, 0, 15, 14);

                summons.Add(summonEntry);
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

            foreach (SummonEntry entry in summons)
            {
                Vector2 offset = player.Center - mapScene.Camera.Position + new Vector2((float)Math.Sin(entry.Angle) * RING_RADIUS, -(float)Math.Cos(entry.Angle) * RING_RADIUS) - new Vector2(7, 8);
                entry.Textbox.Draw(spriteBatch, offset);
                spriteBatch.Draw(entry.Sprite, offset + new Vector2(4, 3), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.02f);
            }
        }
    }
}
