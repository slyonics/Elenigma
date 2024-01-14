using Elenigma.Main;
using Elenigma.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elenigma.Scenes.IntroScene
{
    public class IntroTextBlock
    {
        public ModelProperty<string> Text { get; set; }
        public ModelProperty<Color> Color { get; set; } = new ModelProperty<Color>(new Color(0.0f, 0.0f, 0.0f, 0.0f));
    }

    public class IntroViewModel : ViewModel
    {
        public const int FADE_LENGTH = 800;

        private int fadeIndex = 0;
        private int fadeTime = 0;
        private int waitTime = 0;

        public IntroViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {

            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("Today, There are 6 known regions inhabited by humans.") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("The 4 Great Elemental Regions - guided by the spirits.") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("The Capital - the most advanced region") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("springing from innovation and technology") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("The Tribal Land - a small village that to this day,") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("pray to and worship the first gods.") });

            LoadView(GameView.IntroScene_IntroView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (fadeIndex >= 9) return;

            if (waitTime > 0)
            {
                waitTime -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                return;
            }

            if (fadeTime < FADE_LENGTH)
            {
                fadeTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (fadeTime >= FADE_LENGTH)
                {
                    fadeTime = FADE_LENGTH;
                    TextBlocks.ModelList[fadeIndex].Value.Color.Value = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    fadeIndex++;
                    fadeTime = 0;
                    waitTime = 200;
                }
                else
                {
                    float interval = (float)fadeTime / FADE_LENGTH;
                    TextBlocks.ModelList[fadeIndex].Value.Color.Value = new Color(interval, interval, interval, interval);
                }
            }
        }

        public ModelCollection<IntroTextBlock> TextBlocks { get; set; } = new ModelCollection<IntroTextBlock>();
    }
}
