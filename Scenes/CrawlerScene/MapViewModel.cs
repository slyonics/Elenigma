using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.CrawlerScene
{
    public class MapViewModel : ViewModel
    {
        private static readonly Dictionary<string, Animation> ACTOR_ANIMS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 128, 128, 1, 1000) },
            { "Talk", new Animation(0, 0, 128, 128, 2, 150) }
        };

        private CrawlerScene mapScene;

        private GameSprite oldActor = GameSprite.Actors_Blank;

        public MapViewModel(CrawlerScene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel)
        {
            mapScene = iScene;

            LoadView(GameView.CrawlerScene_MapView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


        }

        public override void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget)
        {
            switch (clickWidget.Name)
            {
                case "MiniMap":
                    mapScene.MiniMapClick(mouseEnd - clickWidget.AbsolutePosition);
                    break;
            }
        }

        public Image.ImageDrawFunction DrawMiniMap { get => mapScene.DrawMiniMap; }

        public ModelProperty<string> MapName { get; set; } = new ModelProperty<string>("");

        public RenderTarget2D MapRender { get => CrawlerScene.mapRender; }

        public ModelProperty<Color> MapColor { get; set; } = new ModelProperty<Color>(Color.White);
    }
}
