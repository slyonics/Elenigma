using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Maps;
using Elenigma.SceneObjects.Shaders;
using Elenigma.Scenes.BattleScene;
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
                case "Conversation": Conversation(tokens, scriptParser); break;
                case "Encounter": Encounter(tokens, mapScene, scriptParser, this); return true;
                case "Shop": Shop(tokens); break;
                case "Pawn": Pawn(tokens); break;
                case "GiveItem": GiveItem(tokens); break;
                case "Inn": Inn(tokens); break;
                case "RestoreParty": RestoreParty(); break;
                case "MoveParty": Move(tokens, mapScene); break;
                case "TurnParty": Turn(tokens, mapScene); break;
                case "WaitParty": mapScene.CaterpillarController.FinishMovement = scriptParser.BlockScript(); return true;
                case "Idle": Idle(tokens); break;
                case "Animate": mapScene.Party[int.Parse(tokens[1])].PlayAnimation(tokens[2]); break;
                case "SpawnMonster": SpawnMonster(tokens, mapScene); break;
                case "RecruitEnvi": RecruitEnvi(); break;
                case "ResetTrigger": EventTrigger.LastTrigger.Terminated = false; mapScene.EventTriggers.Add(EventTrigger.LastTrigger); break;
                case "ChangeClass": ChangeClass(tokens); break;
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

        public static void SpawnMonster(string[] tokens, MapScene mapScene)
        {
            mapScene.SpawnMonster(int.Parse(tokens[1]), int.Parse(tokens[2]), tokens[4], tokens[5], (Orientation)Enum.Parse(typeof(Orientation), tokens[3]));
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

        public static void Encounter(string[] tokens, MapScene mapScene, ScriptParser scriptParser, Controller controller)
        {
            mapScene.BattleImminent = true;

            BattleScene.BattleScene battleScene = null;

            var unblocker = scriptParser.BlockScript();

            var terrain = mapScene.Tilemap.GetTile(mapScene.PartyLeader.Center).TerrainType;
            if (terrain == TerrainType.None) battleScene = new BattleScene.BattleScene(tokens[1], mapScene.BattleBackground);
            else battleScene = new BattleScene.BattleScene(tokens[1], mapScene.Tilemap.GetTile(mapScene.PartyLeader.Center).TerrainType);
            battleScene.OnTerminated += new TerminationFollowup(() =>
            {
                mapScene.BattleImminent = false;
                unblocker();

                if (!GameProfile.PlayerProfile.Party.Any(x => x.Value.HP.Value > 0))
                {
                    controller.OnTerminated += new TerminationFollowup(() =>
                    {
                        if (!GameProfile.GetSaveData<bool>("IntroComplete"))
                        {
                            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene("FoolOutro", new Rectangle(), true);
                            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                            CrossPlatformGame.StackScene(conversationScene);
                        }
                        else
                        {
                            Audio.StopMusic();
                            foreach (Hero hero in mapScene.Party) hero.PlayAnimation("Faint");
                            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
                        }
                    });
                }
            });

            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            Pinwheel pinwheel = new Pinwheel(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => pinwheel.Amount = t);

            int delay = 700;
            if (tokens.Length > 2) delay = int.Parse(tokens[2]);

            Task.Delay(delay).ContinueWith(t =>
            {
                Audio.PlaySound(GameSound.BattleStart);
                mapScene.AddController(transitionController);
                CrossPlatformGame.TransitionShader = pinwheel;
            });

            transitionController.OnTerminated += new TerminationFollowup(() =>
            {
                mapScene.RemoveNearbyEnemies();
                pinwheel.Terminate();
                CrossPlatformGame.StackScene(battleScene);
            });
        }

        public void Shop(string[] tokens)
        {
            ShopScene.ShopScene shopScene = new ShopScene.ShopScene(tokens[1]);
            shopScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(shopScene);
        }

        public void Pawn(string[] tokens)
        {
            ShopScene.ShopScene shopScene = new ShopScene.ShopScene();
            shopScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(shopScene);
        }

        public void GiveItem(string[] tokens)
        {
            ItemRecord item = new ItemRecord(ItemRecord.ITEMS.First(x => x.Name == string.Join(' ', tokens.Skip(1))));

            if (item.ItemType == ItemType.Medicine || item.ItemType == ItemType.Consumable) GameProfile.AddInventory(item.Name, 1);
            else GameProfile.AddInventory(item.Name, -1);

            ConversationRecord conversationData = new ConversationRecord()
            {
                DialogueRecords = new DialogueRecord[]
                {
                    new DialogueRecord() { Text = "Found @" + item.Icon + " " + item.Name + "!"}
                }
            };

            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(conversationData);
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(conversationScene);

            Audio.PlaySound(GameSound.GetItem);
        }

        public void Inn(string[] tokens)
        {
            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene("Inn");
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());

            TransitionController transitionOutController = new TransitionController(TransitionDirection.Out, 600);
            ColorFade colorFadeOut = new SceneObjects.Shaders.ColorFade(Color.Black, transitionOutController.TransitionProgress);
            transitionOutController.UpdateTransition += new Action<float>(t => colorFadeOut.Amount = t);
            transitionOutController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                Audio.PauseMusic(true);
                Audio.PlaySound(GameSound.Rest);
                Task.Delay(1500).ContinueWith(t => Audio.PauseMusic(false));

                transitionOutController.Terminate();
                colorFadeOut.Terminate();
                TransitionController transitionInController = new TransitionController(TransitionDirection.In, 600);
                ColorFade colorFadeIn = new SceneObjects.Shaders.ColorFade(Color.Black, transitionInController.TransitionProgress);
                transitionInController.UpdateTransition += new Action<float>(t => colorFadeIn.Amount = t);
                transitionInController.FinishTransition += new Action<TransitionDirection>(t =>
                {
                    colorFadeIn.Terminate();
                });
                mapScene.AddController(transitionInController);
                mapScene.SceneShader = colorFadeIn;

                CrossPlatformGame.StackScene(conversationScene);
            });

            mapScene.AddController(transitionOutController);
            mapScene.SceneShader = colorFadeOut;

            RestoreParty();
        }

        public void RestoreParty()
        {
            foreach (var partyMember in GameProfile.PlayerProfile.Party)
            {
                partyMember.Value.HP.Value = partyMember.Value.MaxHP.Value;
                partyMember.Value.MP.Value = partyMember.Value.MaxMP.Value;
                partyMember.Value.StatusAilments.RemoveAll();
            }
        }

        public void RecruitEnvi()
        {
            GameProfile.AddInventory("Dart", 5);
            HeroModel envi = new HeroModel(HeroType.Envi, ClassType.Warrior, 3);
            envi.Equip("Oak Club");
            envi.Equip("Bracers");
            envi.Equip("Hide Armor");
            GameProfile.PlayerProfile.Party.ModelList.Insert(0, new ModelProperty<HeroModel>(envi));

            Npc enviNpc = mapScene.EntityList.First(x => x is Npc && ((Npc)x).Label == "Large Woman") as Npc;
            Vector2 heroPosition = enviNpc.Position;
            mapScene.NPCs.Remove(enviNpc);
            mapScene.EntityList.Remove(enviNpc);
            enviNpc.HostTile.Occupants.Remove(enviNpc);

            Hero hero = new Hero(mapScene, mapScene.Tilemap, heroPosition, envi);
            mapScene.AddEntity(hero);
            mapScene.Party.Insert(0, hero);
        }

        public static void Move(string[] tokens, MapScene mapScene)
        {
            mapScene.CaterpillarController.Move((Orientation)Enum.Parse(typeof(Orientation), tokens[1]), true);
            foreach (Hero hero in mapScene.Party) hero.PriorityLevel = PriorityLevel.CutsceneLevel;
        }

        public static void Turn(string[] tokens, MapScene mapScene)
        {
            Hero hero = mapScene.Party[int.Parse(tokens[1])];
            hero.Orientation = (Orientation)Enum.Parse(typeof(Orientation), tokens[2]);
            hero.OrientedAnimation("Idle");
        }

        public void Idle(string[] tokens)
        {
            mapScene.CaterpillarController.Idle();
        }

        public void ChangeClass(string[] tokens)
        {
            var dialogueRecords = new List<ConversationScene.DialogueRecord>();

            dialogueRecords.Add(new ConversationScene.DialogueRecord()
            {
                Text = $"Do you want to change {tokens[1]} to a different character class?",
                Script = new string[] { "DisableEnd", "WaitForText", "SelectionPrompt", "Fool", "Warrior", "Hunter", "Scholar", "End" }
            });

            var convoRecord = new ConversationScene.ConversationRecord()
            {
                DialogueRecords = dialogueRecords.ToArray()
            };
            var convoScene = new ConversationScene.ConversationScene(convoRecord);
            CrossPlatformGame.StackScene(convoScene, true);
            convoScene.OnTerminated += new TerminationFollowup(() =>
            {
                HeroModel heroModel = GameProfile.PlayerProfile.Party.First(x => x.Value.Name.Value == tokens[1]).Value;
                heroModel.ChangeClass((ClassType)Enum.Parse(typeof(ClassType), GameProfile.GetSaveData<string>("LastSelection")));
                var hero = mapScene.Party.First(x => x.HeroModel == heroModel);
                hero.UpdateSprite();
            });
        }
    }
}
