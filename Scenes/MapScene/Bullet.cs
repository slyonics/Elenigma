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
    public class Bullet : Actor
    {
        private MapScene mapScene;


        public Bullet(MapScene iMapScene, Tilemap iTilemap, Vector2 iPosition, Rectangle iBounds)
            : base(iMapScene, iTilemap, iPosition, iBounds, Orientation.Down)
        {
            mapScene = iMapScene;

            UpdateBounds();

            desiredVelocity = Vector2.Zero;
        }
    }
}
