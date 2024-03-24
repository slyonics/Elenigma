﻿using Elenigma.SceneObjects.Maps;
using ldtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elenigma.SceneObjects.Shaders;
using Elenigma.Scenes.MapScene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Elenigma.SceneObjects;
using static Elenigma.SceneObjects.ParallaxBackdrop;

namespace Elenigma.Scenes.CrawlerScene
{
    public class Floor
    {
        private class Tileset
        {
            public Tileset(TilesetDefinition tilesetDefinition)
            {
                TilesetDefinition = tilesetDefinition;
                string tilesetName = tilesetDefinition.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
                SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
            }

            public TilesetDefinition TilesetDefinition { get; private set; }
            public Texture2D SpriteAtlas { get; private set; }
        }

        private Texture2D minimapSprite = AssetCache.SPRITES[GameSprite.YouAreHere];
        private static readonly Rectangle[] minimapSource = new Rectangle[] { new Rectangle(0, 0, 8, 8), new Rectangle(8, 0, 8, 8), new Rectangle(16, 0, 8, 8), new Rectangle(24, 0, 8, 8) };

        public int TileSize { get; set; }
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }

        private GameMap gameMap;
        public Definitions Definitions { get; set; }
        public Level Level { get; set; }

        private Dictionary<long, Tileset> tilesets = new Dictionary<long, Tileset>();

        private MapRoom[,] mapRooms = new MapRoom[8, 8];

        private List<Tile> visibleTiles = new List<Tile>();
        private bool revealAll;

        CrawlerScene parentScene;


        public float AmbientLight { get; private set; } = 1;


        public int MinimapStartX { get; set; } = 0;
        public int MinimapStartY { get; set; } = 0;




        public Floor(CrawlerScene crawlerScene)
        {
            parentScene = crawlerScene;

            LoadMap(GameMap.CrawlerMap);
        }

