using Elenigma.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.BattleScene
{
    public class DamageParticle : Particle
    {
        private const int DIGIT_WIDTH = 8;
        private const int DIGIT_HEIGHT = 11;
        private const int DECAY_DURATION = 500;
        private const int NEXT_DIGIT_INTERVAL = 80;

        private Texture2D DIGIT_SPRITE = AssetCache.SPRITES[GameSprite.Particles_DamageDigits];
        private Rectangle[] DIGIT_SOURCES;

        private Vector2 initialPosition;
        private string digitsRemaining;

        private int digitIndex;
        private char nonDigit;
        private int decayTimer;
        private int nextDigitTimer;

        Color color = Color.White;

        public DamageParticle(Scene iScene, Vector2 iPosition, string digits)
            : base(iScene, iPosition, true)
        {
            DIGIT_SOURCES = new Rectangle[10];
            for (int i = 0; i < DIGIT_SOURCES.Length; i++) DIGIT_SOURCES[i] = new Rectangle(i * DIGIT_WIDTH, 0, DIGIT_WIDTH, DIGIT_HEIGHT);
            digitIndex = digits.First() - '0';
            if (digitIndex > DIGIT_SOURCES.Length) nonDigit = digits.First();

            initialPosition = iPosition;
            digitsRemaining = digits.Substring(1, digits.Length - 1);

            velocityZ = DIGIT_HEIGHT * 10;
            landingFollowup += StartDecay;
            nextDigitTimer = NEXT_DIGIT_INTERVAL;
        }

        public DamageParticle(Scene iScene, Vector2 iPosition, string digits, Color iColor)
            : base(iScene, iPosition, true)
        {
            DIGIT_SOURCES = new Rectangle[10];
            for (int i = 0; i < DIGIT_SOURCES.Length; i++) DIGIT_SOURCES[i] = new Rectangle(i * DIGIT_WIDTH, 0, DIGIT_WIDTH, DIGIT_HEIGHT);
            digitIndex = digits.First() - '0';
            if (digitIndex > DIGIT_SOURCES.Length) nonDigit = digits.First();

            initialPosition = iPosition;
            digitsRemaining = digits.Substring(1, digits.Length - 1);

            velocityZ = DIGIT_HEIGHT * 10;
            landingFollowup += StartDecay;
            nextDigitTimer = NEXT_DIGIT_INTERVAL;

            color = iColor;
        }

        public override void Update(GameTime gameTime)
        {
            UpdatePosition(gameTime);
            UpdateElevation(gameTime);

            if (nextDigitTimer > 0)
            {
                nextDigitTimer -= gameTime.ElapsedGameTime.Milliseconds;
                if (nextDigitTimer <= 0 && digitsRemaining.Length > 0)
                {
                    int digitWidth;
                    if (digitIndex > DIGIT_SOURCES.Length) digitWidth = Text.GetStringLength(GameFont.Main, "" + nonDigit) - 2;
                    else digitWidth = DIGIT_WIDTH;

                    DamageParticle nextParticle = new DamageParticle(parentScene, initialPosition + new Vector2(digitWidth, 0), digitsRemaining, color);
                    parentScene.AddParticle(nextParticle);
                }
            }
            else if (decayTimer > 0)
            {
                decayTimer -= gameTime.ElapsedGameTime.Milliseconds;
                if (decayTimer <= 0) Terminate();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (digitIndex > DIGIT_SOURCES.Length)
            {
                Text.DrawText(spriteBatch, position - new Vector2(0.0f, positionZ + DIGIT_HEIGHT), GameFont.Main, "" + nonDigit, color, 0.1f);
            }
            else spriteBatch.Draw(DIGIT_SPRITE, position - new Vector2(0.0f, positionZ + DIGIT_HEIGHT), DIGIT_SOURCES[digitIndex], color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
        }

        private void StartDecay()
        {
            decayTimer = DECAY_DURATION;
        }
    }
}
