using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects.Maps;
using Elenigma.SceneObjects.Widgets;
using Elenigma.Scenes.SplashScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.TitleScene
{
    public class SaveModel
    {
        public ModelProperty<string> Location { get; set; }
        public ModelProperty<int> SaveSlot { get; set; }
        public ModelProperty<Texture2D> Portrait1 { get; set; }
        public ModelProperty<Texture2D> Portrait2 { get; set; }
    }

    public class TitleViewModel : ViewModel
    {


        private ViewModel settingsViewModel;

        public ModelCollection<SaveModel> AvailableSaves { get; set; } = new ModelCollection<SaveModel>();

        private RadioBox commandBox;


        public List<string> AvailableCommands { get; set; } = new List<string>() { "New Game", "Continue", "Credits", "Map Test" };

        public TitleViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel)
        {
            /*
            GameProfile.NewState();

            var saves = GameProfile.GetAllSaveData();
            foreach (var saveEntry in saves)
            {
                var save = saveEntry.Value;

                GameSprite portrait1 = (GameSprite)Enum.Parse(typeof(GameSprite), (string)save["EnviPortrait"]);
                GameSprite portrait2 = (GameSprite)Enum.Parse(typeof(GameSprite), (string)save["SparrPortrait"]);
                AvailableSaves.Add(new SaveModel()
                {
                    Location = new ModelProperty<string>((string)save["PlayerLocation"]),
                    SaveSlot = new ModelProperty<int>(saveEntry.Key),
                    Portrait1 = new ModelProperty<Texture2D>(AssetCache.SPRITES[portrait1]),
                    Portrait2 = new ModelProperty<Texture2D>(AssetCache.SPRITES[portrait2])
                });
            }
            */

            LoadView(GameView.TitleScene_TitleView);

            var saves = GameProfile.GetAllSaveData();
            if (saves.Count == 0)
            {
                commandBox = GetWidget<RadioBox>("CommandBox");
                commandBox.Selection = 0;
                (commandBox.ChildList[1] as RadioButton).Enabled = false;
            }
            else
            {
                commandBox = GetWidget<RadioBox>("CommandBox");
                commandBox.Selection = 1;
                (commandBox.ChildList[1] as RadioButton).RadioSelect();
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (settingsViewModel != null)
            {
                if (settingsViewModel.Terminated)
                {
                    settingsViewModel = null;
                    commandBox.Enabled = true;
                }
            }
        }

        public void SelectCommand(object parameter)
        {
            if (settingsViewModel != null)
            {
                Audio.PlaySound(GameSound.Confirm);
                return;
            }

            string command;
            if (parameter is IModelProperty) command = (string)((IModelProperty)parameter).GetValue();
            else command = (string)parameter;

            switch (command)
            {
                /*
                case "New Game":
                    commandBox.Enabled = false;
                    GameProfile.NewState();
                    GameProfile.PlayerProfile.Party.Add(new HeroModel("Arthur", "Portraits_YoungMC", "Actors_YoungMC"));
                    CrossPlatformGame.Transition(typeof(MapScene.MapScene), GameMap.TechWorldIntro, 22, 37, Orientation.Down);
                    break;

                case "Continue":
                    commandBox.Enabled = false;
                    settingsViewModel = new ContinueViewModel(parentScene);
                    parentScene.AddOverlay(settingsViewModel);
                    break;
                */

                case "Settings": commandBox.Enabled = false; SettingsMenu(); break;
                case "Credits": commandBox.Enabled = false; Credits(); break;
                case "Map Test":
                    commandBox.Enabled = false;
                    GameProfile.NewState();

                    var tank = new HeroModel(HeroType.TheTank, ClassType.Tank, 15);
                    tank.Equip("Battleaxe");
                    tank.Equip("Talisman");
                    tank.Equip("Steel Mail");
                    GameProfile.PlayerProfile.Party.Add(tank);

                    var warrior = new HeroModel(HeroType.TheWarrior, ClassType.Warrior, 15);
                    warrior.Equip("Battleaxe");
                    warrior.Equip("Bracers");
                    warrior.Equip("Steel Mail");
                    GameProfile.PlayerProfile.Party.Add(warrior);

                    var mage = new HeroModel(HeroType.TheMage, ClassType.Mage, 15);
                    mage.Equip("Mahogany");
                    mage.Equip("Talisman");
                    mage.Equip("Fancy Robes");
                    GameProfile.PlayerProfile.Party.Add(mage);

                    var cleric = new HeroModel(HeroType.TheCleric, ClassType.Cleric, 15);
                    cleric.Equip("Mahogany");
                    cleric.Equip("Talisman");
                    cleric.Equip("Fancy Robes");
                    GameProfile.PlayerProfile.Party.Add(cleric);

                    CrossPlatformGame.Transition(typeof(CrawlerScene.CrawlerScene), 0);
                    break;
            }
        }

        public void Continue(object saveSlot)
        {
            // GetWidget<Button>("NewGame").UnSelect();

            /*
            GameProfile.LoadState("Save" + saveSlot.ToString() + ".sav");

            string mapName = GameProfile.GetSaveData<string>("LastMapName");
            Vector2 mapPosition = new Vector2(GameProfile.GetSaveData<int>("LastPositionX"), GameProfile.GetSaveData<int>("LastPositionY"));

            CrossPlatformGame.Transition(typeof(MapScene.MapScene), mapName, mapPosition);
            */
        }

        public void SettingsMenu()
        {
            settingsViewModel = new SettingsViewModel(parentScene, GameView.TitleScene_SettingsView);
            parentScene.AddOverlay(settingsViewModel);
        }

        public void Credits()
        {
            //CrossPlatformGame.Transition(typeof(CreditsScene.CreditsScene));
            settingsViewModel = new CreditsScene.CreditsViewModel(parentScene, GameView.CreditsScene_CreditsView);
            parentScene.AddOverlay(settingsViewModel);
        }

        public void Exit()
        {
            Settings.SaveSettings();

            CrossPlatformGame.GameInstance.Exit();
        }

        public override void Terminate()
        {
            base.Terminate();

            settingsViewModel.Terminate();
        }
    }
}
