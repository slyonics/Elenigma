using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Particles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.BattleScene
{
    public class BattleController : ScriptController
    {
        private BattleScene battleScene;
        private Battler attacker;
        private Battler target;
        CommandRecord commandRecord;
        AttackData attackData;

        public bool targetAllEnemies;
        public bool targetAllAllies;
        public bool multiTarget;

        public Battler Attacker { get { return attacker; } }

        ConversationScene.ConversationScene convoScene;
        double timeleft = 0;

        public BattleController(BattleScene iBattleScene, Battler iAttacker, Battler iTarget, AttackData iAttackData)
           : base(iBattleScene, iAttackData.Script, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
            attackData = iAttackData;
            targetAllEnemies = attackData.Targetting == TargetType.AllEnemy;
            targetAllAllies = attackData.Targetting == TargetType.AllAlly;

            if (attacker.Dead)
            {
                Terminate();
                return;
            }

            FixTargetting();
        }

        public BattleController(BattleScene iBattleScene, Battler iAttacker, Battler iTarget, AttackData iAttackData, string[] script)
           : base(iBattleScene, script, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
            attackData = iAttackData;

            if (attacker.Dead)
            {
                Terminate();
                return;
            }

            FixTargetting();
        }

        public BattleController(BattleScene iBattleScene, Battler iAttacker, Battler iTarget, CommandRecord iCommandRecord, bool allEnemies = false, bool allAllies = false)
           : base(iBattleScene, ItemModel.ThrowMode ? iCommandRecord.ThrowScript : iCommandRecord.BattleScript, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
            commandRecord = iCommandRecord;
            targetAllEnemies = allEnemies;
            targetAllAllies = allAllies;

            if (attacker.Dead)
            {
                Terminate();
                return;
            }

            FixTargetting();
        }

        public BattleController(BattleScene iBattleScene, Battler iAttacker, Battler iTarget, CommandRecord iCommandRecord, string[] script)
           : base(iBattleScene, script, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
            commandRecord = iCommandRecord;
            multiTarget = true;

            if (attacker.Dead)
            {
                Terminate();
                return;
            }

            FixTargetting();
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (scriptParser.Finished)
            {
                if (convoScene != null)
                {
                    if (convoScene.ConversationViewModel.ReadyToProceed.Value)
                    {
                        timeleft -= gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (timeleft < 0)
                        {
                            convoScene.ConversationViewModel.Terminate();
                            convoScene = null;
                        }
                    }
                }
                else Terminate();
            }
            else scriptParser.Update(gameTime);
        }

        public void FixTargetting()
        {
            if (!targetAllEnemies && !targetAllAllies && target.Dead)
            {
                if (target is BattlePlayer)
                {
                    List<BattlePlayer> eligibleTargets = battleScene.PlayerList.FindAll(x => !x.Dead);
                    target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
                }
                else
                {
                    List<BattleEnemy> eligibleTargets = battleScene.EnemyList.FindAll(x => !x.Dead && !attacker.ScaredOf.Contains(x));
                    if (eligibleTargets.Count > 0) target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
                    else
                    {
                        eligibleTargets = battleScene.EnemyList.FindAll(x => !x.Dead);
                        target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
                    }
                }
            }
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Announce": battleScene.AddOverlay(new AnnounceOverlay(String.Join(' ', tokens.Skip(1)))); break;
                case "Animate": Animate(tokens); break;
                case "Idle": ((BattlePlayer)attacker).Idle(); break;

                case "Effect": Effect(tokens); break;
                case "CenterEffect": CenterEffect(tokens); break;
                case "Damage": CalculateDamage(tokens); break;
                case "Ailment": target.InflictAilment(attacker, (AilmentType)Enum.Parse(typeof(AilmentType), tokens[1])); break;
                case "Heal": if (target.Stats.Class.Value == ClassType.Undead) CalculateDamage(tokens); else CalculateHealing(tokens); break;
                case "Replenish": CalculateReplenish(tokens); break;
                case "Flash": Flash(tokens); break;

                case "Attack": Attack(tokens); break;
                case "Dialogue": Dialogue(tokens); break;
                case "Analyze": Analyze(tokens); return true;
                case "Flee": Flee(tokens); break;
                case "OnHit": if (!CalculateHit(tokens)) scriptParser.EndScript(); break;
                case "Multitarget": if (!Multitarget()) scriptParser.EndScript(); break;
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            switch (parameter)
            {
                case "$targetCenterX": return target.Center.X.ToString();
                case "$targetCenterY": return target.Center.Y.ToString();
                case "$targetTop": return target.Top.Y.ToString();
                case "$targetBottom": return target.Bottom.Y.ToString();
                case "$targetName": return target.Stats.Name.Value;
                case "$attackerName": return attacker.Stats.Name.Value;
                case "$attackerNameEx": return attacker.Stats.Name.Value + "!";
                default: return base.ParseParameter(parameter);
            }
        }

        private bool Multitarget()
        {
            if (attackData != null)
            {
                if (targetAllEnemies)
                {
                    var enemyTargets = battleScene.PlayerList.Where(x => !x.Dead);
                    foreach (BattlePlayer enemy in enemyTargets) battleScene.AddController(new BattleController(battleScene, attacker, enemy, attackData, scriptParser.RemainingCommands));

                    Terminate();

                    return false;
                }
                else if (targetAllAllies)
                {
                    var playerTargets = battleScene.EnemyList.Where(x => !x.Dead);
                    foreach (BattleEnemy player in playerTargets) battleScene.AddController(new BattleController(battleScene, attacker, player, attackData, scriptParser.RemainingCommands));

                    Terminate();

                    return false;
                }
            }
            else
            {
                if (targetAllEnemies)
                {
                    var enemyTargets = battleScene.EnemyList.Where(x => !x.Dead);
                    foreach (BattleEnemy enemy in enemyTargets) battleScene.AddController(new BattleController(battleScene, attacker, enemy, commandRecord, scriptParser.RemainingCommands));

                    Terminate();

                    return false;
                }
                else if (targetAllAllies)
                {
                    var playerTargets = battleScene.PlayerList.Where(x => !x.Dead);
                    foreach (BattlePlayer player in playerTargets) battleScene.AddController(new BattleController(battleScene, attacker, player, commandRecord, scriptParser.RemainingCommands));

                    Terminate();

                    return false;
                }
            }

            return true;
        }

        private void Animate(string[] tokens)
        {
            string animationName = tokens[1];
            if (tokens.Count() == 2)
            {
                attacker.Animate(tokens[1]);
            }
            else if (tokens.Count() == 3)
            {
                attacker.Animate(tokens[1]);
            }
        }

        private void CenterEffect(string[] tokens)
        {
            Vector2 position = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            AnimationType animationType = (AnimationType)Enum.Parse(typeof(AnimationType), tokens[1]);
            AnimationParticle animationParticle = new AnimationParticle(battleScene, position, animationType, true);

            if (tokens.Length > 4) animationParticle.AddFrameEvent(int.Parse(tokens[4]), new FrameFollowup(scriptParser.BlockScript()));

            battleScene.AddParticle(animationParticle);
            attacker.ParticleList.Add(animationParticle);
        }

        private void Effect(string[] tokens)
        {
            Vector2 position = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            AnimationType animationType = (AnimationType)Enum.Parse(typeof(AnimationType), tokens[1]);
            AnimationParticle animationParticle = new AnimationParticle(battleScene, position, animationType, true);
            animationParticle.Position = new Vector2(animationParticle.Position.X, animationParticle.Position.Y - animationParticle.AnimatedSprite.SpriteBounds().Height / 4);

            if (tokens.Length > 4) animationParticle.AddFrameEvent(int.Parse(tokens[4]), new FrameFollowup(scriptParser.BlockScript()));

            battleScene.AddParticle(animationParticle);
            attacker.ParticleList.Add(animationParticle);
        }

        private bool CalculateHit(string[] tokens)
        {
            int hit = 100;
            int evade = 0;

            switch (tokens[1])
            {
                case "Fists":
                    hit = 90;
                    evade = target.Stats.Evade.Value;
                    break;

                case "Swords":
                    hit = 100;
                    evade = target.Stats.Evade.Value;
                    break;

                case "Staves":
                    hit = 100;
                    evade = target.Stats.Evade.Value;
                    break;

                case "Bows":
                    if (attacker.Stats.Class.Value == ClassType.Hunter && target is BattleEnemy && ((BattleEnemy)target).Scoped) hit = 100;
                    hit = ((ItemRecord)commandRecord).Hit;
                    evade = target.Stats.Evade.Value / 2;
                    break;

                case "Axes":
                    hit = ((ItemRecord)commandRecord).Hit;
                    evade = target.Stats.Evade.Value;
                    break;

                case "Magic":
                    hit = ((AbilityRecord)commandRecord).Hit + attacker.Stats.Level.Value - target.Stats.Level.Value;
                    evade = target.Stats.MagicEvade.Value;
                    break;

                case "Throw":
                    hit = 100;
                    //if (((ItemRecord)commandRecord).ItemType == ItemType.Consumable) evade = target.Stats.Evade.Value / 3;
                    break;

                case "Ailment":
                    hit = attackData != null ? attackData.Hit : ((AbilityRecord)commandRecord).Hit;
                    evade = target.Stats.AilmentImmune.Any(x => x.Value.ToString() == tokens[2]) ? 100 : 0;
                    break;

                case "Monster":
                    hit = attackData.Hit;
                    evade = target.Stats.Evade.Value;
                    break;

                case "MonsterMagic":
                    hit = attackData.Hit;
                    evade = target.Stats.MagicEvade.Value;
                    break;
            }

            if (multiTarget && commandRecord.Targetting != TargetType.All &&
                               commandRecord.Targetting != TargetType.AllEnemy &&
                               commandRecord.Targetting != TargetType.AllAlly)
            {
                hit /= 2;
            }

            if (attacker.Stats.StatusAilments.ModelList.Any(x => x.Value == AilmentType.Fear))
            {
                hit /= 2;
            }

            if (attacker.Stats.StatusAilments.ModelList.Any(x => x.Value == AilmentType.Confusion))
            {
                hit -= 10;
            }

            if (attacker == target)
            {
                hit = 100;
                evade = 0;
            }

            if (Rng.RandomInt(0, 99) > hit)
            {
                Audio.PlaySound(GameSound.Error);
                target.Miss();

                return false;
            }
            else if (Rng.RandomInt(0, 99) < evade)
            {
                Audio.PlaySound(GameSound.Error);
                target.Miss();

                return false;
            }
            else
            {
                return true;
            }
        }

        private void CalculateDamage(string[] tokens)
        {
            int attack = 0;
            int multiplier = 0;
            int defense = 0;
            int damage = 0;

            switch (tokens[1])
            {
                case "Fists":
                    attack = Rng.RandomInt(0, attacker.Stats.Level.Value / 4) + 3;
                    multiplier = 2;
                    defense = target.Stats.Defense.Value;
                    break;

                case "Swords":
                    attack = attacker.Stats.Attack.Value + Rng.RandomInt(0, attacker.Stats.Attack.Value / 8);
                    multiplier = attacker.Stats.Strength.Value * attacker.Stats.Level.Value / 256 + 2;
                    defense = target.Stats.Defense.Value;
                    break;

                case "Staves":
                    attack = Rng.RandomInt(attacker.Stats.Attack.Value / 2, attacker.Stats.Attack.Value);
                    multiplier = attacker.Stats.Strength.Value * attacker.Stats.Level.Value / 256 + 2;
                    defense = target.Stats.Defense.Value;
                    break;

                case "Axes":
                    attack = attacker.Stats.Attack.Value / 2 + Rng.RandomInt(0, attacker.Stats.Attack.Value);
                    multiplier = attacker.Stats.Strength.Value * attacker.Stats.Level.Value / 256 + 2;
                    defense = target.Stats.Defense.Value / 4;
                    break;

                case "Bows":
                    attack = attacker.Stats.Attack.Value + Rng.RandomInt(0, 3);
                    multiplier = attacker.Stats.Agility.Value * attacker.Stats.Level.Value / 256 + 2;
                    defense = target.Stats.Defense.Value;
                    break;

                case "Magic":
                    attack = ((AbilityRecord)commandRecord).Power + Rng.RandomInt(0, ((AbilityRecord)commandRecord).Power / 8);
                    multiplier = attacker.Stats.Magic.Value * attacker.Stats.Level.Value / 256 + 4;
                    defense = target.Stats.MagicDefense.Value;
                    break;

                case "Throw":
                    attack = ((ItemRecord)commandRecord).Attack;
                    attack += Rng.RandomInt(0, attack / 8);
                    attack *= 2;
                    multiplier = attacker.Stats.Strength.Value * attacker.Stats.Level.Value / 256 + 2;
                    defense = target.Stats.Defense.Value;
                    break;

                case "Monster":
                    attack = attackData.Power < 0 ? attacker.Stats.Attack.Value : attackData.Power;
                    attack += Rng.RandomInt(0, attack / 8);
                    multiplier = attacker.Stats.AttackMultiplier.Value;
                    defense = target.Stats.Defense.Value;
                    break;

                case "MonsterMagic":
                    attack = attackData.Power < 0 ? attacker.Stats.Magic.Value : attackData.Power;
                    attack += Rng.RandomInt(0, attack / 8);
                    multiplier = attacker.Stats.MagicMultiplier.Value;
                    defense = target.Stats.MagicDefense.Value;
                    break;

                default:
                    attack = int.Parse(tokens[1]);
                    defense = 0;
                    multiplier = 1;
                    break;
            }

            if (multiTarget && battleScene.EnemyList.Count() > 1 &&
                               commandRecord.Targetting != TargetType.All &&
                               commandRecord.Targetting != TargetType.AllEnemy &&
                               commandRecord.Targetting != TargetType.AllAlly)
            {
                if ((target is BattlePlayer && battleScene.PlayerList.Where(x => !x.Dead).Count() > 1) || (target is BattleEnemy && battleScene.EnemyList.Count() > 1))
                {
                    attack /= 2;
                }
            }

            if (tokens.Length > 2)
            {
                ElementType element = (ElementType)Enum.Parse(typeof(ElementType), tokens[2]);

                if (target.Stats.ElementWeak.Any(x => x.Value == element))
                {
                    attack *= 2;
                    defense = 0;
                }

                if (target.Stats.ElementStrong.Any(x => x.Value == element))
                {
                    attack /= 2;
                }

                if (target.Stats.ElementImmune.Any(x => x.Value == element))
                {
                    attack = 0;
                }

                if (target.Stats.ElementAbsorb.Any(x => x.Value == element))
                {
                    int healing = attack * multiplier;
                    if (healing < 1) healing = 1;
                    if (healing > 9999) healing = 9999;

                    target.Heal(healing);

                    return;
                }
            }

            damage = (attack - defense) * multiplier;
            if (damage < 1) damage = 1;
            if (damage > 9999) damage = 9999;
            target.Damage(damage);
        }

        private void CalculateHealing(string[] tokens)
        {
            if (tokens.Length == 2) target.Heal(int.Parse(tokens[1]));
            else
            {
                var abilityRecord = ((AbilityRecord)commandRecord);
                int power = abilityRecord.Power + Rng.RandomInt(0, abilityRecord.Power / 8);
                int multiplier = attacker.Stats.Level.Value * attacker.Stats.Magic.Value / 256 + 4;

                int healing = power * multiplier;

                if (multiTarget &&
                               commandRecord.Targetting != TargetType.All &&
                               commandRecord.Targetting != TargetType.AllEnemy &&
                               commandRecord.Targetting != TargetType.AllAlly)
                {
                    if ((target is BattlePlayer && battleScene.PlayerList.Where(x => !x.Dead).Count() > 1) || (target is BattleEnemy && battleScene.EnemyList.Count() > 1))
                    {
                        healing /= 2;
                    }
                }

                if (healing < 1) healing = 1;
                if (healing > 9999) healing = 9999;

                target.Heal(healing);
            }
        }

        private void CalculateReplenish(string[] tokens)
        {
            target.Replenish(int.Parse(tokens[1]));
        }

        private void Flash(string[] tokens)
        {
            target.FlashColor(new Color(byte.Parse(tokens[1]), byte.Parse(tokens[2]), byte.Parse(tokens[3])));
        }

        private void Attack(string[] tokens)
        {
            List<BattlePlayer> eligibleTargets = battleScene.PlayerList.FindAll(x => !x.Dead);
            target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];

            scriptParser.RunScript("Animate Attack\nWait 600\nSound Slash\nCenterEffect Bash $targetCenterX $targetCenterY 2\nOnHit Monster Strength\nFlash 255 27 0\nDamage Monster Blunt");
        }

        private void Flee(string[] tokens)
        {
            //StackDialogue("You flee...");

            var convoRecord = new ConversationScene.ConversationRecord()
            {
                DialogueRecords = new ConversationScene.DialogueRecord[] {
                            new ConversationScene.DialogueRecord() { Text = "Escaped from battle." }
                        }
            };

            convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-145, 30, 290, 60));
            scriptParser.BlockScript();
            convoScene.OnTerminated += battleScene.BattleViewModel.Close;
            CrossPlatformGame.StackScene(convoScene);

            // timeleft = 1000;
        }

        private void Dialogue(string[] tokens)
        {
            if (tokens.Length == 2)
            {
                convoScene = new ConversationScene.ConversationScene(tokens[1], new Rectangle(-145, 30, 290, 60), true);
                var unblock = scriptParser.BlockScript();
                convoScene.ConversationViewModel.OnDialogueScrolled += new Action(unblock);
                CrossPlatformGame.StackScene(convoScene);

                // timeleft = 1000;
            }
            else
            {
                StackDialogue(String.Join(' ', tokens.Skip(1)));
            }
        }

        private void Analyze(string[] tokens)
        {
            var enemy = target as BattleEnemy;
            if (enemy != null) enemy.Scoped = true;

            string analysis = $"{target.Stats.Name}: HP {target.Stats.HP}/{target.Stats.MaxHP}. " + target.Stats.Analysis.Value;
            StackDialogue(analysis, false);
        }

        private void StackDialogue(string text, bool autoProceed = true)
        {
            var convoRecord = new ConversationScene.ConversationRecord()
            {
                DialogueRecords = new ConversationScene.DialogueRecord[] { new ConversationScene.DialogueRecord() { Text = text } }
            };

            convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-145, 30, 290, 60), autoProceed);
            var unblock = scriptParser.BlockScript();
            if (autoProceed) convoScene.ConversationViewModel.OnDialogueScrolled += new Action(unblock);
            else
            {
                CommandViewModel.ConfirmCooldown = true;
                convoScene.ConversationViewModel.OnTerminated += new Action(() =>
                {
                    unblock();
                    CommandViewModel.ConfirmCooldown = false;
                });
            }
            CrossPlatformGame.StackScene(convoScene);

            if (autoProceed) timeleft = 1000;
        }
    }
}
