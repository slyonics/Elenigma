using Elenigma.Models;
using Elenigma.SceneObjects.Maps;
using Elenigma.Scenes.MapScene;
using ldtk;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.CrawlerScene
{
    public class Billboard
    {
            public VertexPositionTexture[] Quad { get; set; }
            public Texture2D Texture { get; set; }
            public WallShader Shader { get; set; }

        private const int WALL_HALF_LENGTH = 3;
        private const int CAM_HEIGHT = 1;
        private static readonly short[] INDICES = new short[] { 0, 2, 1, 2, 0, 3 };
        private static readonly Vector3[] VERTICES = new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, 0),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, 0),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, 0),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, 0) };

        public int RoomX { get; set; }
        public int RoomY { get; set; }

        private GraphicsDevice graphicsDevice = CrossPlatformGame.GameInstance.GraphicsDevice;

        private CrawlerScene parentScene;

        private Matrix translationMatrix;

        public Billboard(CrawlerScene mapScene, Floor iFloor, Texture2D sprite, int x, int y)
        {
            parentScene = mapScene;
            RoomX = x;
            RoomY = y;

            Shader = new WallShader(Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 472 / 332.0f, 0.7f, 10000.0f));
            Shader.WallTexture = Texture = sprite;

            float startU = 0.0f;
            float startV = 0.0f;
            float endU = 1.0f;
            float endV = 1.0f;
            VertexPositionTexture[] quad = new VertexPositionTexture[4];
            quad[0] = new VertexPositionTexture(VERTICES[0], new Vector2(startU, startV));
            quad[1] = new VertexPositionTexture(VERTICES[1], new Vector2(startU, endV));
            quad[2] = new VertexPositionTexture(VERTICES[2], new Vector2(endU, endV));
            quad[3] = new VertexPositionTexture(VERTICES[3], new Vector2(endU, startV));
            Quad = quad;
        }

        public float Brightness(float x) { return Math.Min(1.0f, Math.Max(x / 4.0f, parentScene.Floor.AmbientLight)); }

        public void Draw(Matrix viewMatrix, float rotation)
        {
            CrossPlatformGame.GameInstance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Shader.World = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(new Vector3(10 * (RoomX), 0, 10 * (parentScene.Floor.MapHeight - RoomY)));
            Shader.View = viewMatrix;

            var room = parentScene.Floor.GetRoom(RoomX, RoomY);
            Shader.Brightness = new Vector4(room.Brightness(room.brightnessLevel));

            foreach (EffectPass pass in Shader.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Quad, 0, 4, INDICES, 0, 2);
            }
        }
    }
}
