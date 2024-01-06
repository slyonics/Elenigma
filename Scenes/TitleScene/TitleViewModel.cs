using Elenigma.Main;
using Elenigma.Models;
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


        public List<string> AvailableCommands { get; set; } = new List<string>() { "New Game", "Continue", "Credits", "Exit" };

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

            commandBox = GetWidget<RadioBox>("CommandBox");
            commandBox.Selection = 1;
            (commandBox.ChildList[1] as RadioButton).RadioSelect();
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
                Audio.PlaySound(GameSound.Selection);
                return;
            }

            string command;
            if (parameter is IModelProperty) command = (string)((IModelProperty)parameter).GetValue();
            else command = (string)parameter;

            switch (command)
            {
                case "New Game": commandBox.Enabled = false; SplashScene.SplashScene.NewGame(); break;

                case "Continue":
                    commandBox.Enabled = false;
                    settingsViewModel = new ContinueViewModel(parentScene);
                    parentScene.AddOverlay(settingsViewModel);
                    break;

                case "Settings": commandBox.Enabled = false; SettingsMenu(); break;
                case "Credits": commandBox.Enabled = false; Credits(); break;
                case "Exit": commandBox.Enabled = false; CrossPlatformGame.GameInstance.Exit(); break;
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
