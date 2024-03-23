using Microsoft.Xna.Framework.Graphics;
using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.BattleScene
{
    public class EnemyHeader
    {
        public string Name { get; set; }
        public ModelProperty<int> Count { get; set; }
    }

    public class BattleViewModel : ViewModel
    {
        BattleScene battleScene;
        CommandViewModel commandViewModel;

        public BattleViewModel(BattleScene iScene, EncounterRecord encounterRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;

            EncounterEnemy[] encounterEnemies = encounterRecord.Enemies;
            int totalEnemyWidth = 0;
            foreach (EncounterEnemy enemy in encounterEnemies)
            {
                EnemyRecord enemyRecord = new EnemyRecord(EnemyRecord.ENEMIES.First(x => x.Name == enemy.Name));
                Texture2D enemySprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + enemyRecord.Sprite)];
                totalEnemyWidth += enemySprite.Width;
                InitialEnemies.Add(enemyRecord);
            }

            foreach (string enemyName in InitialEnemies.Select(x => x.Name).Distinct())
            {
                EnemyNames.Add(new EnemyHeader() { Name = enemyName, Count = new ModelProperty<int>(InitialEnemies.Count(x => x.Name == enemyName)) });
            }

            foreach (var model in GameProfile.PlayerProfile.Party)
            {
                model.Value.NameColor = new ModelProperty<Color>(new Color(252, 252, 252, 255));
                model.Value.HealthColor = new ModelProperty<Color>(new Color(252, 252, 252, 255));
            }

            int i = 0;
            foreach (EncounterEnemy enemy in encounterEnemies)
            {
                if (enemy.BattleOffsetX != 0 || enemy.BattleOffsetY != 0)
                {
                    InitialEnemies[i].BattleAlignment = Alignment.Relative;
                    InitialEnemies[i].BattleOffsetX = enemy.BattleOffsetX;
                    InitialEnemies[i].BattleOffsetY = enemy.BattleOffsetY;
                }
                i++;
            }

            LoadView(GameView.BattleScene_BattleView);

            EnemyPanel = GetWidget<Panel>("EnemyPanel");
        }

        public void UpdateEnemyHeaders()
        {
            List<ModelProperty<EnemyHeader>> headers = new List<ModelProperty<EnemyHeader>>();
            foreach (string enemyName in battleScene.EnemyList.Select(x => x.Stats.Name.Value).Distinct())
            {
                ModelProperty<EnemyHeader> headerProperty = new ModelProperty<EnemyHeader>(new EnemyHeader() { Name = enemyName, Count = new ModelProperty<int>(battleScene.EnemyList.Count(x => x.Stats.Name.Value == enemyName)) });
                headers.Add(headerProperty);
            }

            EnemyNames.ModelList = headers;
        }

        public void StartPlayerTurn(BattlePlayer battlePlayer)
        {
            Audio.PlaySound(GameSound.Ready);
            PlayerTurn.Value = true;
            commandViewModel = new CommandViewModel(battleScene, battlePlayer);
            battleScene.AddView(commandViewModel);
        }

        public void EndPlayerTurn(BattlePlayer battlePlayer)
        {
            PlayerTurn.Value = false;
            commandViewModel.Terminate();
        }

        public override void Close()
        {
            base.Close();

            battleScene.EnemyList.Clear();
        }

        public override void Terminate()
        {
            base.Terminate();

            battleScene.EndScene();
        }

        public List<EnemyRecord> InitialEnemies { get; set; } = new List<EnemyRecord>();

        public ModelProperty<Rectangle> EnemyMargin { get; set; } = new ModelProperty<Rectangle>(new Rectangle());

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<bool> PlayerTurn { get; set; } = new ModelProperty<bool>(false);
        public ModelCollection<EnemyHeader> EnemyNames { get; set; } = new ModelCollection<EnemyHeader>();
        public Panel EnemyPanel { get; private set; }
    }
}
