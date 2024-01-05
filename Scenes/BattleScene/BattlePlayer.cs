using Elenigma.Models;
using Elenigma.SceneObjects.Particles;
using Elenigma.SceneObjects.Widgets;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Elenigma.Scenes.BattleScene
{
    public class BattlePlayer : Battler
    {
        public const int HERO_WIDTH = 21;
        public const int HERO_HEIGHT = 21;

        protected enum HeroAnimation
        {
            Ready,
            Walking,
            Victory,
            Guarding,
            Stab,
            Attack,
            Shoot,
            Chanting,
            Spell,
            Item,
            Point,
            Hit,
            Hurting,
            Dead
        }

        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.Ready.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 1, 400) },
            { HeroAnimation.Walking.ToString(), new Animation(1, 0, HERO_WIDTH, HERO_HEIGHT, 2, 50) },
            { HeroAnimation.Victory.ToString(), new Animation(6, 1, HERO_WIDTH, HERO_HEIGHT, 2, 700) },
            { HeroAnimation.Guarding.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Stab.ToString(), new Animation(3, 0, HERO_WIDTH, HERO_HEIGHT, 3, new int[] { 100, 100, 300 }) },
            { HeroAnimation.Attack.ToString(), new Animation(3, 1, HERO_WIDTH, HERO_HEIGHT, 3, new int[] { 100, 100, 300 }) },
            { HeroAnimation.Shoot.ToString(), new Animation(3, 2, HERO_WIDTH, HERO_HEIGHT, 3, new int[] { 500, 50, 200 }) },
            { HeroAnimation.Chanting.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 3, 1000) },
            { HeroAnimation.Spell.ToString(), new Animation(3, 4, HERO_WIDTH, HERO_HEIGHT, 3, new int[] { 100, 100, 300 }) },
            { HeroAnimation.Item.ToString(), new Animation(3, 5, HERO_WIDTH, HERO_HEIGHT, 3, new int[] { 100, 100, 300 }) },
            { HeroAnimation.Point.ToString(), new Animation(3, 2, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Hit.ToString(), new Animation(0, 4, HERO_WIDTH, HERO_HEIGHT, 1, 600) },
            { HeroAnimation.Hurting.ToString(), new Animation(6, 2, HERO_WIDTH, HERO_HEIGHT, 3, 100) },
            { HeroAnimation.Dead.ToString(), new Animation(6, 5, HERO_WIDTH, HERO_HEIGHT, 1, 1000) }
        };

        public static readonly Dictionary<string, Animation> SHADOW_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.Ready.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 3, 400) }
        };

        private BattleController enqueuedController;
        private CommandRecord enqueuedCommand;
        private int prepTimeLeft = 0;



        private AnimatedSprite shadowSprite;

        private HeroModel heroModel;
        public HeroModel HeroModel
        {
            get => heroModel; set
            {
                heroModel = value;
                //if (HeroModel.FlightHeight.Value > 1)
                //{
                shadowSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), HeroModel.ShadowSprite.Value)], SHADOW_ANIMATIONS);
                //}
            }
        }

        public BattlePlayer(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            shader = AssetCache.EFFECTS[GameShader.BattlePlayer].Clone();
            shader.Parameters["flashInterval"].SetValue(0.0f);

            battlerOffset = new Vector2(160, 0);
        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);
            stats = HeroModel;

            heroModel.StatusAilments.RemoveAll();
            // reapply poision and stone here

            if (heroModel.Weapon.Value != null && heroModel.Weapon.Value.AutoBuffs != null) foreach (BuffType buff in heroModel.Weapon.Value.AutoBuffs) Buffs.Add(buff);
            if (heroModel.Shield.Value != null && heroModel.Shield.Value.AutoBuffs != null) foreach (BuffType buff in heroModel.Shield.Value.AutoBuffs) Buffs.Add(buff);
            if (heroModel.Armor.Value != null && heroModel.Armor.Value.AutoBuffs != null) foreach (BuffType buff in heroModel.Armor.Value.AutoBuffs) Buffs.Add(buff);
            if (heroModel.Accessory.Value != null && heroModel.Accessory.Value.AutoBuffs != null) foreach (BuffType buff in heroModel.Accessory.Value.AutoBuffs) Buffs.Add(buff);

            AnimatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), HeroModel.BattleSprite.Value)], HERO_ANIMATIONS);
            AnimatedSprite.PlayAnimation("Walking");

            bounds = AnimatedSprite.SpriteBounds();
            bounds.X += GetParent<DataGrid>().ChildList.IndexOf(parent) * 2;
            battleScene.AddBattler(this);

            HeroModel.UpdateHealthColor();

            int startingInitiative = Math.Max(1, 120 + HeroModel.Agility.Value - (HeroModel.Weight.Value / 8));
            Initiative = Rng.RandomInt(startingInitiative, 200);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (battlerOffset.X > 0)
            {
                battlerOffset.X -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 250.0f;
                if (battlerOffset.X <= 0)
                {
                    battlerOffset.X = 0;
                    Idle();
                }
            }

            if (prepTimeLeft > 0 && !battleScene.InitiativeSuspended)
            {
                prepTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                if (prepTimeLeft <= 0)
                {
                    battleScene.BattleEventQueue.Add(enqueuedController);
                    enqueuedController.OnTerminated += new TerminationFollowup(() => ResetInitiative());
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            shadowSprite?.Draw(spriteBatch, new Vector2(Bottom.X, Bottom.Y + 2) + battlerOffset, null, Depth);
        }

        public void DrawShader(SpriteBatch spriteBatch)
        {
            if (!drawSprite) return;

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, null);
            AnimatedSprite.Draw(spriteBatch, Bottom + battlerOffset - new Vector2(0, Dead ? 0 : HeroModel.FlightHeight.Value), null, Depth);
            spriteBatch.End();
        }

        public void DrawAilment(SpriteBatch spriteBatch)
        {
            if (!drawSprite) return;

            AilmentSprite.Draw(spriteBatch, Center, null, 0.1f);
        }

        public override void Animate(string animationName)
        {
            switch (animationName)
            {
                case "Stab":
                case "Attack":
                case "Shoot":
                case "Spell":
                case "Item": PlayAnimation(animationName, Idle); break;

                case "Victory": PlayAnimation("Victory"); AnimatedSprite.NextFrame(); break;
                default: PlayAnimation(animationName); break;
            }
        }

        public override void StartTurn()
        {
            base.StartTurn();

            Idle();

            HeroModel.NameColor.Value = new Color(206, 109, 10);

            battleScene.BattleViewModel.StartPlayerTurn(this);
        }

        public override void EndTurn()
        {
            base.EndTurn();

            HeroModel.NameColor.Value = new Color(252, 252, 252, 255);

            battleScene.BattleViewModel.EndPlayerTurn(this);
        }

        public void EnqueueCommand(BattleController battleController, CommandRecord commandRecord, BattleCommand battleCommand)
        {
            enqueuedController = battleController;
            enqueuedCommand = commandRecord;

            switch (battleCommand)
            {
                case BattleCommand.Fight:
                case BattleCommand.Throw:
                case BattleCommand.Scope:
                case BattleCommand.Item:
                    prepTimeLeft = (100 - Stats.Agility.Value + Stats.Weight.Value / 8) * 10;
                    break;

                case BattleCommand.Defend:
                    prepTimeLeft = 0;
                    break;

                default:
                    var abilityRecord = commandRecord as AbilityRecord;
                    prepTimeLeft = (100 - Stats.Magic.Value + Stats.Weight.Value / 8) * 10 * Math.Max(1, abilityRecord.Cost - Stats.Level.Value / 2);
                    break;
            }
        }

        public void ResetCommand()
        {
            enqueuedCommand = null;
            enqueuedController = null;
        }

        public List<ConversationScene.DialogueRecord> GrowAfterBattle(EncounterRecord encounterRecord)
        {
            List<ConversationScene.DialogueRecord> reports = new List<ConversationScene.DialogueRecord>();

            return reports;
        }

        public override void Damage(int damage)
        {
            base.Damage(damage);

            if (Dead)
            {
                prepTimeLeft = 0;
                enqueuedController = null;
                enqueuedCommand = null;
                Initiative = 0;
                stats.StatusAilments.RemoveAll();
                AilmentSprite.PlayAnimation(AilmentType.Healthy.ToString());
                HeroModel.InitiativeColor.Value = new Color(255, 255, 255);
                HeroModel.NameColor.Value = new Color(252, 252, 252, 255);
            }

            PlayAnimation("Hit", Idle);

            HeroModel.UpdateHealthColor();
        }

        public override void InflictAilment(Battler attacker, AilmentType ailment)
        {
            base.InflictAilment(attacker, ailment);

            if (Dead)
            {
                prepTimeLeft = 0;
                enqueuedController = null;
                enqueuedCommand = null;
                Initiative = 0;
                stats.StatusAilments.RemoveAll();
                AilmentSprite.PlayAnimation(AilmentType.Healthy.ToString());
                HeroModel.InitiativeColor.Value = new Color(255, 255, 255);
                HeroModel.NameColor.Value = new Color(252, 252, 252, 255);
                HeroModel.UpdateHealthColor();
                PlayAnimation("Dead");
            }
            else Idle();
        }

        public override void HealAilment(AilmentType ailment)
        {
            base.HealAilment(ailment);

            Idle();
        }

        public override void Heal(int healing)
        {
            base.Heal(healing);

            HeroModel.UpdateHealthColor();

            Task.Delay(500).ContinueWith(t =>
            {
                while (ParticleList.Any()) Thread.Sleep(100);
                Idle();
            });
        }

        public void Idle()
        {
            if (Defending && !Dead) PlayAnimation("Guarding");
            else if (enqueuedCommand != null && !enqueuedController.Terminated) PlayAnimation(enqueuedCommand.Animation);
            else if (Stats.HP.Value > HeroModel.MaxHP.Value / 8 && !Stats.StatusAilments.Any()) PlayAnimation("Ready");
            else if (Stats.HP.Value > 0) PlayAnimation("Hurting");
            else PlayAnimation("Dead");
        }

        public override Vector2 Top { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y - bounds.Height / 4) + Position; }
        public override Vector2 Center { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y + bounds.Height / 4) + Position; }

        public override Rectangle SpriteBounds
        {
            get
            {
                return new Rectangle(currentWindow.Left - 14 + (int)Position.X, currentWindow.Top - 4 + (int)Position.Y, 130, 23);
            }
        }

        public override float Initiative
        {
            set
            {
                initiative = value;
                heroModel.Initiative.Value = value;
                if (initiative >= 255) heroModel.InitiativeColor.Value = new Color(0, 240, 240);
                else HeroModel.InitiativeColor.Value = new Color(255, 255, 255);
            }
        }

        public bool Ready { get => initiative >= 255 && !Dead && enqueuedController == null; }
    }
}
