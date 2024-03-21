using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Elenigma.Scenes.BattleScene
{
    public class BattleEnemy : Battler
    {
        private const int FADE_IN_DURATION = 0;
        private const int ATTACK_DURATION = 400;
        private const int DEATH_DURATION = 600;

        private static readonly Dictionary<string, Texture2D> ENEMY_SHADOWS = new Dictionary<string, Texture2D>();

        private static Effect ENEMY_BATTLER_EFFECT;
        public static Texture2D STATIC_TEXTURE;

        public EnemyRecord EnemyRecord { get; set; }

        public int LayoutX { get; set; }
        public int LayoutY { get; set; }

        public override bool TurnActive { get => turnActive || attackTimeLeft > 0; }

        private int fadeInTime;
        private int attackTimeLeft;
        private int deathTimeLeft;

        private RenderTarget2D spriteRender;

        private BattleController enqueuedController;

        public bool Scoped { get; set; }

        public BattleEnemy(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            Alignment = Alignment.Cascading;

            shader = ENEMY_BATTLER_EFFECT.Clone();
            shader.Parameters["destroyInterval"].SetValue(1.1f);
            shader.Parameters["noise"].SetValue(STATIC_TEXTURE);
            shader.Parameters["flashInterval"].SetValue(0.0f);
            shader.Parameters["flashColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            stats = new BattlerModel(EnemyRecord);

            AnimatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + EnemyRecord.Sprite)], null);
            shadow = ENEMY_SHADOWS["Enemies_" + EnemyRecord.Sprite];
            spriteRender = new RenderTarget2D(CrossPlatformGame.GameInstance.GraphicsDevice, AnimatedSprite.SpriteBounds().Width, AnimatedSprite.SpriteBounds().Height);

            Alignment = EnemyRecord.BattleAlignment;
            bounds.X = EnemyRecord.BattleOffsetX;
            bounds.Y = EnemyRecord.BattleOffsetY;
            bounds.Width = AnimatedSprite.SpriteBounds().Width;
            bounds.Height = AnimatedSprite.SpriteBounds().Height;
            bounds.Y -= EnemyRecord.ShadowOffset / 2;

            //if (Alignment == Alignment.Cascading) bounds.Inflate(4, 4);

            battleScene.AddBattler(this);

            battlerOffset = new Vector2(-160, 0);
        }

        public static void Initialize()
        {
            var enemyTextures = Enum.GetValues(typeof(GameSprite));
            foreach (GameSprite textureName in enemyTextures)
            {
                if (textureName == GameSprite.None) continue;

                Texture2D sprite = AssetCache.SPRITES[textureName];
                ENEMY_SHADOWS.Add(textureName.ToString(), BuildShadow(new Rectangle(-sprite.Width / 2, -sprite.Height / 4, sprite.Width, sprite.Height / 4)));
            }

            ENEMY_BATTLER_EFFECT = AssetCache.EFFECTS[GameShader.BattleEnemy];
            STATIC_TEXTURE = new Texture2D(CrossPlatformGame.GameInstance.GraphicsDevice, 200, 200);
            Color[] colorData = new Color[STATIC_TEXTURE.Width * STATIC_TEXTURE.Height];
            for (int y = 0; y < STATIC_TEXTURE.Height; y++)
            {
                for (int x = 0; x < STATIC_TEXTURE.Width; x++)
                {
                    colorData[y * STATIC_TEXTURE.Width + x] = new Color(Rng.RandomInt(0, 255), 255, 255, 255);
                }
            }
            STATIC_TEXTURE.SetData<Color>(colorData);
        }


        public override void DrawShadow(SpriteBatch spriteBatch)
        {
            Color shadowColor = Color.Lerp(SHADOW_COLOR, new Color(0, 0, 0, 0), Math.Min(1.0f, positionZ / (AnimatedSprite.SpriteBounds().Width + AnimatedSprite.SpriteBounds().Height) / 2));
            if (Dead) shadowColor.A = (byte)MathHelper.Lerp(0, shadowColor.A, (float)deathTimeLeft / DEATH_DURATION);
            spriteBatch.Draw(shadow, new Vector2((int)(Center.X - shadow.Width / 2), (int)(Bottom.Y - shadow.Height * 3 / 4) + 1 + EnemyRecord.ShadowOffset) + battlerOffset, null, shadowColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, SHADOW_DEPTH);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Transitioning) return;

            if (fadeInTime < FADE_IN_DURATION)
            {
                fadeInTime += gameTime.ElapsedGameTime.Milliseconds;
                float flashInterval = Math.Min((float)fadeInTime / FADE_IN_DURATION, 1.0f);
                shader.Parameters["flashInterval"].SetValue(1.0f - flashInterval);
            }

            if (attackTimeLeft > 0)
            {
                if (attackTimeLeft > 0 && (attackTimeLeft / (ATTACK_DURATION / 4)) % 2 == 0) FlashColor(Color.White, 200);
                attackTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            }

            if (deathTimeLeft > 0)
            {
                deathTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                shader.Parameters["destroyInterval"].SetValue((float)deathTimeLeft / DEATH_DURATION);
                if (deathTimeLeft <= 0) { terminated = true; deathTimeLeft = 0; }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawShadow(spriteBatch);

            if (Transitioning) return;

            
            spriteBatch.Draw(spriteRender, Bottom - new Vector2(AnimatedSprite.SpriteBounds().Width / 2, AnimatedSprite.SpriteBounds().Height) + battlerOffset, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, Depth);
        }

        public void DrawShader(SpriteBatch spriteBatch)
        {
            if (Transitioning || Terminated) return;

            CrossPlatformGame.GameInstance.GraphicsDevice.SetRenderTarget(spriteRender);
            CrossPlatformGame.GameInstance.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, null);
            AnimatedSprite.Draw(spriteBatch, new Vector2(AnimatedSprite.SpriteBounds().Width / 2, AnimatedSprite.SpriteBounds().Height), null, 0.5f);
            spriteBatch.End();
        }

        public override void StartTurn()
        {
            base.StartTurn();

            Dictionary<AttackData, double> attacks = EnemyRecord.Attacks.ToDictionary(x => x, x => (double)x.Weight);
            AttackData attack = Rng.WeightedEntry<AttackData>(attacks);

            List<BattlePlayer> eligibleTargets = battleScene.PlayerList.FindAll(x => !x.Dead);
            var target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
            BattleController battleController = new BattleController(battleScene, this, target, attack);
            battleScene.BattleEventQueue.Add(battleController);
            enqueuedController = battleController;
            battleController.OnTerminated += new TerminationFollowup(() => Initiative = Math.Max(1, Stats.Agility.Value - (Stats.Weight.Value / 8)));

            EndTurn();
        }

        public override void Damage(int damage)
        {
            base.Damage(damage);

            if (Dead) Death();
        }

        public override void InflictAilment(Battler attacker, AilmentType ailment)
        {
            base.InflictAilment(attacker, ailment);

            if (Dead) Death();
        }

        public override void Animate(string animationName)
        {
            switch (animationName)
            {
                case "Attack": attackTimeLeft = ATTACK_DURATION; break;
            }
        }

        public void Death()
        {
            deathTimeLeft = DEATH_DURATION;
            Audio.PlaySound(GameSound.Error);

            foreach (BattlePlayer player in battleScene.PlayerList)
            {
                if (player.ScaredOf.Count > 0)
                {
                    player.ScaredOf.Remove(this);
                    if (player.ScaredOf.Count == 0) player.HealAilment(AilmentType.Fear);
                }
            }
        }

        public Rectangle EnemySize { get => new Rectangle(0, 0, AnimatedSprite.SpriteBounds().Width, AnimatedSprite.SpriteBounds().Height); }

        public override bool Busy { get => base.Busy || deathTimeLeft > 0 || Transitioning || fadeInTime < FADE_IN_DURATION; }

        public override bool Transitioning { get => GetParent<Panel>().Transitioning; }

        public int ShadowOffset { get => EnemyRecord.ShadowOffset; }

        public bool Ready { get => initiative >= 255 && !Dead && (enqueuedController == null || enqueuedController.Terminated); }
    }
}
