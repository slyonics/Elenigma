using Elenigma.Models;
using Elenigma.Scenes.StatusScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.ShopScene
{
    public class ShopScene : Scene
    {
        private ViewModel shopViewModel;

        public ShopScene(string shopName)
        {
            ShopRecord shopRecord = ShopRecord.SHOPS.First(x => x.Name == shopName);

            shopViewModel = AddView(new ShopViewModel(this, shopRecord));
        }
        public ShopScene()
        {
            shopViewModel = AddView(new SellViewModel(this));
        }

        public override void BeginScene()
        {
            sceneStarted = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (shopViewModel.Terminated) EndScene();
        }
    }
}
