﻿using Elenigma.SceneObjects;
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

        public string Encounter { get; set; }

        public Foe(CrawlerScene iScene, Floor iFloor, EntityInstance entity)
        {
            crawlerScene = iScene;

            string sprite = "";
            foreach (FieldInstance field in entity.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Encounter": Encounter = field.Value; break;
                    case "Sprite": sprite = field.Value; break;
                }
            }

            int TileSize = iFloor.TileSize;
            int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
            int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);

            CurrentRoom = iFloor.GetRoom(startX, startY);
            CurrentRoom.Foe = this;

            var texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + sprite)];
            float sizeX = texture.Width / 24.0f;
            float sizeY = texture.Height / 24.0f;
            Billboard = new Billboard(crawlerScene, iFloor, texture, sizeX, sizeY);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX)
        {
            if (CrossPlatformGame.CurrentScene is BattleScene.BattleScene) return;

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
            controller.UpdateTransition += new Action<float>(t => MoveInterval = t);
            controller.FinishTransition += new Action<TransitionDirection>(t => { CurrentRoom.Foe = null; CurrentRoom = DestinationRoom; CurrentRoom.Foe = this; });
        }

        public void Threaten(Direction moveDirection)
        {
            Direction = moveDirection + 2;
            if (Direction > Direction.West) Direction = moveDirection - 2;

            MoveInterval = 0.0f;
            DestinationRoom = CurrentRoom[Direction];

            TransitionController controller = new TransitionController(TransitionDirection.In, 300, PriorityLevel.CutsceneLevel);
            crawlerScene.AddController(controller);
            controller.UpdateTransition += new Action<float>(t => MoveInterval = t * 0.28f);
            controller.FinishTransition += new Action<TransitionDirection>(t =>
            {
                MoveInterval = 0.28f;

                BattleScene.BattleScene battleScene = new BattleScene.BattleScene(Encounter, this);
                CrossPlatformGame.StackScene(battleScene, true);

                battleScene.OnTerminated += new TerminationFollowup(() =>
                {
                    crawlerScene.FinishMovement();
                });
            });
        }

        public void Destroy()
        {
            crawlerScene.FoeList.Remove(this);
            CurrentRoom.Foe = null;
        }

        public bool IsMoving { get => DestinationRoom != null; }
    }
}
