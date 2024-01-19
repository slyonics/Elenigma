using Elenigma.SceneObjects.Shaders;
using Elenigma.Scenes.MapScene;
using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Elenigma.SceneObjects.ParallaxBackdrop;

namespace Elenigma.SceneObjects.Maps
{
    public class Tilemap : Entity
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

        private MapScene mapScene;

        private GameMap gameMap;
        public Definitions Definitions { get; set; }
        public Level Level { get; set; }

        private Dictionary<long, Tileset> tilesets = new Dictionary<long, Tileset>();

        private Tile[,] tiles;

        private List<Tile> visibleTiles = new List<Tile>();
        private bool revealAll;

        private NavNode[,] navMesh;

        public List<Rectangle> MapColliders { get; } = new List<Rectangle>();

        public List<Light> Lights { get; private set; } = new List<Light>();

        public string Name { get => gameMap.ToString(); }

        public Tilemap(MapScene iScene, GameMap iGameMap)
            : base(iScene, Vector2.Zero)
        {
            mapScene = iScene;
            gameMap = iGameMap;

            LdtkJson ldtkJson = LdtkJson.FromJson(AssetCache.MAPS[gameMap]);

            Definitions = ldtkJson.Defs;
            Level = ldtkJson.Levels[0];

            TileSize = (int)Level.LayerInstances[0].GridSize;
            Columns = (int)Level.LayerInstances[0].CWid;
            Rows = (int)Level.LayerInstances[0].CHei;
            Width = TileSize * Columns;
            Height = TileSize * Rows;

            foreach (TilesetDefinition tilesetDefinition in Definitions.Tilesets)
            {
                tilesets.Add(tilesetDefinition.Uid, new Tileset(tilesetDefinition));
            }

            tiles = new Tile[Columns, Rows];
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y] = new Tile(this, x, y);
                }
            }

            LoadLayers(Level.LayerInstances);

            for (int y = Rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y].AssignNeighbors();
                }
            }

            BuildNavMesh();

            var entityLayers = Level.LayerInstances.Where(x => x.Type == "Entities");
            foreach (var entityLayer in entityLayers)
            {
                foreach (EntityInstance entity in entityLayer.EntityInstances)
                {
                    switch (entity.Identifier)
                    {
                        case "ColliderBox":
                            MapColliders.Add(new Rectangle((int)entity.Px[0], (int)entity.Px[1], (int)entity.Width, (int)entity.Height));
                            break;

                        case "Light":
                            {
                                Light light = new Light(new Vector2(entity.Px[0] + entity.Width / 2, entity.Px[1] + entity.Height / 2), 0.0f);
                                light.Color = Graphics.ParseHexcode("#" + entity.FieldInstances.FirstOrDefault(x => x.Identifier == "Color").Value.Substring(1));
                                light.Intensity = entity.FieldInstances.FirstOrDefault(x => x.Identifier == "Intensity").Value;
                                Lights.Add(light);
                                break;
                            }
                    }
                }
            }
                    }

        protected virtual void LoadLayers(LayerInstance[] layers)
        {
            foreach (LayerInstance layer in layers.Reverse())
            {
                switch (layer.Type)
                {
                    case "Tiles": LoadTileLayer(layer); break;
                    case "IntGrid": LoadTileLayer(layer); break;
                    case "AutoLayer": LoadTileLayer(layer); break;
                }
            }
        }

        protected virtual void LoadTileLayer(LayerInstance layer)
        {
            var tileset = Definitions.Tilesets.First(x => x.Uid == layer.TilesetDefUid);

            foreach (var tile in layer.GridTiles)
            {
                int x = (int)(tile.Px[0]) / TileSize;
                int y = (int)(tile.Px[1]) / TileSize;
                tiles[x, y].ApplyTileLayer(tile, tileset, layer, new Rectangle((int)tile.Src[0], (int)tile.Src[1], TileSize, TileSize), tilesets[layer.TilesetDefUid.Value].SpriteAtlas);
            }

            foreach (var tile in layer.AutoLayerTiles)
            {
                int x = (int)(tile.Px[0]) / TileSize;
                int y = (int)(tile.Px[1]) / TileSize;
                tiles[x, y].ApplyTileLayer(tile, tileset, layer, new Rectangle((int)tile.Src[0], (int)tile.Src[1], TileSize, TileSize), tilesets[layer.TilesetDefUid.Value].SpriteAtlas);
            }


            // tiles[x, y].ApplyEntityTile(tilesetTile, tiledLayer, new Rectangle(spriteSource.x, spriteSource.y, spriteSource.width, spriteSource.height), tileset.SpriteAtlas, height);

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y].Update(gameTime);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((camera.View.Left / TileSize) - 1, 0);
            int startTileY = Math.Max((camera.View.Top / TileSize) - 1, 0);
            int endTileX = Math.Min((camera.View.Right / TileSize), Columns - 1);
            int endTileY = Math.Min((camera.View.Bottom / TileSize), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].DrawBackground(spriteBatch, camera);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((camera.View.Left / TileSize) - 1, 0);
            int startTileY = Math.Max((camera.View.Top / TileSize) - 1, 0);
            int endTileX = Math.Min((camera.View.Right / TileSize), Columns - 1);
            int endTileY = Math.Min((camera.View.Bottom / TileSize), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].Draw(spriteBatch, camera);
                }
            }
        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {

        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns || y >= Rows) return null;
            return tiles[x, y];
        }

        public Tile GetTile(Vector2 position)
        {
            int tileX = (int)(position.X / TileSize);
            int tileY = (int)(position.Y / TileSize);

            if (tileX < 0 || tileY < 0 || tileX >= Columns || tileY >= Rows) return null;

            return tiles[tileX, tileY];
        }

        public NavNode GetNavNode(Vector2 position)
        {
            int nodeX = (int)(position.X / NavNode.NODE_SIZE) - 1;
            int nodeY = (int)(position.Y / NavNode.NODE_SIZE) - 1;
            if (nodeX < 0) nodeX = 0;
            if (nodeY < 0) nodeY = 0;
            if (nodeX > Columns * 2 - 2) nodeX = Columns * 2 - 2;
            if (nodeY > Rows * 2 - 2) nodeY = Rows * 2 - 2;

            return navMesh[nodeX, nodeY];
        }

        public NavNode GetNavNode(Actor actor)
        {
            int nodeX = (int)(actor.Center.X / NavNode.NODE_SIZE) - 1;
            int nodeY = (int)(actor.Bounds.Bottom / NavNode.NODE_SIZE) - 1;
            if (nodeX < 0) nodeX = 0;
            if (nodeY < 0) nodeY = 0;
            if (nodeX > Columns * 2 - 2) nodeX = Columns * 2 - 2;
            if (nodeY > Rows * 2 - 2) nodeY = Rows * 2 - 2;

            NavNode closestNode = navMesh[nodeX, nodeY];

            List<NavNode> nodeList = new List<NavNode>();
            nodeList.Add(closestNode);
            nodeList.AddRange(closestNode.NeighborList);

            IOrderedEnumerable<NavNode> sortedNodes = nodeList.OrderBy(x => Vector2.Distance(x.Center, new Vector2(actor.Center.X, actor.Bounds.Bottom)));
            return sortedNodes.FirstOrDefault(x => x.AccessibleFromActor(actor));
        }

        public NavNode GetNavNode(Actor seeker, Actor target)
        {
            NavNode targetNode = GetNavNode(target);
            if (targetNode == null) return null;
            if (targetNode.FitsActor(seeker)) return targetNode;

            List<NavNode> nodeList = new List<NavNode>();
            nodeList.Add(targetNode);
            nodeList.AddRange(targetNode.NeighborList);

            IOrderedEnumerable<NavNode> sortedNodes = nodeList.OrderBy(x => Vector2.Distance(x.Center, new Vector2(seeker.Center.X, seeker.Bounds.Bottom)));
            return sortedNodes.FirstOrDefault(x => x.FitsActor(seeker));
        }

        public NavNode GetNavNode(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns * 2 - 1 || y >= Rows * 2 - 1) return null;

            return navMesh[x, y];
        }

        private void BuildNavMesh()
        {
            navMesh = new NavNode[Columns * 2 - 1, Rows * 2 - 1];
            for (int y = 0; y < Rows * 2 - 1; y++)
            {
                for (int x = 0; x < Columns * 2 - 1; x++)
                {
                    navMesh[x, y] = new NavNode(this, x, y);
                }
            }

            for (int y = 0; y < Rows * 2 - 1; y++)
            {
                for (int x = 0; x < Columns * 2 - 1; x++)
                {
                    navMesh[x, y].AssignNeighbors(this);
                }
            }
        }

        public List<NavNode> GetPath(NavNode startNode, NavNode endNode, Actor actor, int searchLimit)
        {
            List<NavNode> processedNodes = new List<NavNode>();
            List<NavNode> unprocessedNodes = new List<NavNode> { startNode };
            Dictionary<NavNode, NavNode> cameFrom = new Dictionary<NavNode, NavNode>();
            Dictionary<NavNode, int> currentDistance = new Dictionary<NavNode, int>();
            Dictionary<NavNode, int> predictedDistance = new Dictionary<NavNode, int>();

            int searchCount = 0;

            currentDistance.Add(startNode, 0);
            predictedDistance.Add(startNode, (int)Vector2.Distance(startNode.Center, endNode.Center));

            while (unprocessedNodes.Count > 0 && searchCount < searchLimit)
            {
                searchCount++;

                // get the node with the lowest estimated cost to finish
                NavNode current = (from p in unprocessedNodes orderby predictedDistance[p] ascending select p).First();

                // if it is the finish, return the path
                if (current == endNode)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, endNode);
                }

                // move current node from open to closed
                unprocessedNodes.Remove(current);
                processedNodes.Add(current);

                foreach (NavNode neighbor in current.NeighborList)
                {
                    if (neighbor.AccessibleFromNode(current, actor))
                    {
                        int tempCurrentDistance = currentDistance[current] + (int)Vector2.Distance(current.Center, neighbor.Center);

                        // if we already know a faster way to this neighbor, use that route and ignore this one
                        if (currentDistance.ContainsKey(neighbor) && tempCurrentDistance >= currentDistance[neighbor]) continue;

                        // if we don't know a route to this neighbor, or if this is faster, store this route
                        if (!processedNodes.Contains(neighbor) || tempCurrentDistance < currentDistance[neighbor])
                        {
                            if (cameFrom.Keys.Contains(neighbor)) cameFrom[neighbor] = current;
                            else cameFrom.Add(neighbor, current);

                            currentDistance[neighbor] = tempCurrentDistance;
                            predictedDistance[neighbor] = currentDistance[neighbor] + (int)Vector2.Distance(neighbor.Center, endNode.Center);

                            if (!unprocessedNodes.Contains(neighbor)) unprocessedNodes.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private static List<NavNode> ReconstructPath(Dictionary<NavNode, NavNode> cameFrom, NavNode current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<NavNode> { current };
            }

            List<NavNode> path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

        public bool CanTraverse(Actor actor, Tile destinationTile)
        {
            Rectangle destinationBounds = new Rectangle((int)destinationTile.Center.X + actor.BoundingBox.Left, (int)destinationTile.Center.Y + actor.BoundingBox.Top, actor.BoundingBox.Width, actor.BoundingBox.Height);
            Rectangle boundsForActor = Rectangle.Union(actor.Bounds, destinationBounds);

            int tileStartX = boundsForActor.Left / Width;
            int tileEndX = boundsForActor.Right / Width;
            int tileStartY = boundsForActor.Top / Height;
            int tileEndY = boundsForActor.Bottom / Height;

            List<Rectangle> colliderList = new List<Rectangle>();
            for (int x = tileStartX; x <= tileEndX; x++)
            {
                for (int y = tileStartY; y <= tileEndY; y++)
                {
                    colliderList.AddRange(GetTile(x, y).ColliderList);
                }
            }

            colliderList.AddRange(MapColliders);

            foreach (Rectangle collider in colliderList)
            {
                if (collider.Intersects(boundsForActor)) return false;
            }

            return true;
        }

        
        public int TileSize { get; set; }

        public int Columns { get; set; }
        public int Rows { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