        public void LoadMap(GameMap iGameMap, string spawnName = "Default")
        {
            gameMap = iGameMap;

            LdtkJson ldtkJson = LdtkJson.FromJson(AssetCache.MAPS[gameMap]);
            Definitions = ldtkJson.Defs;
            Level = ldtkJson.Levels[0];
            foreach (TilesetDefinition tilesetDefinition in Definitions.Tilesets)
            {
                tilesets.Add(tilesetDefinition.Uid, new Tileset(tilesetDefinition));
            }

            TileSize = (int)Level.LayerInstances[0].GridSize;
            MapWidth = (int)Level.LayerInstances[0].CWid;
            MapHeight = (int)Level.LayerInstances[0].CHei;
            mapRooms = new MapRoom[MapWidth, MapHeight];


            foreach (LayerInstance layer in Level.LayerInstances.Reverse())
            {
                if (layer.Type == "Entities") continue;

                var tileset = Definitions.Tilesets.First(x => x.Uid == layer.TilesetDefUid);
                foreach (var tile in layer.GridTiles)
                {
                    int x = (int)(tile.Px[0]) / TileSize;
                    int y = (int)(tile.Px[1]) / TileSize;
                    var room = mapRooms[x, y];

                    if (room == null)
                    {
                        room = mapRooms[x, y] = new MapRoom(parentScene, this, x, y);
                    }

                    room.ApplyTile(layer.Identifier, tileset, tile);
                }
            }



            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    mapRooms[x, y]?.BuildNeighbors();
                }
            }

            foreach (LayerInstance layer in Level.LayerInstances)
            {
                if (layer.Type == "Entities")
                {
                    foreach (var entity in layer.EntityInstances) ParseEntity(entity);
                }
            }

            AmbientLight = 0.6f;

            FinishMap();
        }

        private void ParseEntity(EntityInstance entity)
        {
            switch (entity.Identifier)
            {
                case "Light":
                    int startX = (int)(entity.Px[0] / TileSize);
                    int startY = (int)(entity.Px[1] / TileSize);
                    int endX = (int)(entity.Width / TileSize);
                    int endY = (int)(entity.Height / TileSize);

                    int brightness = 4;
                    foreach (FieldInstance field in entity.FieldInstances)
                    {
                        if (field.Identifier == "Brightness") brightness = (int)field.Value;
                    }

                    Lighting(startX, startY, endX, endY, brightness);
                    break;
            }
        }

        public void FinishMap()
        {
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    mapRooms[x, y]?.SetVertices(x, y);
                }
            }
        }


        void Lighting(int startX, int startY, int width, int height, int brightness)
        {
            int fullBrightness = 0;
            int attenuatedBrightness = brightness;

            MapRoom originRoom = mapRooms[startX + width / 2, startY + height / 2];
            List<MapRoom> visitedRooms = new List<MapRoom>();
            List<MapRoom> roomsToVisit = new List<MapRoom>() { originRoom };
            List<MapRoom> nextRooms = new List<MapRoom>();
            while (attenuatedBrightness > 0)
            {
                visitedRooms.AddRange(roomsToVisit);
                foreach (MapRoom room in roomsToVisit)
                {
                    room.brightnessLevel += attenuatedBrightness;
                    nextRooms.AddRange(room.Neighbors.FindAll(x => !x.Blocked && !visitedRooms.Contains(x) && !nextRooms.Contains(x)));
                }

                roomsToVisit = nextRooms;
                nextRooms = new List<MapRoom>();

                if (fullBrightness > 0) fullBrightness--;
                else attenuatedBrightness--;
            }

            for (int x = 0; x < mapRooms.GetLength(0); x++)
            {
                for (int y = 0; y < mapRooms.GetLength(1); y++)
                {
                    MapRoom mapRoom = mapRooms[x, y];
                    mapRoom?.BlendLighting();
                }
            }
        }


        public void DrawMap(GraphicsDevice graphicsDevice, Panel mapWindow, int roomX, int roomY, float cameraPosX, float cameraPosZ, float cameraX)
        {
            graphicsDevice.Clear(new Color(0.0f, 1.0f, 0.5f, 0.0f));

            if (!mapWindow.Transitioning)
            {
                Rectangle mapBounds = mapWindow.InnerBounds;
                mapBounds.X += (int)mapWindow.Position.X;
                mapBounds.Y += (int)mapWindow.Position.Y;


                Vector3 cameraUp = new Vector3(0, -1, 0);
                Vector3 cameraPos = new Vector3(cameraPosX + 10 * roomX, 0, cameraPosZ + 10 * (mapRooms.GetLength(1) - roomY));
                Matrix viewMatrix = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(new Vector3(0, 0, 1), Matrix.CreateRotationY(cameraX)), cameraUp);

                for (int x = 0; x < mapRooms.GetLength(0); x++)
                {
                    for (int y = 0; y < mapRooms.GetLength(1); y++)
                    {
                        mapRooms[x, y]?.Draw(viewMatrix);
                    }
                }
            }
        }

        public void DrawMiniMap(SpriteBatch spriteBatch, Rectangle bounds, Color color, float depth, int roomX, int roomY, Direction direction)
        {
            MinimapStartX = Math.Max(0, roomX - 5);
            int endX = MinimapStartX + 12;
            if (endX > mapRooms.GetLength(0) - 1)
            {
                endX = mapRooms.GetLength(0) - 1;
                MinimapStartX = Math.Max(0, endX - 11);
            }

            MinimapStartY = Math.Max(0, roomY - 5);
            int endY = MinimapStartY + 12;
            if (endY > mapRooms.GetLength(1) - 1)
            {
                endY = mapRooms.GetLength(1) - 1;
                MinimapStartY = Math.Max(0, endY - 11);
            }

            Vector2 offset = new Vector2(bounds.X, bounds.Y);
            for (int x = MinimapStartX; x < endX; x++)
            {
                for (int y = MinimapStartY; y < endY; y++)
                {
                    MapRoom mapRoom = mapRooms[x, y];
                    spriteBatch.Draw(minimapSprite, offset, new Rectangle(0, 0, 8, 8), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth - 0.001f);
                    mapRoom?.DrawMinimap(spriteBatch, offset, depth - 0.002f);

                    offset.Y += 8;
                }

                offset.Y = bounds.Y;
                offset.X += 8;
            }

            spriteBatch.Draw(minimapSprite, new Vector2((roomX - MinimapStartX) * 8, (roomY - MinimapStartY) * 8) + new Vector2(bounds.X, bounds.Y), minimapSource[(int)direction], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth - 0.003f);
        }

        public List<MapRoom> GetPath(MapRoom startTile, MapRoom endTile)
        {
            List<MapRoom> processedTiles = new List<MapRoom>();
            List<MapRoom> unprocessedTiles = new List<MapRoom> { startTile };
            Dictionary<MapRoom, MapRoom> cameFrom = new Dictionary<MapRoom, MapRoom>();
            Dictionary<MapRoom, int> currentDistance = new Dictionary<MapRoom, int>();
            Dictionary<MapRoom, int> predictedDistance = new Dictionary<MapRoom, int>();

            currentDistance.Add(startTile, 0);
            predictedDistance.Add(startTile, Distance(startTile, endTile));

            while (unprocessedTiles.Count > 0)
            {
                // get the node with the lowest estimated cost to finish
                MapRoom current = (from p in unprocessedTiles orderby predictedDistance[p] ascending select p).First();

                // if it is the finish, return the path
                if (current.RoomX == endTile.RoomX && current.RoomY == endTile.RoomY)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, endTile);
                }

                // move current node from open to closed
                unprocessedTiles.Remove(current);
                processedTiles.Add(current);

                foreach (MapRoom neighbor in current.Neighbors)
                {
                    int tempCurrentDistance = currentDistance[current] + Distance(neighbor, endTile);

                    // if we already know a faster way to this neighbor, use that route and ignore this one
                    if (processedTiles.Contains(neighbor) && tempCurrentDistance >= currentDistance[neighbor]) continue;

                    // if we don't know a route to this neighbor, or if this is faster, store this route
                    if (!processedTiles.Contains(neighbor) || tempCurrentDistance < currentDistance[neighbor])
                    {
                        if (cameFrom.Keys.Contains(neighbor)) cameFrom[neighbor] = current;
                        else cameFrom.Add(neighbor, current);

                        currentDistance[neighbor] = tempCurrentDistance;
                        predictedDistance[neighbor] = currentDistance[neighbor] + Distance(neighbor, endTile);

                        if (!unprocessedTiles.Contains(neighbor)) unprocessedTiles.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private int Distance(MapRoom room1, MapRoom room2)
        {
            return Math.Abs(room1.RoomX - room2.RoomX) + Math.Abs(room1.RoomY - room2.RoomY);
        }

        private static List<MapRoom> ReconstructPath(Dictionary<MapRoom, MapRoom> cameFrom, MapRoom current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<MapRoom> { current };
            }

            List<MapRoom> path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

        public MapRoom GetRoom(int x, int y)
        {
            if (x < 0 || y < 0 || x >= mapRooms.GetLength(0) || y >= mapRooms.GetLength(1)) return null;
            return mapRooms[x, y];
        }
    }
}
