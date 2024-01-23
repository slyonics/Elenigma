using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Maps;
using Elenigma.SceneObjects.Particles;
using Elenigma.SceneObjects.Shaders;
using Elenigma.Scenes.ConversationScene;
using FMOD;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class EventController : ScriptController
    {
        private MapScene mapScene;

        public bool EndGame { get; private set; }
        public Actor ActorSubject { get; set; }

        public EventController(MapScene iScene, string[] script)
            : base(iScene, script, PriorityLevel.CutsceneLevel)
        {
            mapScene = iScene;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "EndGame": EndGame = true; break;
                case "ChangeMap": ChangeMap(tokens, mapScene); break;
                case "SetWaypoint": SetWaypoint(tokens); break;
                case "Conversation": Conversation(tokens, scriptParser); mapScene.PartyLeader.Idle(); break;
                case "Animate": mapScene.Party[int.Parse(tokens[1])].PlayAnimation(tokens[2]); break;

                case "ResetTrigger": EventTrigger.LastTrigger.Terminated = false; mapScene.EventTriggers.Add(EventTrigger.LastTrigger); break;

                case "LearnSummon": GameProfile.PlayerProfile.AvailableSummons.Add((SummonType)Enum.Parse(typeof(SummonType), tokens[1])); break;
                case "MovePlayerTo": mapScene.PlayerController.MoveTo(int.Parse(tokens[1]), int.Parse(tokens[2])); break;
                case "MoveNpcTo": MoveNpcTo(tokens); break;
                case "WaitPlayer": mapScene.PlayerController.ChildController.OnTerminated += new TerminationFollowup(() => scriptParser.BlockScript()); break;
                case "ChangeSprite": ChangeSprite(tokens); break;
                case "RemoveNpc": mapScene.NPCs.First(x => x.Name == tokens[1]).Terminate(); break;
                case "Recruit": Recruit(tokens); break;

                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            if (parameter.StartsWith("$saveData."))
            {
                return GameProfile.GetSaveData<string>(parameter.Split('.')[1]).ToString();
            }
            else if (parameter[0] == '$')
            {
                switch (parameter)
                {

                    default: return null;
                }
            }
            else return null;
        }

        public static void ChangeMap(string[] tokens, MapScene mapScene)
        {
            if (tokens.Length == 5) CrossPlatformGame.Transition(typeof(MapScene), (GameMap)Enum.Parse(typeof(GameMap), tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]), (Orientation)Enum.Parse(typeof(Orientation), tokens[4]));
            else if (tokens.Length == 4) CrossPlatformGame.Transition(typeof(MapScene), (GameMap)Enum.Parse(typeof(GameMap), tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]));
            else if (tokens.Length == 2) CrossPlatformGame.Transition(typeof(MapScene), tokens[1], mapScene.Tilemap.Name);
        }

        public static void SetWaypoint(string[] tokens)
        {
            /*
            mapScene.SetWaypoint(int.Parse(tokens[1]), int.Parse(tokens[2]));
            */
        }

        public static void Conversation(string[] tokens, ScriptParser scriptParser)
        {
            if (tokens.Length == 2)
            {
                ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(tokens[1]);
                conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                CrossPlatformGame.StackScene(conversationScene);
            }
            else
            {
                ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(tokens[1], new Rectangle(), true);
                conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                CrossPlatformGame.StackScene(conversationScene);
            }
        }

        public void MoveNpcTo(string[] tokens)
        {
            var npc = mapScene.NPCs.First(x => x.Name == tokens[1]);
            var destination = mapScene.Tilemap.GetTile(int.Parse(tokens[2]), int.Parse(tokens[3])).Center;
            mapScene.AddController(new PathingController(PriorityLevel.CutsceneLevel, mapScene.Tilemap, npc, destination, PlayerController.RUN_SPEED));
        }

        public void ChangeSprite(string[] tokens)
        {
            var npc = mapScene.NPCs.First(x => x.Name == tokens[1]);
            npc.AnimatedSprite.SpriteTexture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + tokens[2])];
        }

        public void Recruit(string[] tokens)
        {
            if (tokens[1] == "Keeva")
            {
                var npc = mapScene.NPCs.First(x => x.Name == "Familiar");
                Hero follower = mapScene.AddEntity(new Hero(mapScene, mapScene.Tilemap, npc.Position, GameSprite.Actors_DogFamiliar, npc.Orientation));
                mapScene.AddController(new FollowerController(mapScene, follower, mapScene.PartyLeader));
                mapScene.AddParticle(new AnimationParticle(mapScene, follower.Position + new Vector2(2, 0), AnimationType.Smoke, true));
            }
        }
    }
}
