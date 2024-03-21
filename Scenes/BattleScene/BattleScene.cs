using Microsoft.Xna.Framework.Graphics;
using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMOD;
using Elenigma.SceneObjects.Maps;

namespace Elenigma.Scenes.BattleScene
{
    public class BattleScene : Scene
    {
        public List<BattlePlayer> PlayerList { get; } = new List<BattlePlayer>();
        public List<BattleEnemy> EnemyList { get; } = new List<BattleEnemy>();
        public BattleViewModel BattleViewModel { get => battleViewModel; }

        public bool BattleOver { get; private set; }

        private EncounterRecord encounterRecord;

        private BattleViewModel battleViewModel;

        private bool introFinished = false;

        private GameMusic mapMusic = Audio.CurrentMusic;


        protected List<Particle> overlayParticleList = new List<Particle>();

        public List<BattleController> BattleEventQueue { get; } = new List<BattleController>();

        public BattleScene(string encounterName)
        {
            encounterRecord = EncounterRecord.ENCOUNTERS.First(x => x.Name == encounterName);

            EncounterEnemy[] encounterEnemy = encounterRecord.Enemies;
            List<Texture2D> enemySpriteList = new List<Texture2D>();
            int totalEnemyWidth = 0;
            foreach (EncounterEnemy enemy in encounterEnemy)
            {
                EnemyRecord enemyData = EnemyRecord.ENEMIES.First(x => x.Name == enemy.Name);
                Texture2D enemySprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + enemyData.Sprite)];
                totalEnemyWidth += enemySprite.Width;
            }


            battleViewModel = AddView(new BattleViewModel(this, EncounterRecord.ENCOUNTERS.First(x => x.Name == encounterName)));

            List<Battler> battlerList = new List<Battler>();
            battlerList.AddRange(PlayerList);
            battlerList.AddRange(EnemyList);

