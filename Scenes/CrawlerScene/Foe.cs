using Elenigma.SceneObjects;
using ldtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Elenigma.SceneObjects.Controllers;

namespace Elenigma.Scenes.CrawlerScene
{
    public class Foe
    {
        private CrawlerScene crawlerScene;
        public Billboard Billboard { get; private set; }

        public Direction Direction { get; private set; }
        public MapRoom CurrentRoom { get; private set; }
        public MapRoom DestinationRoom { get; private set; }
        public float MoveInterval { get; set; }

        public Foe(CrawlerScene iScene, Floor iFloor, EntityInstance entity)
        {
            crawlerScene = iScene;

            int size = 6;
            string sprite = "";
            foreach (FieldInstance field in entity.FieldInstances)
            {
                if (field.Identifier == "Sprite") sprite = field.Value;
                if (field.Identifier == "Size") size = (int)field.Value;
            }

            int TileSize = iFloor.TileSize;
            int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
            int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);

            CurrentRoom = iFloor.GetRoom(startX, startY);
            CurrentRoom.Foe = this;

            Billboard = new Billboard(crawlerScene, iFloor, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + sprite)], size);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX)
        {
            float x = 10 * CurrentRoom.RoomX;
            float z = 10 * (crawlerScene.Floor.MapHeight - CurrentRoom.RoomY);
            float brightness = CurrentRoom.Brightness(CurrentRoom.brightnessLevel);

            if (IsMoving)
            {
                float destX = 10 * DestinationRoom.RoomX;
                float destZ = 10 * (crawlerScene.Floor.MapHeight - DestinationRoom.RoomY);
                float destBrightness = CurrentRoom.Brightness(DestinationRoom.brightnessLevel);

                Billboard.Draw(viewMatrix, MathHelper.Lerp(x, destX, MoveInterval), MathHelper.Lerp(z, destZ, MoveInterval), cameraX, MathHelper.Lerp(brightness, destBrightness, MoveInterval));
            }
            else
            {
                Billboard.Draw(viewMatrix, x, z, cameraX, brightness);
            }
        }

        public void Move(Direction moveDirection)
        {
            var newRoom = CurrentRoom[moveDirection];
            if (newRoom == null || newRoom.Blocked) return;

            Direction = moveDirection;
            MoveInterval = 0.0f;
            DestinationRoom = newRoom;

            TransitionController controller = new TransitionController(TransitionDirection.In, 300, PriorityLevel.CutsceneLevel);
            crawlerScene.AddController(controller);
            controller.UpdateTransition += new Action<float>(t => CurrentRoom.Foe.MoveInterval = t);
            controller.FinishTransition += new Action<TransitionDirection>(t => { CurrentRoom.Foe = null; CurrentRoom = DestinationRoom; CurrentRoom.Foe = this; });
        }

        public bool IsMoving { get => DestinationRoom != null; }
    }
}
