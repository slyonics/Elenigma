using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects;
using Elenigma.SceneObjects.Maps;
using ldtk;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class MapScene : Scene
    {
        public const float SIGHT_RANGE = 20.0f;

        public static MapScene Instance;

        public Tilemap Tilemap { get; set; }

        public string LocationName { get; private set; }

        public List<Hero> Party { get; private set; } = new List<Hero>();
        public Hero PartyLeader { get => Party.FirstOrDefault(); }
        public CaterpillarController CaterpillarController { get; private set; }

        public List<Npc> NPCs { get; private set; } = new List<Npc>();
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        public List<EventTrigger> EventTriggers { get; private set; } = new List<EventTrigger>();

        private ParallaxBackdrop parallaxBackdrop;

        public bool BattleImminent { get; set; }

        public string BattleBackground { get; private set; }

        public MapScene(GameMap gameMap)
        {
            Instance = this;

            Tilemap = AddEntity(new Tilemap(this, gameMap));

            foreach (FieldInstance field in Tilemap.Level.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Music": if (!string.IsNullOrEmpty(field.Value) && (gameMap != GameMap.Overworld || Audio.CurrentMusic != GameMusic.BeyondtheHills)) Audio.PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), field.Value)); break;
                    case "Script": if (!string.IsNullOrEmpty(field.Value)) AddController(new EventController(this, field.Value.Split('\n'))); break;

                    case "ColorFilter": SceneShader = new SceneObjects.Shaders.ColorFade(Graphics.ParseHexcode("#" + field.Value.Substring(3)), 0.75f); break;
                    case "DayNight": SceneShader = new SceneObjects.Shaders.DayNight(Graphics.ParseHexcode("#" + field.Value.Substring(3)), 1.2f); break;
                    case "HeatDistortion": SceneShader = new SceneObjects.Shaders.HeatDistortion(); break;

                    case "LocationName": if (!string.IsNullOrEmpty(field.Value)) LocationName = field.Value; else LocationName = Tilemap.Name; break;
                    case "Background": if (!string.IsNullOrEmpty(field.Value)) BuildParallaxBackground(field.Value); break;
                    case "BattleBackground": if (!string.IsNullOrEmpty(field.Value)) BattleBackground = field.Value; break;
                }
            }

            Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));
            Tilemap.ClearFieldOfView();

            var leaderHero = new Hero(this, Tilemap, new Vector2(32, 96), GameProfile.PlayerProfile.Party.First().Value);
            Party.Add(leaderHero);
            CaterpillarController = AddController(new CaterpillarController(this));

            foreach (var partymember in GameProfile.PlayerProfile.Party.Skip(1))
            {
                Hero follower = new Hero(this, Tilemap, new Vector2(64, 96), partymember.Value);
                Party.Add(follower);
            }

            foreach (var partymember in Party.Reverse<Hero>())
            {
                AddEntity(partymember);
            }

            var entityLayers = Tilemap.Level.LayerInstances.Where(x => x.Type == "Entities");
            foreach (var entityLayer in entityLayers)
            {
                foreach (EntityInstance entity in entityLayer.EntityInstances)
                {
                    switch (entity.Identifier)
                    {
                        case "Enemy":
                            Enemy enemy = new Enemy(this, Tilemap, entity);
                            if (enemy.IdleScript != null)
                            {
                                EnemyController enemyController = new EnemyController(this, enemy);
                                AddController(enemyController);
                            }
                            Enemies.Add(enemy);
                            AddEntity(enemy);
                            break;

                        case "NPC":
                            {
                                var property = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "DisableIf");
                                if (property != null && property.Value != null && GameProfile.GetSaveData<bool>(property.Value)) continue;

                                Npc npc = new Npc(this, Tilemap, entity);
                                if (npc.Behavior != null)
                                {
                                    NpcController npcController = new NpcController(this, npc);
                                    AddController(npcController);
                                }
                                NPCs.Add(npc);
                                AddEntity(npc);
                                break;
                            }

                        case "Chest":
                            Chest chest = new Chest(this, Tilemap, entity);
                            NPCs.Add(chest);
                            AddEntity(chest);
                            break;

                        case "Interactable":
                        case "Automatic":
                        case "Travel":
                            {
                                var property = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "EnableIf");
                                if (property != null && property.Value != null && !GameProfile.GetSaveData<bool>(property.Value)) continue;

                                property = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "DisableIf");
                                if (property != null && property.Value != null && GameProfile.GetSaveData<bool>(property.Value)) continue;


                                EventTriggers.Add(new EventTrigger(this, entity));
                                break;
                            }
                    }
                }
            }
        }

        public MapScene(GameMap gameMap, int startX, int startY, Orientation orientation)
            : this(gameMap)
        {
            PartyLeader.CenterOn(Tilemap.GetTile(startX, startY).Center);
            PartyLeader.Orientation = orientation;
            Tilemap.GetTile(PartyLeader.Center).Occupants.Add(PartyLeader);
            PartyLeader.HostTile = Tilemap.GetTile(PartyLeader.Center);
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(Tilemap.GetTile(startX, startY).Center);
                hero.Orientation = orientation;
                hero.Idle();

                i++;
            }

            Camera.Center(PartyLeader.Center);

            foreach (Hero hero in Party) Tilemap.CalculateFieldOfView(Tilemap.GetTile(hero.Center), SIGHT_RANGE);
            Tilemap.UpdateVisibility();
        }

        public MapScene(GameMap gameMap, int startX, int startY)
            : this(gameMap, startX, startY, Orientation.Right)
        {
            if (Party.Count == 2)
            {
                CaterpillarController.Move(Orientation.Right);
                Update(new GameTime(new TimeSpan(0, 0, 2), new TimeSpan(0, 0, 2)));
                Party[0].Orientation = Orientation.Left;
                Party[0].Idle();
                Party[1].Orientation = Orientation.Right;
                Party[1].Idle();
            }
            else
            {
                Party[0].Hide = true;

                var airship = new Airship(this, Tilemap, Party[0].Position, Orientation.Left);
                airship.Idle();
                airship.SetTargetAltitude(0);
                airship.LandAction = new Action(() =>
                {
                    Audio.PlayMusic(GameMusic.NewDestinations);
                    Party[0].Hide = false;
                    CaterpillarController.Move(Orientation.Left, true);
                    CaterpillarController.FinishMovement = new ScriptParser.UnblockFollowup(() =>
                    {
                        Party[0].Orientation = Orientation.Right;
                        Party[0].Idle();
                        var pilot = new Npc(this, Tilemap, 95, 17, "Pilot", Orientation.Left);
                        AddEntity(pilot);
                        ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene("AirshipOutro");
                        CrossPlatformGame.StackScene(conversationScene);
                    });
                });
                AddEntity(airship);
            }
        }

        public MapScene(GameMap gameMap, Vector2 leaderPosition)
            : this(gameMap)
        {
            PartyLeader.Position = leaderPosition;
            PartyLeader.Orientation = Orientation.Down;
            Tilemap.GetTile(PartyLeader.Center).Occupants.Add(PartyLeader);
            PartyLeader.HostTile = Tilemap.GetTile(PartyLeader.Center);
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();
            Camera.Center(PartyLeader.Center);

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(new Vector2(PartyLeader.SpriteBounds.Left + i * 6, PartyLeader.SpriteBounds.Bottom - 12 + (i % 2) * 6));
                hero.Orientation = Orientation.Down;
                hero.Idle();

                i++;
            }
        }

        public MapScene(string gameMap, string sourceMapName)
            : this((GameMap)Enum.Parse(typeof(GameMap), gameMap))
        {
            var spawnZone = EventTriggers.First(x => x.Name == sourceMapName);

            Orientation orientation = spawnZone.Direction;

            Vector2 spawnPosition = new Vector2(spawnZone.Bounds.Center.X, spawnZone.Bounds.Center.Y);
            switch (orientation)
            {
                case Orientation.Up: spawnPosition.Y -= Tilemap.TileSize; break;
                case Orientation.Right: spawnPosition.X += Tilemap.TileSize; break;
                case Orientation.Down: spawnPosition.Y += Tilemap.TileSize; break;
                case Orientation.Left: spawnPosition.X -= Tilemap.TileSize; break;
            }
            PartyLeader.CenterOn(spawnPosition);
            PartyLeader.Orientation = orientation;
            Tilemap.GetTile(PartyLeader.Center).Occupants.Add(PartyLeader);
            PartyLeader.HostTile = Tilemap.GetTile(PartyLeader.Center);
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();
            Camera.Center(PartyLeader.Center);

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(Tilemap.GetTile(PartyLeader.Center).Center);
                hero.Orientation = orientation;
                hero.Idle();

                i++;
            }

            foreach (Hero hero in Party) Tilemap.CalculateFieldOfView(Tilemap.GetTile(hero.Center), SIGHT_RANGE);
            Tilemap.UpdateVisibility();
        }

        public void SaveMapPosition()
        {
            GameProfile.SetSaveData<string>("LastMapName", Tilemap.Name);
            GameProfile.SetSaveData<int>("LastPositionX", (int)PartyLeader.HostTile.TileX);
            GameProfile.SetSaveData<int>("LastPositionY", (int)PartyLeader.HostTile.TileY);
            GameProfile.SetSaveData<string>("PlayerLocation", Tilemap.Level.FieldInstances.First(x => x.Identifier == "LocationName").Value);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Camera.Center(PartyLeader.Center);

            NPCs.RemoveAll(x => x.Terminated);
            Enemies.RemoveAll(x => x.Terminated);

            parallaxBackdrop?.Update(gameTime, Camera);
        }

        public bool ProcessAutoEvents()
        {
            bool eventTriggered = false;
            foreach (EventTrigger eventTrigger in EventTriggers)
            {
                if (eventTrigger.Bounds.Intersects(PartyLeader.Bounds) && !eventTrigger.Interactive)
                {
                    eventTriggered = true;
                    eventTrigger.Terminated = true;
                    EventTrigger.LastTrigger = eventTrigger;
                    AddController(new EventController(this, eventTrigger.Script));
                }
            }
            EventTriggers.RemoveAll(x => x.Terminated);

            return eventTriggered;
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            parallaxBackdrop?.Draw(spriteBatch);

            Tilemap.DrawBackground(spriteBatch, Camera);
        }

        private void BuildParallaxBackground(string background)
        {
            string[] tokens = background.Split(' ');

            parallaxBackdrop = new ParallaxBackdrop(tokens[0], tokens.Skip(1).Select(x => float.Parse(x)).ToArray());
        }

        public override void DrawGame(SpriteBatch spriteBatch, Effect shader, Matrix matrix)
        {
            base.DrawGame(spriteBatch, shader, matrix);

            // TODO: disable rendering when battle scene active, unless it's transitioning out
        }

        public void HandleOffscreen()
        {
            var travelZone = EventTriggers.Where(x => x.TravelZone && x.DefaultTravelZone).OrderBy(x => Vector2.Distance(new Vector2(x.Bounds.Center.X, x.Bounds.Center.Y), PartyLeader.Position)).First();
            travelZone.Activate(PartyLeader);
        }

        public void RemoveNearbyEnemies()
        {
            foreach (Hero hero in Party)
            {
                Tile hostTile = Tilemap.GetTile(hero.Center);

                foreach (Actor occupant in hostTile.Occupants) if (occupant is Enemy) occupant.Terminate();
                hostTile.Occupants.RemoveAll(x => x.Terminated);

                foreach (Tile neighbor in hostTile.NeighborList)
                {
                    foreach (Actor occupant in neighbor.Occupants) if (occupant is Enemy) occupant.Terminate();
                    neighbor.Occupants.RemoveAll(x => x.Terminated);
                }
            }
        }

        public void SpawnMonster(int x, int y, string sprite, string encounter, Orientation orientation = Orientation.Down)
        {
            Enemy enemy = new Enemy(this, Tilemap, x, y, sprite, encounter, orientation);
            if (enemy.IdleScript != null)
            {
                EnemyController enemyController = new EnemyController(this, enemy);
                AddController(enemyController);
            }
            Enemies.Add(enemy);
            AddEntity(enemy);
        }
    }
}
