using Elenigma.Main;
using Elenigma.SceneObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elenigma.Scenes.BattleScene
{
    public class IntroShader : Shader
    {
        public IntroShader(Color color, float amount)
            : base(AssetCache.EFFECTS[GameShader.BattleIntro].Clone())
        {
            Effect.Parameters["filterRed"].SetValue(color.R / 255.0f);
            Effect.Parameters["filterGreen"].SetValue(color.G / 255.0f);
            Effect.Parameters["filterBlue"].SetValue(color.B / 255.0f);
            Effect.Parameters["noise"].SetValue(BattleEnemy.STATIC_TEXTURE);
            Amount = amount;
        }

        public float Amount
        {
            set
            {
                Effect.Parameters["amount"].SetValue(value);
            }
        }
    }
}
