using Elenigma.Main;
using Elenigma.SceneObjects.Maps;
using ldtk;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class Npc : Actor, IInteractive
    {
        protected enum NpcAnimation
        {
            IdleDown,
            IdleLeft,
            IdleRight,
            IdleUp,
            WalkDown,
            WalkLeft,
            WalkRight,
            WalkUp
        }

        public const int NPC_WIDTH = 32;
        public const int NPC_HEIGHT = 32;

        public static readonly Rectangle NPC_BOUNDS = new Rectangle(-7, -8, 13, 7);

        private static readonly Dictionary<string, Animation> NPC_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { NpcAnimation.IdleDown.ToString(), new Animation(1, 0, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleLeft.ToString(), new Animation(1, 3, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleRight.ToString(), new Animation(1, 2, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleUp.ToString(), new Animation(1, 1, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.WalkDown.ToString(), new Animation(0, 0, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkLeft.ToString(), new Animation(0, 3, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkRight.ToString(), new Animation(0, 2, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkUp.ToString(), new Animation(0, 1, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
        };

        private MapScene mapScene;

        private string[] interactionScript = null;

        public Npc(MapScene iMapScene, Tilemap iTilemap, EntityInstance entityInstance, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, new Vector2(), NPC_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            foreach (FieldInstance field in entityInstance.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Sprite":
                        if (field.Value != null) animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + field.Value)], NPC_ANIMATIONS);
                        if (field.Value != null && (field.Value as string).Contains("Crystal")) PriorityLevel = PriorityLevel.CutsceneLevel;

                        if (field.Value == "Slyph")
                        {
                            SetFlight(6, AssetCache.SPRITES[GameSprite.Actors_DroneShadow]);
                        }
                        break;

                    case "Behavior": if (field.Value != null) Behavior = field.Value.Split('\n'); break;
                    case "Interact": if (field.Value != null) interactionScript = field.Value.Split('\n'); break;
                    case "Direction": if (field.Value != null) Orientation = (Orientation)Enum.Parse(typeof(Orientation), field.Value); break;
                    case "Label": Label = field.Value; break;
                    case "Name": Name = field.Value; break;
                }
            }

            CenterOn(iTilemap.GetTile(new Vector2(entityInstance.Px[0] + entityInstance.Width / 2, entityInstance.Px[1] + entityInstance.Height / 2)).Center);

            tilemap.GetTile(Center).Occupants.Add(this);
            HostTile = tilemap.GetTile(Center);
            Idle();

            
        }

        public Npc(MapScene iMapScene, Tilemap iTilemap, int x, int y, string spriteName, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, new Vector2(), NPC_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + spriteName)], NPC_ANIMATIONS);

            CenterOn(iTilemap.GetTile(x, y).Center);

            tilemap.GetTile(Center).Occupants.Add(this);
            HostTile = tilemap.GetTile(Center);
            Idle();

            if (spriteName == "Slyph")
            {
                SetFlight(6, AssetCache.SPRITES[GameSprite.Actors_DroneShadow]);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (PriorityLevel == PriorityLevel.GameLevel && CrossPlatformGame.SceneStack.Count > 0) return;

            base.Update(gameTime);
        }

        public virtual bool Activate(Hero activator)
        {
            if (interactionScript == null) return false;

            Rectangle areaOfInterest = Rectangle.Union(SpriteBounds, mapScene.PartyLeader.SpriteBounds);
            EventController eventController = new EventController(mapScene, interactionScript);
            eventController.ActorSubject = this;

            mapScene.AddController(eventController);
            controllerList.Add(eventController);

            Reorient(activator.Center - Center);
            OrientedAnimation("Idle");

            return true;
        }

        public string Name { get; protected set; } = "Npc";
        public string Label { get; protected set; } = "NPC";
        public string[] Behavior { get; protected set; } = null;
        public Vector2 LabelPosition { get => new Vector2(position.X, position.Y - animatedSprite.SpriteBounds().Height - 8); }
        public virtual bool Interactive { get => interactionScript != null; }

        public Tile HostTile { get; set; }
        public int TileX { get => HostTile.TileX; }
        public int TileY { get => HostTile.TileY; }
    }
}
