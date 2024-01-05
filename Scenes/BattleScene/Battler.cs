using Elenigma.Models;
using Elenigma.SceneObjects.Particles;
using Elenigma.SceneObjects.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.BattleScene
{
    public abstract class Battler : Widget
    {
        public const int STANDARD_TURN = 10;

        private const int DAMAGE_FLASH_DURATION = 600;

        protected const float SHADOW_DEPTH = Camera.MAXIMUM_ENTITY_DEPTH + 0.001f;
        protected const float START_SHADOW = 0.4f;
        protected const float END_SHADOW = 0.7f;
        protected static readonly Color SHADOW_COLOR = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        private static readonly Dictionary<string, Animation> AILMENT_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { AilmentType.Healthy.ToString(), new Animation(7, 9, 32, 32, 1, 9999) },
            { AilmentType.Fear.ToString(), new Animation(0, 1, 32, 32, 8, 300) },
            { AilmentType.Confusion.ToString(), new Animation(0, 4, 32, 32, 8, 300) }
        };

        protected BattleScene battleScene;

        protected Texture2D shadow = null;
        protected float positionZ = 0;
        protected Effect shader;
        protected int flashTime;
        protected int flashDuration;

        protected Vector2 battlerOffset;

        protected float initiative;
        protected bool turnActive;
        public virtual bool TurnActive { get => turnActive; }

        protected BattlerModel stats;
        public BattlerModel Stats { get => stats; }

        public bool Defending { get; set; }
        public bool Delaying { get; set; }
        public List<Battler> ScaredOf { get; private set; } = new List<Battler>();

        public AnimatedSprite AnimatedSprite { get; protected set; }
        public AnimatedSprite AilmentSprite { get; protected set; }
        private int confusionTurns = 0;

        public List<Particle> ParticleList { get; } = new List<Particle>();

        protected bool drawSprite = false;

        public Battler(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            initiative = 0;

            battleScene = GetParent<ViewModel>().ParentScene as BattleScene;

            AilmentSprite = new AnimatedSprite(AssetCache.SPRITES[GameSprite.Ailments], AILMENT_ANIMATIONS);
        }

        protected static Texture2D BuildShadow(Rectangle bounds)
        {
            int shadowWidth = (int)Math.Max(1, bounds.Width * 1.0f);
            int shadowHeight = (int)Math.Max(1, bounds.Height * 1.0f);
            float ovalFactorX = ((float)shadowHeight / (shadowWidth + shadowHeight));
            float ovalFactorY = ((float)shadowWidth / (shadowWidth + shadowHeight));
            float maxDistance = (float)Math.Sqrt(Math.Pow(shadowWidth / 2 * ovalFactorX, 2) + Math.Pow(shadowHeight / 2 * ovalFactorY, 2));

            Texture2D result = new Texture2D(CrossPlatformGame.GameInstance.GraphicsDevice, shadowWidth, shadowHeight);
            Color[] colorData = new Color[shadowWidth * shadowHeight];
            for (int y = 0; y < shadowHeight; y++)
            {
                for (int x = 0; x < shadowWidth; x++)
                {
                    float distance = (float)Math.Sqrt(Math.Pow(Math.Abs(x - shadowWidth / 2) * ovalFactorX, 2) + Math.Pow(Math.Abs(y - shadowHeight / 2) * ovalFactorY, 2));
                    float shadowInterval = distance / maxDistance;

                    if (shadowInterval < START_SHADOW) colorData[y * shadowWidth + x] = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                    else if (shadowInterval > END_SHADOW) colorData[y * shadowWidth + x] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                    else
                    {
                        float interval = 1.0f - (shadowInterval - START_SHADOW) / (END_SHADOW - START_SHADOW);
                        if (interval < 0.25f) interval = 0.0f;
                        else interval = 1.0f;
                        colorData[y * shadowWidth + x] = new Color(0.0f, 0.0f, 0.0f, interval);
                    }
                }
            }
            result.SetData<Color>(colorData);

            return result;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            drawSprite = false;

            if (flashTime > 0)
            {
                flashTime -= gameTime.ElapsedGameTime.Milliseconds;

                if (flashTime > 0) shader.Parameters["flashInterval"].SetValue((float)flashTime / DAMAGE_FLASH_DURATION);
                else shader.Parameters["flashInterval"].SetValue(0.0f);
            }

            AnimatedSprite.Update(gameTime);
            AilmentSprite?.Update(gameTime);

            ParticleList.RemoveAll(x => x.Terminated);

            if (battlerOffset.X < 0)
            {
                battlerOffset.X += gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 500.0f;
                if (battlerOffset.X >= 0) battlerOffset.X = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            drawSprite = true;
        }

        public virtual void DrawShadow(SpriteBatch spriteBatch)
        {
            if (shadow == null) return;

            Color shadowColor = Color.Lerp(SHADOW_COLOR, new Color(0, 0, 0, 0), Math.Min(1.0f, positionZ / (currentWindow.Width + currentWindow.Height) / 2));
            spriteBatch.Draw(shadow, new Vector2((int)(Top.X - shadow.Width / 2), (int)(Top.Y) + 1), null, shadowColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, SHADOW_DEPTH);
        }

        public virtual void IncrementInitiative(GameTime gameTime)
        {


            if (initiative < 255) Initiative += 3.5f * gameTime.ElapsedGameTime.Milliseconds / 100.0f;
            if (initiative >= 255) Initiative = 255;
        }

        public virtual void StartTurn()
        {
            turnActive = true;
            Defending = false;
            Delaying = false;

            if (Stats.StatusAilments.Any(x => x.Value == AilmentType.Confusion))
            {
                confusionTurns--;
                if (confusionTurns <= 0) HealAilment(AilmentType.Confusion);
            }
        }

        public virtual void EndTurn()
        {
            turnActive = false;
        }

        public virtual void ResetInitiative()
        {
            Initiative = Math.Max(1, 120 + Stats.Agility.Value - (Stats.Weight.Value / 8));
        }

        public virtual void Animate(string animationName)
        {

        }

        public virtual void PlayAnimation(string animationName, AnimationFollowup animationFollowup = null)
        {
            if (animationFollowup == null) AnimatedSprite.PlayAnimation(animationName);
            else AnimatedSprite.PlayAnimation(animationName, animationFollowup);
        }

        public virtual void Damage(int damage)
        {
            if (Defending)
            {
                damage /= 2;
            }

            Stats.HP.Value = Math.Max(0, Stats.HP.Value - damage);

            if (Dead)
            {
                var autoReviveBuff = Buffs.FirstOrDefault(x => x.Value == BuffType.AutoRevive);
                if (autoReviveBuff != null)
                {
                    Stats.HP.Value = 1;
                    Buffs.Remove(autoReviveBuff);
                }
            }

            ParticleList.Add(battleScene.AddParticle(new DamageParticle(battleScene, Bottom, damage.ToString())));
        }

        public virtual void InflictAilment(Battler attacker, AilmentType ailment)
        {
            if (!stats.StatusAilments.Any(x => x.Value == ailment)) stats.StatusAilments.Add(ailment);

            switch (ailment)
            {
                case AilmentType.Death:
                    Stats.HP.Value = 0;
                    if (Dead)
                    {
                        var autoReviveBuff = Buffs.FirstOrDefault(x => x.Value == BuffType.AutoRevive);
                        if (autoReviveBuff != null)
                        {
                            Stats.HP.Value = 1;
                            Buffs.Remove(autoReviveBuff);
                        }
                    }
                    break;

                case AilmentType.Fear:
                    if (!ScaredOf.Contains(attacker)) ScaredOf.Add(attacker);
                    AilmentSprite.PlayAnimation(AilmentType.Fear.ToString());
                    break;

                case AilmentType.Confusion:
                    AilmentSprite.PlayAnimation(AilmentType.Confusion.ToString());
                    confusionTurns = Rng.RandomInt(2, 4);
                    break;
            }
        }

        public virtual void Miss()
        {
            ParticleList.Add(battleScene.AddParticle(new DamageParticle(battleScene, Bottom, "MISS", Color.WhiteSmoke)));
        }

        public virtual void Heal(int healing)
        {
            if (Dead)
            {
                Initiative = 0;
            }

            Stats.HP.Value = Math.Min(Stats.MaxHP.Value, Stats.HP.Value + healing);

            ParticleList.Add(battleScene.AddParticle(new DamageParticle(battleScene, Bottom, healing.ToString(), new Color(28, 210, 160))));
        }

        public virtual void Replenish(int replenishment)
        {
            Stats.MP.Value = Math.Min(Stats.MaxMP.Value, Stats.MP.Value + replenishment);

            ParticleList.Add(battleScene.AddParticle(new DamageParticle(battleScene, Bottom, replenishment.ToString(), new Color(28, 160, 210))));
        }

        public virtual void HealAilment(AilmentType ailment)
        {
            Stats.StatusAilments.ModelList.RemoveAll(x => x.Value == ailment);

            if (Stats.StatusAilments.Count() == 0) AilmentSprite.PlayAnimation(AilmentType.Healthy.ToString());
        }

        public void FlashColor(Color flashColor, int duration = DAMAGE_FLASH_DURATION)
        {
            shader.Parameters["flashColor"].SetValue(flashColor.ToVector4());

            flashTime = flashDuration = duration;
        }

        public bool Dead { get => Stats.HP.Value <= 0; }

        public virtual float Initiative
        {
            get => initiative;
            set
            {
                initiative = value;
            }
        }

        public virtual bool Busy { get => turnActive || ParticleList.Count > 0 || (Math.Abs(battlerOffset.X) > 0.1f); }

        public override bool Transitioning { get => GetParent<Panel>().Transitioning; }

        public virtual Vector2 Bottom { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y + bounds.Height / 2) + Position; }
        public virtual Vector2 Top { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y - bounds.Height / 2) + Position; }
        public virtual Vector2 TopLeft { get => new Vector2(currentWindow.Left, currentWindow.Center.Y + bounds.Y - bounds.Height / 2) + Position; }
        public virtual Vector2 Center { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y) + Position; }


        public virtual Rectangle SpriteBounds
        {
            get
            {
                return new Rectangle(currentWindow.Left + (int)Position.X, currentWindow.Top + (int)Position.Y, currentWindow.Width, currentWindow.Height);
            }
        }

        public ModelCollection<BuffType> Buffs { get; private set; } = new ModelCollection<BuffType>();
    }
}
