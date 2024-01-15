using Elenigma.Models;
using Elenigma.SceneObjects;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class SummonOverlay : Overlay
    {
        private const int RING_RADIUS = 24;
        private const int SCROLL_DURATION = 800;

        private class SummonEntry
        {
            public SummonType Summon { get; set; }
            public Texture2D Sprite { get; set; }
            public NinePatch Textbox { get; set; }
        }

        private MapScene mapScene;
        private Hero player;
        private List<SummonEntry> summons = new List<SummonEntry>();

        public SummonType SummonSelection { get => summons.First().Summon; }

        public NinePatch labelBox;

        float intervalLength;
        float scrollOffset;
        int scrollInterval;

        public SummonOverlay(MapScene iMapScene, Hero iPlayer, List<SummonType> availableSummons)
        {
            mapScene = iMapScene;
            player = iPlayer;

            intervalLength = (float)Math.PI * 2 / availableSummons.Count;

            int i = 0;
            foreach (SummonType summon in availableSummons)
            {
                var summonEntry = new SummonEntry()
                {
                    Summon = summon,
                    Sprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Widgets_Icons_" + summon)],
                    Textbox = new NinePatch("LightFrame", 0.05f)
                };
                summonEntry.Textbox.Bounds = new Rectangle(0, 0, 15, 14);

                summons.Add(summonEntry);
                i++;
            }

            labelBox = new NinePatch("LightFrame", 0.05f);
            labelBox.Bounds = new Rectangle(0, 0, Text.GetStringLength(GameFont.Dialogue, summons.First().Summon.ToString()) + 12, 13);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Scrolling)
            {
                if (scrollInterval > 0)
                {
                    scrollInterval += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (scrollInterval >= SCROLL_DURATION)
                    {
                        scrollInterval = 0;
                        summons.Add(summons.First());
                        summons.RemoveAt(0);

                        labelBox.Bounds = new Rectangle(0, 0, Text.GetStringLength(GameFont.Dialogue, summons.First().Summon.ToString()) + 12, 13);
                    }
                }
                else
                {
                    scrollInterval -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (scrollInterval <= -SCROLL_DURATION)
                    {
                        scrollInterval = 0;
                        summons.Insert(0, summons.Last());
                        summons.RemoveAt(summons.Count - 1);

                        labelBox.Bounds = new Rectangle(0, 0, Text.GetStringLength(GameFont.Dialogue, summons.First().Summon.ToString()) + 12, 13);
                    }
                }

                scrollOffset = (float)scrollInterval / SCROLL_DURATION * intervalLength;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            int i = 0;
            foreach (SummonEntry entry in summons)
            {
                float Angle = (i > 0) ? intervalLength * i : 0;
                Vector2 offset = player.Center - mapScene.Camera.Position + new Vector2((float)Math.Sin(Angle + scrollOffset) * RING_RADIUS, -(float)Math.Cos(Angle + scrollOffset) * RING_RADIUS) - new Vector2(7, 8);
                entry.Textbox.Draw(spriteBatch, offset);
                spriteBatch.Draw(entry.Sprite, offset + new Vector2(4, 3), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.045f);

                i++;
            }

            Vector2 selectionOffset = player.Center - mapScene.Camera.Position + new Vector2(-0, -RING_RADIUS);
            spriteBatch.Draw(AssetCache.SPRITES[GameSprite.Target], selectionOffset + new Vector2(-7, -9), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.04f);

            if (!Scrolling)
            {
                labelBox.Draw(spriteBatch, selectionOffset + new Vector2(-labelBox.Bounds.Width / 2, -21));
                Text.DrawCenteredText(spriteBatch, selectionOffset + new Vector2(0, -13), GameFont.Dialogue, summons.First().Summon.ToString(), new Color(173, 119, 87), 0.03f);
            }
        }

        public void ScrollRight()
        {
            scrollInterval = 1;
        }

        public void ScrollLeft()
        {
            scrollInterval = -1;
        }

        public bool Scrolling { get => scrollInterval != 0; }
    }
}
