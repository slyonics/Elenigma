using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects;
using Elenigma.SceneObjects.Controllers;
using Elenigma.Scenes.MapScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.ConversationScene
{
    public class ConversationController : ScriptController
    {
        private ConversationScene conversationScene;

        public bool EndGame { get; private set; }

        public ConversationController(ConversationScene iScene, string script)
            : base(iScene, script, PriorityLevel.GameLevel)
        {
            conversationScene = iScene;
        }

        public ConversationController(ConversationScene iScene, string[] script)
            : base(iScene, script, PriorityLevel.GameLevel)
        {
            conversationScene = iScene;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (!scriptParser.Finished) scriptParser.Update(gameTime);
        }

        public bool ScriptCommandsLeft
        {
            get
            {
                return scriptParser.ScriptCommands != null && scriptParser.ScriptCommands.Count > 0;
            }
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "AddPortrait": AddPortrait(tokens); break;
                case "RemovePortrait": RemovePortrait(tokens); break;
                case "PortraitSprite": PortraitSprite(tokens); break;
                case "PortraitPosition": PortraitPosition(tokens); break;
                case "PortraitColor": PortraitColor(tokens); break;
                case "PortraitScale": PortraitScale(tokens); break;
                case "PortraitVelocity": PortraitVelocity(tokens); break;
                case "GameEvent": GameEvent(tokens); break;
                case "EndGame": EndGame = true; break;
                case "WaitForText": WaitForText(tokens); return false;
                case "ProceedText": conversationScene.ConversationViewModel.NextDialogue(); break;
                case "Actor": Actor(tokens); break;
                case "Animate": Animate(tokens); break;
                case "SelectionPrompt": SelectionPrompt(tokens); return false;
                case "ChangeConversation": ChangeConversation(tokens); break;
                case "EndConversation": conversationScene.ConversationViewModel.Close(); break;
                case "SetAutoProceed": conversationScene.ConversationViewModel.AutoProceedLength = int.Parse(tokens[1]); break;
                case "RemoveBackground": conversationScene.BackgroundSprite = null; break;

                case "ChangeMap": MapScene.EventController.ChangeMap(tokens, MapScene.MapScene.Instance); break;
                case "AnimateHero": MapScene.MapScene.Instance.Party[int.Parse(tokens[1])].PlayAnimation(tokens[2], new AnimationFollowup(() => { })); break;
                case "ResetTrigger": EventTrigger.LastTrigger.Terminated = false; MapScene.MapScene.Instance.EventTriggers.Add(EventTrigger.LastTrigger); break;
                case "GiveItem": GiveItem(tokens); break;

                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            switch (parameter)
            {
                case "$leftPortraitX": return (((int)(CrossPlatformGame.ScreenWidth / 1.6) - 60) / 2).ToString();
                case "$rightPortraitX": return (CrossPlatformGame.ScreenWidth - ((int)(CrossPlatformGame.ScreenWidth / 1.6) - 60) / 2).ToString();
                case "$portraitY": return ((int)(CrossPlatformGame.ScreenHeight)).ToString();
                case "$portraitScaleX": return (CrossPlatformGame.ScreenWidth / 1920.0f / 1.6f).ToString();
                case "$portraitScaleY": return (CrossPlatformGame.ScreenHeight / 1080.0f / 1.6f).ToString();
                default: return base.ParseParameter(parameter);
            }
        }

        private void AddPortrait(string[] tokens)
        {
            if (tokens.Length > 8)
            {
                string name = tokens[1];
                string sprite = tokens[2];
                int positionX = int.Parse(tokens[3]);
                int positionY = int.Parse(tokens[4]);
                int cellWidth = int.Parse(tokens[5]);
                int cellHeight = int.Parse(tokens[6]);
                int cellCount = int.Parse(tokens[7]);
                int frameLength = int.Parse(tokens[8]);
                float transitionLength = (tokens.Length > 9) ? float.Parse(tokens[9]) : 1.0f;

                Portrait portrait = new Portrait(conversationScene, name, sprite, new Vector2(positionX, positionY), cellWidth, cellHeight, cellCount, frameLength, transitionLength);
                conversationScene.AddPortrait(portrait);
            }
            else if (tokens.Length > 6)
            {
                string name = tokens[1];
                string sprite = tokens[2];
                int startX = int.Parse(tokens[3]);
                int startY = int.Parse(tokens[4]);
                int endX = int.Parse(tokens[5]);
                int endY = int.Parse(tokens[6]);
                float transitionLength = (tokens.Length > 7) ? float.Parse(tokens[7]) : 1.0f;

                Portrait portrait = new Portrait(conversationScene, name, sprite, new Vector2(startX, startY), new Vector2(endX, endY), transitionLength);
                conversationScene.AddPortrait(portrait);
            }
            else
            {
                string name = tokens[1];
                string sprite = tokens[2];
                int positionX = int.Parse(tokens[3]);
                int positionY = int.Parse(tokens[4]);
                float transitionLength = (tokens.Length > 5) ? float.Parse(tokens[5]) : 1.0f;

                Portrait portrait = new Portrait(conversationScene, name, sprite, new Vector2(positionX, positionY), transitionLength);
                conversationScene.AddPortrait(portrait);
            }
        }

        private void RemovePortrait(string[] tokens)
        {
            if (tokens.Length > 3)
            {
                string name = tokens[1];
                int endX = int.Parse(tokens[2]);
                int endY = int.Parse(tokens[3]);
                float transitionLength = (tokens.Length > 4) ? float.Parse(tokens[4]) : 1.0f;

                Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
                portrait.Remove(new Vector2(endX, endY), transitionLength);
            }
            else
            {
                string name = tokens[1];
                float transitionLength = (tokens.Length > 2) ? float.Parse(tokens[2]) : 1.0f;

                Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
                portrait?.Remove(transitionLength);
            }
        }

        private void PortraitSprite(string[] tokens)
        {
            string name = tokens[1];
            string sprite = tokens[2];
            float transitionLength = (tokens.Length > 3) ? float.Parse(tokens[3]) : 1.0f;

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
            portrait.SetSprite(sprite, transitionLength);
        }

        private void PortraitPosition(string[] tokens)
        {
            string name = tokens[1];
            int endX = int.Parse(tokens[2]);
            int endY = int.Parse(tokens[3]);
            float transitionLength = (tokens.Length > 4) ? float.Parse(tokens[4]) : 1.0f;

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
            portrait.SetPosition(new Vector2(endX, endY), transitionLength);
        }

        private void PortraitColor(string[] tokens)
        {
            string name = tokens[1];
            int colorR = int.Parse(tokens[2]);
            int colorG = int.Parse(tokens[3]);
            int colorB = int.Parse(tokens[4]);
            int colorA = int.Parse(tokens[5]);
            float transitionLength = (tokens.Length > 6) ? float.Parse(tokens[6]) : 1.0f;

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
            portrait.SetColor(new Color(colorR, colorG, colorB, colorA), transitionLength);
        }

        public void PortraitScale(string[] tokens)
        {
            string name = tokens[1];
            float scaleX = float.Parse(tokens[2]);
            float scaleY = float.Parse(tokens[3]);

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
            portrait.AnimatedSprite.Scale = new Vector2(scaleX, scaleY);
        }

        public void PortraitVelocity(string[] tokens)
        {
            string name = tokens[1];
            float scaleX = float.Parse(tokens[2]);
            float scaleY = float.Parse(tokens[3]);

            Portrait portrait = conversationScene.Portraits.Find(x => x.Name == name);
            portrait.Velocity = new Vector2(scaleX, scaleY);
        }

        private void GameEvent(string[] tokens)
        {
            switch (tokens[1])
            {

            }
        }

        private void WaitForText(string[] tokens)
        {
            ScriptParser.UnblockFollowup followup = scriptParser.BlockScript();
            conversationScene.ConversationViewModel.OnDialogueScrolled += new Action(followup);
        }

        private void Actor(string[] tokens)
        {
            //CrossPlatformGame.GetScene<MapScene.MapScene>().MapViewModel.SetActor(tokens[1]);
        }

        private void Animate(string[] tokens)
        {
            //CrossPlatformGame.GetScene<MapScene.MapScene>().MapViewModel.AnimateActor(tokens[1]);
        }

        private void SelectionPrompt(string[] tokens)
        {
            List<string> options = new List<string>();
            string skipLine;
            do
            {
                skipLine = scriptParser.DequeueNextCommand();
                options.Add(skipLine);
            } while (skipLine != "End");
            options.RemoveAt(options.Count - 1);

            SelectionViewModel selectionViewModel = new SelectionViewModel(conversationScene, options);
            conversationScene.AddOverlay(selectionViewModel);

            ScriptParser.UnblockFollowup followup = scriptParser.BlockScript();
            selectionViewModel.OnTerminated += new Action(followup);
        }

        private void ChangeConversation(string[] tokens)
        {
            var conversationData = ConversationScene.CONVERSATIONS.FirstOrDefault(x => x.Name == tokens[1]);

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) scriptParser.RunScript(conversationData.DialogueRecords[0].Script);

            conversationScene.ConversationViewModel.ChangeConversation(conversationData);
        }

        public void GiveItem(string[] tokens)
        {
            var name = string.Join(' ', tokens.Skip(1));

            GameProfile.AddInventory(name, -1);
        }
    }
}
