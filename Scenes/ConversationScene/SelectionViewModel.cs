using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.ConversationScene
{
    public class SelectionViewModel : ViewModel
    {
        private GameFont OPTION_FONT = GameFont.Dialogue;

        private ConversationScene conversationScene;

        private int selection = -1;

        public SelectionViewModel(Scene iScene, List<string> options)
            : base(iScene, PriorityLevel.MenuLevel)
        {
            conversationScene = iScene as ConversationScene;

            int longestOption = 0;
            foreach (string option in options)
            {
                AvailableOptions.Add(option);
                int optionLength = Text.GetStringLength(OPTION_FONT, option);
                if (optionLength > longestOption) longestOption = optionLength;
            }
            int width = longestOption + 16;
            ButtonSize.Value = new Rectangle(0, 0, longestOption + 6, Text.GetStringHeight(OPTION_FONT));
            LabelSize.Value = new Rectangle(0, 0, longestOption + 6, ButtonSize.Value.Height);

            int height = ButtonSize.Value.Height * options.Count() + 11;
            WindowSize.Value = new Rectangle(120 - width, 26 - height, width, height);


            LoadView(GameView.ConversationScene_SelectionView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var input = Input.CurrentInput;
            if (input.CommandPressed(Command.Confirm) && selection != -1)
            {
                Audio.PlaySound(GameSound.Cursor);
                Terminate();
            }
        }

        public override void Terminate()
        {
            //conversationScene.ConversationViewModel.Proceed();
            base.Terminate();
        }

        public void SelectOption(object parameter)
        {
            GameProfile.SetSaveData<string>("LastSelection", parameter.ToString());
            Terminate();
        }

        public ModelCollection<string> AvailableOptions { get; set; } = new ModelCollection<string>();

        public ModelProperty<Rectangle> WindowSize { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 20, 240, 60));
        public ModelProperty<Rectangle> ButtonSize { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 20, 240, 60));
        public ModelProperty<Rectangle> LabelSize { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 20, 240, 60));
    }
}
