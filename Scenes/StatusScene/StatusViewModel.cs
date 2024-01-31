using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.StatusScene
{
    public class StatusViewModel : ViewModel
    {
        List<ViewModel> SubViews { get; set; } = new List<ViewModel>();

        StatusScene statusScene;

        RadioBox partyList;
        RadioBox commandBox;

        bool returnToTitle = false;

        public ViewModel ChildViewModel { get; set; }

        public string LocationName { get; set; }

        public StatusViewModel(StatusScene iScene, string locationName)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            LocationName = locationName;

            AvailableCommands.Add("Item");
            AvailableCommands.Add("Save");
            AvailableCommands.Add("Quit");

            LoadView(GameView.StatusScene_StatusView);

            partyList = GetWidget<RadioBox>("PartyList");
            commandBox = GetWidget<RadioBox>("CommandBox");

            partyList.Selection = -1;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (returnToTitle)
            {
                CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
                returnToTitle = false;
                return;
            }

            if (ChildViewModel != null)
            {
                if (ChildViewModel.Terminated)
                {
                    if (ChildViewModel is ItemViewModel || ChildViewModel is SystemViewModel) commandBox.Enabled = true;
                    else
                    {
                        partyList.Enabled = true;
                    }

                    ChildViewModel = null;
                }
                return;
            }

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);

                if (commandBox.Enabled) Close();
                else
                {
                    commandBox.Enabled = true;
                    partyList.Enabled = false;
                    partyList.Selection = -1;
                }
            }
        }

        public void SelectCommand(object parameter)
        {
            string command;
            
            if (parameter is IModelProperty)
            {
                command = (string)((IModelProperty)parameter).GetValue();
            }
            else command = (string)parameter;

            switch (command)
            {
                case "Item":
                    commandBox.Enabled = false;
                    ChildViewModel = statusScene.AddView(new ItemViewModel(statusScene));
                    break;

                case "Save":
                    commandBox.Enabled = false;
                    partyList.Enabled = false;
                    ChildViewModel = statusScene.AddView(new SystemViewModel(statusScene));
                    break;

                case "Quit":
                    if (statusScene.Saved) CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
                    else
                    {
                        var dialogueRecords = new List<ConversationScene.DialogueRecord>();

                        dialogueRecords.Add(new ConversationScene.DialogueRecord()
                        {
                            Text = "Quit without saving?",
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
                                commandBox.Enabled = false;
                                returnToTitle = true;
                            }
                        });
                    }
                    break;
            }
        }

        public ModelCollection<string> AvailableCommands { get; set; } = new ModelCollection<string>();
    }
}
