using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Maps;
using Elenigma.SceneObjects.Widgets;
using Elenigma.Scenes.MapScene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.CrawlerScene
{
    public enum Direction
    {
        North, East, South, West, Up, Down
    }

    public class CrawlerScene : Scene
    {
        public static CrawlerScene Instance;

        private const int WALL_LENGTH = 128;

        public string MapName { get; private set; }
        public string LocationName { get; private set; } = "Test Map";


        public static RenderTarget2D mapRender;

        private MapViewModel mapViewModel;

        private MovementController movementController;

        private float cameraX = 0.0f;
        private float cameraPosX = 0.0f;
        private float cameraPosZ = 0.0f;

        private Floor floor;
        
        public int roomX = -1;
        public int roomY = -1;
        private Direction direction;

        public Panel MapPanel { get; set; }

        private int bumpCooldown;

        

        public CrawlerScene()
        {
            Instance = this;
        }

        public CrawlerScene(int huh) : this()
        {
            mapViewModel = AddView(new MapViewModel(this, GameView.CrawlerScene_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");

            floor = new Floor(this);

            roomX = 3; roomY = 3;

            floor.GetRoom(roomX, roomY).EnterRoom();

            movementController = AddController(new MovementController(this));
        }

        public CrawlerScene(string iMapName) : this()
        {

            MapName = iMapName;

            mapViewModel = AddView(new MapViewModel(this, GameView.CrawlerScene_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");

            floor = new Floor(this);

            floor.GetRoom(roomX, roomY).EnterRoom();

            movementController = AddController(new MovementController(this));
        }

        public CrawlerScene(string iMapName, string spawnName) : this()
        {
            MapName = iMapName;

            mapViewModel = AddView(new MapViewModel(this, GameView.CrawlerScene_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");

            floor = new Floor(this);

            cameraX = (float)(Math.PI * (int)direction / 2.0f);

            floor.GetRoom(roomX, roomY).EnterRoom();

            movementController = AddController(new MovementController(this));
        }

        public CrawlerScene(string iMapName, int spawnX, int spawnY, Direction iDirection) : this()
        {
            MapName = iMapName;

            mapViewModel = AddView<MapViewModel>(new MapViewModel(this, GameView.CrawlerScene_MapView));
            MapPanel = mapViewModel.GetWidget<SceneObjects.Widgets.Panel>("MapPanel");

            roomX = spawnX;
            roomY = spawnY;
            direction = iDirection;

            floor = new Floor(this);


            cameraX = (float)(Math.PI * (int)direction / 2.0f);

            floor.GetRoom(roomX, roomY).EnterRoom();

            movementController = AddController(new MovementController(this));
        }

        public void ResetPathfinding()
        {
            movementController?.Path.Clear();
        }

        public void SaveData()
        {
            GameProfile.SetSaveData<string>("LastMap", MapFileName);
            GameProfile.SetSaveData<int>("LastRoomX", roomX);
            GameProfile.SetSaveData<int>("LastRoomY", roomY);
            GameProfile.SetSaveData<Direction>("LastDirection", direction);
            GameProfile.SetSaveData<string>("PlayerLocation", LocationName);

            GameProfile.SaveState();
        }

        public static void Initialize(GraphicsDevice graphicsDevice, int multiSamples)
        {
            mapRender = new RenderTarget2D(graphicsDevice, 324, 200, false, SurfaceFormat.Color, DepthFormat.Depth16, multiSamples, RenderTargetUsage.PlatformContents);
        }

        public override void BeginScene()
        {
            base.BeginScene();

            // Audio.PlayMusic(GameMusic.Elenigma);
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.CurrentInput.CommandPressed(Command.Up) ||
                Input.CurrentInput.CommandPressed(Command.Down) ||
                Input.CurrentInput.CommandPressed(Command.Right) ||
                Input.CurrentInput.CommandPressed(Command.Left))
                ResetPathfinding();

            base.Update(gameTime);

            if (bumpCooldown > 0) bumpCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void TurnLeft()
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
            AddController(transitionController);

            transitionController.UpdateTransition += new Action<float>(t => cameraX = MathHelper.Lerp(((float)(Math.PI * ((int)direction - 1) / 2.0f)), (float)(Math.PI * (int)direction / 2.0f), t));
            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                if (direction == Direction.North) direction = Direction.West; else direction--;
                cameraX = (float)(Math.PI * (int)direction / 2.0f);
            });
        }

        public void TurnRight()
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
            AddController(transitionController);

            transitionController.UpdateTransition += new Action<float>(t => cameraX = MathHelper.Lerp(((float)(Math.PI * (int)direction / 2.0f)), (float)(Math.PI * ((int)direction + 1) / 2.0f), t));
            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                if (direction == Direction.West) direction = Direction.North; else direction++;
                cameraX = (float)(Math.PI * (int)direction / 2.0f);
            });
        }

        public void MoveForward()
        {
            TransitionController transitionController;
            
            var currentRoom = floor.GetRoom(roomX, roomY);

            switch (direction)
            {
                case Direction.North:

                    var northRoom = floor.GetRoom(roomX, roomY - 1);
                    if (roomY > 0 && northRoom != null && !northRoom.Blocked && currentRoom.Neighbors.Contains(northRoom))
                    {
                        if (northRoom.PreEnterScript != null) { northRoom.ActivatePreScript(); return; }

                        transitionController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
                        AddController(transitionController);
                        transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(0, 10, t));
                        transitionController.FinishTransition += new Action<TransitionDirection>(t => { cameraPosZ = 0; roomY--; currentRoom.EnterRoom(); });
                    }
                    else if (!Activate()) { WallBump(); return; }
                    break;

                case Direction.East:
                    var eastRoom = floor.GetRoom(roomX + 1, roomY);
                    if (roomX < floor.MapWidth - 1 && eastRoom != null && !eastRoom.Blocked && currentRoom.Neighbors.Contains(eastRoom))
                    {
                        if (eastRoom.PreEnterScript != null) { eastRoom.ActivatePreScript(); return; }

                        transitionController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
                        AddController(transitionController);
                        transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(0, 10, t));
                        transitionController.FinishTransition += new Action<TransitionDirection>(t => { cameraPosX = 0; roomX++; currentRoom.EnterRoom(); });
                    }
                    else if (!Activate()) { WallBump(); return; }
                    break;

                case Direction.South:
                    var southRoom = floor.GetRoom(roomX, roomY + 1);
                    if (roomY < floor.MapHeight - 1 && southRoom != null && !southRoom.Blocked && currentRoom.Neighbors.Contains(southRoom))
                    {
                        if (southRoom.PreEnterScript != null) { southRoom.ActivatePreScript(); return; }

                        transitionController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
                        AddController(transitionController);
                        transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(-10, 0, t));
                        transitionController.FinishTransition += new Action<TransitionDirection>(t => { cameraPosZ = 0; roomY++; currentRoom.EnterRoom(); });
                    }
                    else if (!Activate()) { WallBump(); return; }
                    break;

                case Direction.West:
                    var westRoom = floor.GetRoom(roomX - 1, roomY);
                    if (roomX > 0 && westRoom != null && !westRoom.Blocked && currentRoom.Neighbors.Contains(westRoom))
                    {
                        if (westRoom.PreEnterScript != null) { westRoom.ActivatePreScript(); return; }

                        transitionController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
                        AddController(transitionController);
                        transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(-10, 0, t));
                        transitionController.FinishTransition += new Action<TransitionDirection>(t => { cameraPosX = 0; roomX--; currentRoom.EnterRoom(); });
                    }
                    else if (!Activate()) { WallBump(); return; }
                    break;
            }
        }

        public void MoveTo(MapRoom destinationRoom)
        {
            Direction requiredDirection;
            if (destinationRoom.RoomX > roomX) requiredDirection = Direction.East;
            else if (destinationRoom.RoomX < roomX) requiredDirection = Direction.West;
            else if (destinationRoom.RoomY > roomY) requiredDirection = Direction.South;
            else requiredDirection = Direction.North;

            if (requiredDirection == direction) MoveForward();
            else
            {
                if (requiredDirection == direction + 1 || (requiredDirection == Direction.North && direction == Direction.West)) TurnRight();
                else TurnLeft();
            }
        }

        private void WallBump()
        {
            if (bumpCooldown <= 0)
            {
                Audio.PlaySound(GameSound.wall_bump);
                bumpCooldown = 350;
            }
        }

        public override void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D pixelRender, RenderTarget2D compositeRender)
        {
            graphicsDevice.SetRenderTarget(mapRender);
            floor.DrawMap(graphicsDevice, mapViewModel.GetWidget<Panel>("MapPanel"), roomX, roomY, cameraPosX, cameraPosZ, cameraX);

            graphicsDevice.SetRenderTarget(pixelRender);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawBackground(spriteBatch);
            spriteBatch.End();

            Matrix matrix = (Camera == null) ? Matrix.Identity : Camera.Matrix;
            Effect shader = (spriteShader == null) ? null : spriteShader.Effect;
            foreach (Entity entity in entityList) entity.DrawShader(spriteBatch, Camera, matrix);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, matrix);
            DrawGame(spriteBatch, shader, matrix);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawOverlay(spriteBatch);
            spriteBatch.End();

            Rectangle miniMapBounds = MapViewModel.GetWidget<Panel>("MiniMapPanel").InnerBounds;
            miniMapBounds.X += (int)MapViewModel.GetWidget<Panel>("MiniMapPanel").Position.X;
            miniMapBounds.Y += (int)MapViewModel.GetWidget<Panel>("MiniMapPanel").Position.Y;
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            floor.DrawMiniMap(spriteBatch, miniMapBounds, Color.White, 0.6f, roomX, roomY, direction);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(compositeRender);

            if (!CrossPlatformGame.ClearedCompositeRender)
            {
                CrossPlatformGame.ClearedCompositeRender = true;
                graphicsDevice.Clear(Color.Transparent);
            }

            shader = (SceneShader == null) ? null : SceneShader.Effect;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, Matrix.Identity);
            spriteBatch.Draw(pixelRender, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.End();


        }

        public bool Activate()
        {
            return floor.GetRoom(roomX, roomY).Activate(direction);
        }

        public void MiniMapClick(Vector2 clickPosition)
        {
            /*
            if (PriorityLevel != PriorityLevel.GameLevel || controllerList.Any(x => x.Any(y => y is EventController))) return;

            Panel miniMapPanel = mapViewModel.GetWidget<Panel>("MiniMapPanel");
            clickPosition -= new Vector2((miniMapPanel.InnerBounds.Width - 7 * 8) / 2, (miniMapPanel.InnerBounds.Height - 7 * 8) / 2);
            int newRoomX = (int)clickPosition.X / 8 + MinimapStartX;
            int newRoomY = (int)clickPosition.Y / 8 + MinimapStartY;

            if (newRoomX >= 0 && newRoomY >= 0 && newRoomX < mapRooms.GetLength(0) && newRoomY < mapRooms.GetLength(1))
            {
                if (mapRooms[newRoomX, newRoomY] != null && !mapRooms[newRoomX, newRoomY].Blocked)
                {
                    movementController.Path = GetPath(mapRooms[roomX, roomY], mapRooms[newRoomX, newRoomY]);
                }
            }
            */
        }

        public MapViewModel MapViewModel { get => mapViewModel; }

        


        public string MapFileName { get; set; }

        public Floor Floor { get => floor; }
    }
}
