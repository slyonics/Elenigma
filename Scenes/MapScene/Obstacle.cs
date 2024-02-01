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
    public class Obstacle : Actor
    {
        private MapScene mapScene;

        public string[] Script { get; private set; }


        public Obstacle(MapScene iMapScene, Tilemap iTilemap, EntityInstance entityInstance)
            : base(iMapScene, iTilemap, new Vector2(), new Rectangle(-(int)entityInstance.Width / 2, -(int)entityInstance.Height, (int)entityInstance.Width, (int)entityInstance.Height), Orientation.Down)
        {
            mapScene = iMapScene;

            foreach (FieldInstance field in entityInstance.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Sprite":
                        if (field.Value != null) animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), field.Value)], null);
                        if (field.Value != null && (field.Value as string).Contains("Crystal")) PriorityLevel = PriorityLevel.CutsceneLevel;

                        if (field.Value == "Slyph")
                        {
                            SetFlight(6, AssetCache.SPRITES[GameSprite.Actors_DroneShadow]);
                        }
                        break;

                    case "Script": if (field.Value != null) Script = field.Value.Split('\n'); break;
                }
            }

            position = new Vector2(entityInstance.Px[0] + entityInstance.Width / 2, entityInstance.Px[1] + entityInstance.Height);
            UpdateBounds();

            desiredVelocity = Vector2.Zero;
        }

        public void Hit()
        {
            Terminate();

            if (Script == null || Script.Length == 0) return;

            EventController eventController = new EventController(mapScene, Script);
            mapScene.AddController(eventController);
        }
    }
}
