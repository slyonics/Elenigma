using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.SplashScene
{
    public class SplashScene : Scene, ISkippableWait
    {
        private Texture2D splashSprite = AssetCache.SPRITES[GameSprite.Background_Splash];

        public SplashScene()
            : base()
        {
            AddController(new SkippableWaitController(PriorityLevel.MenuLevel, this));
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(splashSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), new Rectangle(0, 0, 1, 1), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(splashSprite, new Rectangle((CrossPlatformGame.ScreenWidth - splashSprite.Width) / 2, (CrossPlatformGame.ScreenHeight - splashSprite.Height) / 2, splashSprite.Width, splashSprite.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
        }

        public void Notify(SkippableWaitController sender)
        {
            GameProfile.NewState();

            if (GameProfile.SaveList.Count > 0)
            {
                CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
                //CrossPlatformGame.Transition(typeof(TitleScene.TitleScene), "True");
            }
            else
            {
                NewGame();
            }
        }

        public static void NewGame()
        {
            GameProfile.NewState();

            GameProfile.AddInventory("Tonic", 13);
            GameProfile.AddInventory("Ether", 6);

            var fool1 = new HeroModel(HeroType.Envi, ClassType.Fool, 13, BattleCommand.Magic);
            fool1.Equip("Huge Mallet");
            fool1.Equip("Motley");
            fool1.Abilities.Add(AbilityRecord.ABILITIES.First(x => x.Name == "Eruption"));
            fool1.Abilities.Add(AbilityRecord.ABILITIES.First(x => x.Name == "Life Sprinkle"));
            fool1.Abilities.Add(AbilityRecord.ABILITIES.First(x => x.Name == "Health Shower"));
            GameProfile.PlayerProfile.Party.Add(fool1);

            var fool2 = new HeroModel(HeroType.Sparr, ClassType.Fool, 13, BattleCommand.Magic);
            fool2.Equip("Huge Mallet");
            fool2.Equip("Motley");
            fool2.Abilities.Add(AbilityRecord.ABILITIES.First(x => x.Name == "Icefall"));
            fool2.Abilities.Add(AbilityRecord.ABILITIES.First(x => x.Name == "Life Sprinkle"));
            fool2.Abilities.Add(AbilityRecord.ABILITIES.First(x => x.Name == "Health Shower"));
            GameProfile.PlayerProfile.Party.Add(fool2);

            CrossPlatformGame.Transition(typeof(MapScene.MapScene), GameMap.Tower, 19, 33);
            //CrossPlatformGame.Transition(typeof(MapScene.MapScene), GameMap.Tower, 19, 30);
            //CrossPlatformGame.Transition(typeof(MapScene.MapScene), GameMap.Tower2, 19, 11);
            //CrossPlatformGame.Transition(typeof(MapScene.MapScene), GameMap.Tower2, 19, 26);
        }

        public bool Terminated { get => false; }
    }
}
