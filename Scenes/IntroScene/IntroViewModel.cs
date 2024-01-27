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
        public const int FADE_LENGTH = 1000;

        private int fadeIndex = 0;
        private int fadeTime = 0;
        private int waitTime = 0;

        public IntroViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {

            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("I see you have returned hero.") });
            TextBlocks.Add(new IntroTextBlock() { Text = new ModelProperty<string>("Let's start from the beginning...") });

            LoadView(GameView.IntroScene_IntroView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.AnythingPressed())
            {
                CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
                return;
            }

            if (waitTime > 0)
            {
                waitTime -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (waitTime <= 0 && fadeIndex >= 2)
                {
                    IntroScene.NewGame();
                    return;
                }

                return;
            }

            if (fadeIndex >= 2) return;

            if (fadeTime < FADE_LENGTH)
            {
                fadeTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (fadeTime >= FADE_LENGTH)
                {
                    fadeTime = FADE_LENGTH;
                    TextBlocks.ModelList[fadeIndex].Value.Color.Value = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    fadeIndex++;
                    fadeTime = 0;
                    waitTime = 500;

                    if (fadeIndex >= 2)
                    {
                        waitTime = 1500;
                        return;
                    }
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
