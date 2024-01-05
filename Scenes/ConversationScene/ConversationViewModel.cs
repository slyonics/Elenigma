using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elenigma.Scenes.ConversationScene
{
    public class ConversationViewModel : ViewModel, ISkippableWait
    {
        private ConversationScene conversationScene;
        private ConversationRecord conversationRecord;
        private DialogueRecord currentDialogue;
        private int dialogueIndex;

        private CrawlText crawlText;

        public bool AutoProceed { get; set; }

        public ModelProperty<bool> CrystalVisible { get; set; } = new ModelProperty<bool>();


        public ConversationViewModel(ConversationScene iScene, ConversationRecord iConversationRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            conversationScene = (parentScene as ConversationScene);
            conversationRecord = iConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Dialogue.Value = currentDialogue.Text;

            if (!string.IsNullOrEmpty(conversationRecord.Bounds))
            {
                string[] tokens = conversationRecord.Bounds.Split(',');
                Window.Value = new Rectangle(ParseInt(tokens[0]), ParseInt(tokens[1]), ParseInt(tokens[2]), ParseInt(tokens[3]));
            }

            LoadView(GameView.ConversationScene_ConversationView);

            crawlText = GetWidget<CrawlText>("ConversationText");
        }

        public ConversationViewModel(ConversationScene iScene, ConversationRecord iConversationRecord, Rectangle conversationBounds, bool autoProceed)
            : base(iScene, PriorityLevel.GameLevel)
        {
            conversationScene = (parentScene as ConversationScene);
            conversationRecord = iConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Dialogue.Value = currentDialogue.Text;
            if (conversationBounds.Width != 0 && conversationBounds.Height != 0) Window.Value = conversationBounds;

            AutoProceed = autoProceed;
            if (autoProceed)
            {
                parentScene.AddController(new SkippableWaitController(PriorityLevel.GameLevel, this, false, 2000));
                OnTerminated += new Action(() => parentScene.EndScene());
                LoadView(GameView.ConversationScene_ConversationView3);
            }
            else LoadView(GameView.ConversationScene_ConversationView2);

            crawlText = GetWidget<CrawlText>("ConversationText");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (conversationScene.PriorityLevel > PriorityLevel.GameLevel) return;

            if (crawlText.ReadyToProceed && !CrystalVisible.Value)
            {
                if (!conversationScene.IsScriptRunning() || (conversationScene.ConversationController == null || !conversationScene.ConversationController.ScriptCommandsLeft))
                {
                    CrystalVisible.Value = true;
                }
            }

            if (crawlText.ReadyToProceed)
            {
                if (!ReadyToProceed.Value && !conversationScene.IsScriptRunning())
                {
                    ReadyToProceed.Value = true;
                }

                OnDialogueScrolled?.Invoke();
                OnDialogueScrolled = null;
            }

            if (!Closed && !ChildList.Any(x => x.Transitioning))
            {
                if (Input.CurrentInput.CommandPressed(Command.Confirm) && !AutoProceed)
                {
                    Proceed();
                }
            }

            if (terminated)
            {
                parentScene.EndScene();
            }
        }

        public void Proceed()
        {
            if (!crawlText.ReadyToProceed)
            {
                crawlText.FinishText();
                conversationScene.FinishDialogue();
            }
            else NextDialogue();
        }

        public override void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget)
        {
            switch (clickWidget.Name)
            {
                case "ConversationText":
                    if (!crawlText.ReadyToProceed)
                    {
                        crawlText.FinishText();
                        conversationScene.FinishDialogue();
                    }
                    else if (!AutoProceed) NextDialogue();
                    break;
            }
        }

        public void NextDialogue()
        {
            dialogueIndex++;

            if (dialogueIndex >= conversationRecord.DialogueRecords.Length)
            {
                if (conversationRecord.EndScript != null)
                {
                    ConversationController conversationController = conversationScene.AddController(new ConversationController(conversationScene, conversationRecord.EndScript));
                    conversationController.OnTerminated += Close;
                }
                else EndConversation();

                return;
            }

            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Dialogue.Value = currentDialogue.Text;
            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;

            ReadyToProceed.Value = false;
            CrystalVisible.Value = false;

            if (currentDialogue.Script != null) conversationScene.RunScript(currentDialogue.Script);

            if (AutoProceed)
            {
                parentScene.AddController(new SkippableWaitController(PriorityLevel.GameLevel, this, false, 2000));
            }
        }

        private void EndConversation()
        {
            if (string.IsNullOrEmpty(conversationRecord.Background))
            {
                Close();
            }
        }

        public void ChangeConversation(ConversationRecord newConversationRecord)
        {
            dialogueIndex = 0;

            conversationRecord = newConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Dialogue.Value = currentDialogue.Text;

            ReadyToProceed.Value = false;
            CrystalVisible.Value = false;

            conversationScene.ConversationController?.Terminate();
            if (currentDialogue.Script != null) conversationScene.RunScript(currentDialogue.Script);
            else conversationScene.ConversationController = null;

        }

        public void Notify(SkippableWaitController sender)
        {
            Proceed();
        }

        public event Action OnDialogueScrolled;

        public ModelProperty<Rectangle> Window { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 20, 240, 61));
        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<GameFont> ConversationFont { get; set; } = new ModelProperty<GameFont>(GameFont.Main);
        public ModelProperty<string> Dialogue { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Speaker { get; set; } = new ModelProperty<string>("");
    }
}
