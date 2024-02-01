using Elenigma.Main;
using Elenigma.SceneObjects.Maps;
using Elenigma.SceneObjects.Particles;
using ldtk;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.MapScene
{
    public class ParticleSpawner : Entity
    {
        private MapScene mapScene;

        private int interval;
        private int timer;

        private Elenigma.SceneObjects.Particles.AnimationType particleType;

        public ParticleSpawner(MapScene iMapScene, Tilemap iTilemap, EntityInstance entityInstance)
            : base(iMapScene, new Vector2())
        {
            mapScene = iMapScene;

            priorityLevel = PriorityLevel.CutsceneLevel;

            foreach (FieldInstance field in entityInstance.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Particle":
                        particleType = (AnimationType)Enum.Parse(typeof(AnimationType), field.Value);
                        break;

                    case "Interval": interval = (int)field.Value; break;
                    case "Offset": timer = (int)field.Value; break;
                }
            }

            position = new Vector2(entityInstance.Px[0] + entityInstance.Width / 2, entityInstance.Px[1] + entityInstance.Height);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            timer -= gameTime.ElapsedGameTime.Milliseconds;
            if (timer < 0)
            {
                timer = interval;

                mapScene.AddParticle(new AnimationParticle(mapScene, position, particleType));
            }
        }
    }
}
