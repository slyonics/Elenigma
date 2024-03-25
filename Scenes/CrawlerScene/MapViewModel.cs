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
        private CrawlerScene crawlerScene;

        public MapViewModel(CrawlerScene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel)
        {
            crawlerScene = iScene;

            LoadView(GameView.CrawlerScene_MapView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


        }

        public string LocationName { get => crawlerScene.Floor.LocationName; }

        public RenderTarget2D MapRender { get => CrawlerScene.mapRender; }

        public ModelProperty<Color> MapColor { get; set; } = new ModelProperty<Color>(Color.White);
    }
}
