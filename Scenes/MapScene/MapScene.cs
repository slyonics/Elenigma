using Elenigma.GameObjects.Maps;
using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects;
using Elenigma.SceneObjects.Maps;
using Elenigma.SceneObjects.Particles;
using Elenigma.SceneObjects.Shaders;
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
        public PlayerController PlayerController { get; private set; }

        public List<Npc> NPCs { get; private set; } = new List<Npc>();
        public List<EventTrigger> EventTriggers { get; private set; } = new List<EventTrigger>();

        private ParallaxBackdrop parallaxBackdrop;


        public bool Indoors { get; private set; }
        public WeatherController WeatherController { get; set; }


        public MapScene(GameMap gameMap)
        {
            Instance = this;

            Tilemap = AddEntity(new Tilemap(this, gameMap));

            Color mapColor = Color.White;

            foreach (FieldInstance field in Tilemap.Level.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Music": if (!string.IsNullOrEmpty(field.Value) && (gameMap != GameMap.Overworld || Audio.CurrentMusic != GameMusic.BeyondtheHills)) Audio.PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), field.Value)); break;
                    case "Script": if (!string.IsNullOrEmpty(field.Value)) AddController(new EventController(this, field.Value.Split('\n'))); break;

                    case "ColorFilter": SceneShader = new SceneObjects.Shaders.ColorFade(Graphics.ParseHexcode("#" + field.Value.Substring(1)), 0.75f); break;
                    case "DayNight": SceneShader = new SceneObjects.Shaders.DayNight(Graphics.ParseHexcode("#" + field.Value.Substring(1)), 1.2f); break;
                    case "HeatDistortion": SceneShader = new SceneObjects.Shaders.HeatDistortion(); break;

                    case "LocationName": if (!string.IsNullOrEmpty(field.Value)) LocationName = field.Value; else LocationName = Tilemap.Name; break;
                    case "Background": if (!string.IsNullOrEmpty(field.Value)) BuildParallaxBackground(field.Value); break;
                }
            }

            Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));

            var leaderHero = new Hero(this, Tilemap, new Vector2(32, 96), GameSprite.Actors_AdultMC);
            leaderHero.FootstepSound = GameSound.footsteps_grass_1;
            AddEntity(leaderHero);
            Party.Add(leaderHero);
            PlayerController = AddController(new PlayerController(this, leaderHero));

            var entityLayers = Tilemap.Level.LayerInstances.Where(x => x.Type == "Entities");
            foreach (var entityLayer in entityLayers)
            {
                foreach (EntityInstance entity in entityLayer.EntityInstances)
                {
                    switch (entity.Identifier)
                    {
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

            var dayNight = SceneShader as DayNight;
            if (dayNight != null)
            {
                WeatherController = new WeatherController(dayNight, GameProfile.WorldTime, 2, Indoors);
                AddController(WeatherController);
                WeatherController.PreUpdate(new GameTime());

                if (!WeatherController.Indoors) dayNight.Ambient = WeatherController.AmbientLight.ToVector4();
                else WeatherController.AmbientLight = mapColor;
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
        }

        public void SaveMapPosition()
        {
            GameProfile.SetSaveData<string>("LastMapName", Tilemap.Name);
            GameProfile.SetSaveData<Vector2>("LastPosition", PartyLeader.Position);
            GameProfile.SetSaveData<string>("PlayerLocation", Tilemap.Level.FieldInstances.First(x => x.Identifier == "LocationName").Value);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Camera.Center(PartyLeader.Center);

            NPCs.RemoveAll(x => x.Terminated);

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
        }

        public void HandleOffscreen()
        {
            var travelZone = EventTriggers.Where(x => x.TravelZone && x.DefaultTravelZone).OrderBy(x => Vector2.Distance(new Vector2(x.Bounds.Center.X, x.Bounds.Center.Y), PartyLeader.Position)).First();
            travelZone.Activate(PartyLeader);
        }

        public void Summon(SummonType summon)
        {
            Spirit existingSummon = entityList.FirstOrDefault(x => x is Spirit) as Spirit;
            if (existingSummon != null)
            {
                if (existingSummon.SummonType == summon)
                {
                    (existingSummon as Spirit).RefreshLifespan();
                    return;
                }
                else
                {
                    existingSummon.Terminate();
                }
            }

            Tile closestEmptyTile = Tilemap.GetTile(PartyLeader.Center).NeighborList.First(x => !x.Blocked);

            Spirit followerHero = new Spirit(this, Tilemap, closestEmptyTile.Center, summon);
            FollowerController followerController = new FollowerController(this, followerHero, PartyLeader);
            AddEntity(followerHero);
            AddController(followerController);
            AddParticle(new AnimationParticle(this, followerHero.Position, AnimationType.Smoke, true));

            TargetOverlay targetOverlay = AddOverlay(new TargetOverlay(this, PartyLeader.Center));
            AddController(new TargetController(this, PartyLeader, followerHero, targetOverlay));
        }
    }
}