            TargetViewModel.ClearLastTarget();
        }

        public override void BeginScene()
        {
            sceneStarted = true;

            TransitionController transitionController = new TransitionController(TransitionDirection.In, 300);
            IntroShader colorFade = new IntroShader(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
            transitionController.FinishTransition += new Action<TransitionDirection>(t => colorFade.Terminate());
            AddController(transitionController);
            CrossPlatformGame.TransitionShader = colorFade;

            if (encounterRecord.Music != GameMusic.None) Audio.PlayMusic(encounterRecord.Music);
            else Audio.PlayMusic(GameMusic.ChoiceEncounter);
        }

        public override void EndScene()
        {
            Audio.PlayMusic(mapMusic);

            if (GameProfile.PlayerProfile.Party.Any(x => x.Value.HP.Value > 0))
            {
                foreach (ModelProperty<HeroModel> heroModel in GameProfile.PlayerProfile.Party)
                {
                    if (heroModel.Value.HP.Value <= 0) heroModel.Value.HP.Value = 1;
                }
            }

            base.EndScene();
        }

        public static void Initialize()
        {
            BattleEnemy.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            int i = 0;
            while (i < overlayParticleList.Count) { overlayParticleList[i].Update(gameTime); i++; }
            overlayParticleList.RemoveAll(x => x.Terminated);

            if (battleViewModel.EnemyPanel.Transitioning) return;
            if (PriorityLevel == PriorityLevel.TransitionLevel) return;
            if (BattleOver) return;
            if (battleViewModel.Closed) return;

            int enemiesRemoved = EnemyList.RemoveAll(x => x.Terminated);
            if (enemiesRemoved > 0) BattleViewModel.UpdateEnemyHeaders();

            if (!introFinished)
            {
                introFinished = true;
                if (!string.IsNullOrEmpty(encounterRecord.Intro))
                {
                    var convoRecord = new ConversationScene.ConversationRecord()
                    {
                        DialogueRecords = new ConversationScene.DialogueRecord[] {
                            new ConversationScene.DialogueRecord() { Text = encounterRecord.Intro }
                        }
                    };
                    var convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-145, 30, 290, 60), true);
                    CrossPlatformGame.StackScene(convoScene);
                }
            }

            if (!controllerList.Any(y => y.Any(x => x is BattleController)))
            {
                if (!EnemyList.Any(x => !x.Terminated))
                {
                    Victory();
                    return;
                }
                else if (!PlayerList.Any(x => !x.Dead))
                {
                    Defeat();
                    return;
                }

                if (CrossPlatformGame.CurrentScene == this)
                {
                    if (BattleEventQueue.Count > 0 && overlayParticleList.Count == 0)
                    {
                        var nextController = BattleEventQueue.First();
                        BattleEventQueue.RemoveAt(0);

                        if (!nextController.Attacker.Dead)
                        {
                            nextController.FixTargetting();
                            AddController(nextController);

                            (nextController.Attacker as BattlePlayer)?.ResetCommand();
                        }
                    }
                    else
                    {
                        if (!InitiativeSuspended)
                        {
                            foreach (Battler battler in PlayerList)
                            {
                                if (battler.Dead) continue;
                                battler.IncrementInitiative(gameTime);
                            }

                            foreach (Battler battler in EnemyList)
                            {
                                if (battler.Dead) continue;
                                battler.IncrementInitiative(gameTime);
                            }
                        }

                        if (!overlayList.Any(y => y is CommandViewModel))
                        {
                            Battler nextPlayer = PlayerList.FirstOrDefault(x => x.Ready);
                            nextPlayer?.StartTurn();
                        }
                        
                        Battler nextEnemy = EnemyList.FirstOrDefault(x => x.Ready);
                        nextEnemy?.StartTurn();
                    }
                }
            }
        }

        public override void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D pixelRender, RenderTarget2D compositeRender)
        {
            graphicsDevice.SetRenderTarget(pixelRender);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawBackground(spriteBatch);
            spriteBatch.End();

            Matrix matrix = (Camera == null) ? Matrix.Identity : Camera.Matrix;
            Effect shader = (spriteShader == null) ? null : spriteShader.Effect;
            foreach (Entity entity in entityList) entity.DrawShader(spriteBatch, Camera, matrix);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, matrix);
            DrawGame(spriteBatch, shader, matrix);
            spriteBatch.End();

            foreach (BattleEnemy battleEnemy in EnemyList) battleEnemy.DrawShader(spriteBatch);
            graphicsDevice.SetRenderTarget(pixelRender);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawOverlay(spriteBatch);
            spriteBatch.End();

            foreach (BattlePlayer battlePlayer in PlayerList) battlePlayer.DrawShader(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            foreach (Particle particle in overlayParticleList) particle.Draw(spriteBatch, null);
            foreach (BattlePlayer battlePlayer in PlayerList) battlePlayer.DrawAilment(spriteBatch);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(compositeRender);

            if (!CrossPlatformGame.ClearedCompositeRender)
            {
                CrossPlatformGame.ClearedCompositeRender = true;
                graphicsDevice.Clear(Color.Transparent);
            }

            shader = (SceneShader == null) ? null : SceneShader.Effect;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, Matrix.Identity);
            spriteBatch.Draw(pixelRender, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.End();
        }

        public void AddBattler(Battler battler)
        {
            if (battler is BattleEnemy) EnemyList.Add(battler as BattleEnemy);
            else if (battler is BattlePlayer) PlayerList.Add(battler as BattlePlayer);
        }

        public override T AddParticle<T>(T newParticle)
        {
            if (newParticle.Foreground) overlayParticleList.Add(newParticle);
            else particleList.Add(newParticle);
            return newParticle;
        }

        private void Victory()
        {
            BattleOver = true;

            Task.Delay(500).ContinueWith(t =>
            {
                int expGain = 0;
                foreach (EncounterEnemy enemy in encounterRecord.Enemies)
                {
                    EnemyRecord enemyRecord = EnemyRecord.ENEMIES.First(x => x.Name == enemy.Name);
                    expGain += enemyRecord.Exp;
                }

                List<ConversationScene.DialogueRecord> victoryRecords = new List<ConversationScene.DialogueRecord>();

                var survivorList = PlayerList.Where(x => !x.Dead);
                string participation;
                if (survivorList.Count() == 2) participation = $"{survivorList.First().Stats.Name.Value} and {survivorList.Last().Stats.Name.Value}";
                else participation = survivorList.First().Stats.Name.Value;
                victoryRecords.Add(new ConversationScene.DialogueRecord { Text = $"Good work, all foes have been dispatched! {participation} gained {expGain} experience points." });

                foreach (var battlePlayer in PlayerList)
                {
                    if (battlePlayer.Dead) continue;
                    List<ConversationScene.DialogueRecord> reports = battlePlayer.HeroModel.GrowAfterBattle(expGain);
                    foreach (ConversationScene.DialogueRecord report in reports) victoryRecords.Add(report);
                    battlePlayer.HealAilment(AilmentType.Confusion);
                    battlePlayer.Animate("Victory");
                }
                var victoryConversation = new ConversationScene.ConversationRecord() { DialogueRecords = victoryRecords.ToArray() };

                var convoScene = new ConversationScene.ConversationScene(victoryConversation, new Rectangle(-145, 30, 290, 60));
                convoScene.OnTerminated += new TerminationFollowup(() =>
                {
                    TransitionController transitionOut = new TransitionController(TransitionDirection.Out, 600);
                    ColorFade colorFade = new ColorFade(Color.Black, transitionOut.TransitionProgress);
                    transitionOut.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
                    this.AddController(transitionOut);
                    this.SceneShader = colorFade;
                    CrossPlatformGame.TransitionShader = colorFade;
                    transitionOut.FinishTransition += new Action<TransitionDirection>(t => EndScene());
                });

                CrossPlatformGame.StackScene(convoScene);
            });
        }

        private void Defeat()
        {
            BattleOver = true;

            List<ConversationScene.DialogueRecord> defeatRecords = new List<ConversationScene.DialogueRecord>();
            defeatRecords.Add(new ConversationScene.DialogueRecord {
                Text = $"Better luck next life...",
                Script = new string[] { "Wait 3000", "ProceedText" }
            });

            var defeatConversation = new ConversationScene.ConversationRecord()
            {
                DialogueRecords = defeatRecords.ToArray()
            };

            var convoScene = new ConversationScene.ConversationScene(defeatConversation, new Rectangle(-145, 30, 290, 60));
            convoScene.OnTerminated += new TerminationFollowup(() =>
            {
                TransitionController transitionOut = new TransitionController(TransitionDirection.Out, 600);
                ColorFade colorFade = new ColorFade(Color.Black, transitionOut.TransitionProgress);
                transitionOut.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
                this.AddController(transitionOut);
                this.SceneShader = colorFade;
                CrossPlatformGame.TransitionShader = colorFade;
                transitionOut.FinishTransition += new Action<TransitionDirection>(t => EndScene());
            });

            CrossPlatformGame.StackScene(convoScene);
        }

        public bool InitiativeSuspended
        {
            get => overlayList.Any(x => { var controller = x as CommandViewModel; return (controller != null && controller.SubmenuActive) || x is TargetViewModel; });
        }
    }
}
