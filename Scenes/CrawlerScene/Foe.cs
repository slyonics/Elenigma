using Elenigma.SceneObjects;
using ldtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

namespace Elenigma.Scenes.CrawlerScene
{
    public class Foe
    {
        private CrawlerScene crawlerScene;
        public Billboard Billboard { get; private set; }

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

            Billboard = new Billboard(crawlerScene, iFloor, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + sprite)], startX, startY, size);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX)
        {
            float x = 10 * (Billboard.RoomX);
            float z = 10 * (crawlerScene.Floor.MapHeight - Billboard.RoomY);
            Billboard.Draw(viewMatrix, x, z, cameraX);
        }
    }
}
