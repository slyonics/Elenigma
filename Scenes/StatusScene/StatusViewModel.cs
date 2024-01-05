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

        public StatusViewModel(StatusScene iScene, string locationName, bool canSave)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            LocationName = locationName;

            AvailableCommands.Add("Item");
            AvailableCommands.Add("Skill");
            AvailableCommands.Add("Equip");
            AvailableCommands.Add("Status");
            AvailableCommands.Add("Save");
            AvailableCommands.Add("Quit");

            LoadView(GameView.StatusScene_StatusView);

            partyList = GetWidget<RadioBox>("PartyList");
            commandBox = GetWidget<RadioBox>("CommandBox");

            if (!canSave) commandBox.ChildList[4].Enabled = false;

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

                case "Skill":
                    commandBox.Enabled = false;
                    partyList.Enabled = true;
                    ((RadioButton)partyList.ChildList[0]).RadioSelect();
                    break;

                case "Equip":
                    commandBox.Enabled = false;
                    partyList.Enabled = true;
                    ((RadioButton)partyList.ChildList[0]).RadioSelect();
                    break;

                case "Status":
                    commandBox.Enabled = false;
                    partyList.Enabled = true;
                    ((RadioButton)partyList.ChildList[0]).RadioSelect();
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
                            Script = new string[] { "DisableEnd", "WaitForText", "SelectionPrompt", "No", "Yes", "End" }
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

        public void SelectParty(object parameter)
        {
            HeroModel partyMember;
            if (parameter is IModelProperty)
            {
                partyMember = (HeroModel)((IModelProperty)parameter).GetValue();
            }
            else partyMember = (HeroModel)parameter;

            partyList.Enabled = false;

            switch (commandBox.Selection)
            {
                case 1: ChildViewModel = statusScene.AddView(new SkillViewModel(statusScene, partyMember)); break;
                case 2: ChildViewModel = statusScene.AddView(new EquipViewModel(statusScene, partyMember)); break;
                case 3: ChildViewModel = statusScene.AddView(new StatsViewModel(statusScene, partyMember)); break;
            }
        }

        public ModelCollection<string> AvailableCommands { get; set; } = new ModelCollection<string>();
    }
}
