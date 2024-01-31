using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Elenigma.Scenes.ConversationScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.StatusScene
{
    public class SystemViewModel : ViewModel
    {
        StatusScene statusScene;

        ViewModel childViewModel;

        public SystemViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            var saves = GameProfile.GetAllSaveData();
            for (int i = 0; i < 3; i++)
            {
                if (saves.ContainsKey(i))
                {
                    var save = saves[i];
                    GameSprite portrait1 = (GameSprite)Enum.Parse(typeof(GameSprite), (string)save["EnviPortrait"]);
                    GameSprite portrait2 = (GameSprite)Enum.Parse(typeof(GameSprite), (string)save["SparrPortrait"]);
                    AvailableSaves.Add(new TitleScene.SaveModel()
                    {
                        Location = new ModelProperty<string>((string)save["PlayerLocation"]),
                        SaveSlot = new ModelProperty<int>(i),
                        Portrait1 = new ModelProperty<Texture2D>(AssetCache.SPRITES[portrait1]),
                        Portrait2 = new ModelProperty<Texture2D>(AssetCache.SPRITES[portrait2])
                    });
                }
                else
                {
                    AvailableSaves.Add(new TitleScene.SaveModel()
                    {
                        Location = new ModelProperty<string>("- Empty Save -"),
                        SaveSlot = new ModelProperty<int>(i),
                        Portrait1 = new ModelProperty<Texture2D>(AssetCache.SPRITES[GameSprite.Actors_Blank]),
                        Portrait2 = new ModelProperty<Texture2D>(AssetCache.SPRITES[GameSprite.Actors_Blank])
                    });
                }
            }

            LoadView(GameView.StatusScene_SystemView);

            int slot = GameProfile.SaveSlot == -1 ? 0 : GameProfile.SaveSlot;
            GetWidget<RadioBox>("SaveList").Selection = slot;
            (GetWidget<RadioBox>("SaveList").ChildList[slot] as RadioButton).RadioSelect();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (childViewModel != null)
            {
                if (childViewModel.Terminated)
                {
                    childViewModel = null;
                    GetWidget<RadioBox>("SaveList").Enabled = true;
                }
                else return;
            }

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Close();
            }
        }

        public void Save(object parameter)
        {
            int saveSlot;
            if (parameter is IModelProperty)
            {
                saveSlot = (int)((IModelProperty)parameter).GetValue();
            }
            else saveSlot = (int)parameter;
            GameProfile.SaveSlot = saveSlot;

            if (AvailableSaves[saveSlot].Location.Value == "- Empty Save -")
            {
                ((MapScene.MapScene)CrossPlatformGame.SceneStack.First(x => x is MapScene.MapScene)).SaveMapPosition();
                
                string portrait1 = GameProfile.PlayerProfile.Party[0].Portrait.Value;
                string portrait2 = GameProfile.PlayerProfile.Party.Count() > 1 ? GameProfile.PlayerProfile.Party[1].Portrait.Value : GameSprite.Actors_Blank.ToString();
                GameProfile.SetSaveData<string>("EnviPortrait", portrait1);
                GameProfile.SetSaveData<string>("SparrPortrait", portrait2);

                var save = GameProfile.SaveData;

                AvailableSaves[saveSlot].Location.Value = MapScene.MapScene.Instance.LocationName;
                AvailableSaves[saveSlot].Portrait1.Value = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), portrait1)];
                AvailableSaves[saveSlot].Portrait2.Value = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), portrait2)];

                GameProfile.SaveState();

                statusScene.Saved = true;
            }
            else
            {
                var dialogueRecords = new List<ConversationScene.DialogueRecord>();

                dialogueRecords.Add(new ConversationScene.DialogueRecord()
                {
                    Text = "Overwrite existing save?",
                    Script = new string[] { "WaitForText", "SelectionPrompt", "No", "Yes", "End", "ProceedText" }
                });

                var convoRecord = new ConversationScene.ConversationRecord()
                {
                    DialogueRecords = dialogueRecords.ToArray()
                };
                var convoScene = new ConversationScene.ConversationScene(convoRecord);
                CrossPlatformGame.StackScene(convoScene, true);
                convoScene.OnTerminated += new TerminationFollowup(() =>
                {
                    if (GameProfile.GetSaveData<string>("LastSelection") == "Yes")
                    {
                        ((MapScene.MapScene)CrossPlatformGame.SceneStack.First(x => x is MapScene.MapScene)).SaveMapPosition();

                        string portrait1 = GameProfile.PlayerProfile.Party[0].Portrait.Value;
                        string portrait2 = GameProfile.PlayerProfile.Party.Count() > 1 ? GameProfile.PlayerProfile.Party[1].Portrait.Value : GameSprite.Actors_Blank.ToString();
                        GameProfile.SetSaveData<string>("EnviPortrait", portrait1);
                        GameProfile.SetSaveData<string>("SparrPortrait", portrait2);

                        var save = GameProfile.SaveData;

                        AvailableSaves[saveSlot].Location.Value = MapScene.MapScene.Instance.LocationName;
                        AvailableSaves[saveSlot].Portrait1.Value = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), portrait1)];
                        AvailableSaves[saveSlot].Portrait2.Value = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), portrait2)];

                        GameProfile.SaveState();

                        statusScene.Saved = true;
                    }
                });
            }
        }

        public ModelCollection<TitleScene.SaveModel> AvailableSaves { get; set; } = new ModelCollection<TitleScene.SaveModel>();
    }
}
