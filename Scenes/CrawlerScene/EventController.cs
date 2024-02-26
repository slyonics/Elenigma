using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.CrawlerScene
{
    public class EventController : ScriptController
    {
        private CrawlerScene mapScene;

        public bool EndGame { get; private set; }

        MapRoom mapRoom = null;


        public EventController(CrawlerScene iScene, string[] script, MapRoom iMapRoom)
            : base(iScene, script, PriorityLevel.CutsceneLevel)
        {
            mapScene = iScene;
            mapRoom = iMapRoom;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Actor": Actor(tokens); break;
                case "Animate": Animate(tokens); break;
                case "GameEvent": GameEvent(tokens); break;
                case "EndGame": EndGame = true; break;
                case "ChangeMap": ChangeMap(tokens); Audio.PlaySound(GameSound.Door); break;
                case "DisableEvent": mapRoom.Script = null; break;
                case "SetWaypoint": SetWaypoint(tokens); break;
                case "Conversation": Conversation(tokens); break;
                case "Encounter": Encounter(tokens, scriptParser); break;
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            if (parameter.Contains("Flag."))
            {
                return GameProfile.GetSaveData<bool>(parameter.Split('.')[1]).ToString();
            }
            else return base.ParseParameter(parameter);
        }

        private void Actor(string[] tokens)
        {
            mapScene.MapViewModel.SetActor(tokens[1]);
        }

        private void Animate(string[] tokens)
        {
            mapScene.MapViewModel.AnimateActor(tokens[1]);
        }

        private void GameEvent(string[] tokens)
        {
            switch (tokens[1])
            {

            }
        }

        private void ChangeMap(string[] tokens)
        {
            mapScene.ResetWaypoints();

            Type sceneType = Type.GetType(tokens[1]);
            if (tokens.Length == 6) CrossPlatformGame.Transition(sceneType, tokens[2], int.Parse(tokens[3]), int.Parse(tokens[4]), (Direction)Enum.Parse(typeof(Direction), tokens[5]));
            else if (tokens.Length == 3) CrossPlatformGame.Transition(typeof(CrawlerScene), tokens[1], tokens[2]);
            else if (tokens.Length == 2) CrossPlatformGame.Transition(sceneType);
            else CrossPlatformGame.Transition(sceneType, tokens[2]);
        }

        public static void SetWaypoint(string[] tokens)
        {
            GameProfile.SetSaveData<string>("Waypoint", tokens[1]);

            CrawlerScene.Instance.ApplyWaypoint(tokens[1]);

        }

        private void Conversation(string[] tokens)
        {
            if (tokens.Length == 2)
            {


                ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(tokens[1]);
                conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                CrossPlatformGame.StackScene(conversationScene);

            }
            else
            {
                var convoRecord = new ConversationScene.ConversationRecord()
                {
                    DialogueRecords = new ConversationScene.DialogueRecord[] { new ConversationScene.DialogueRecord() { Text = String.Join(' ', tokens.Skip(1)) } }
                };

                var convoScene = new ConversationScene.ConversationScene(convoRecord);
                convoScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                CrossPlatformGame.StackScene(convoScene);
            }
        }

        private void Encounter(string[] tokens)
        {
            /*
            mapScene.MapViewModel.SetActor("Actors_" + tokens[1]);

            MatchScene.MatchScene matchScene;
            if (tokens.Length > 2) matchScene = new MatchScene.MatchScene(tokens[1], tokens[2]);
            else matchScene = new MatchScene.MatchScene(tokens[1]);
            var unblock = scriptParser.BlockScript();
            matchScene.OnTerminated += new TerminationFollowup(unblock);
            CrossPlatformGame.StackScene(matchScene);
            */
        }

        public static void Encounter(string[] tokens, ScriptParser scriptParser)
        {
            /*
            BattleScene.BattleScene battleScene = new BattleScene.BattleScene(tokens[1]);
            battleScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(battleScene, true);
            */
        }
    }
}
