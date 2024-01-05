using Elenigma.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Elenigma.SceneObjects.Widgets
{
    public class RadioBox : DataGrid
    {
        bool initializing = true;
        int selectionIndex = -1;

        public int Selection
        {
            get => selectionIndex;

            set
            {
                if (value == -1 && selectionIndex != -1) ((RadioButton)ChildList[selectionIndex]).UnSelect();
                selectionIndex = value;
            }
        }

        private NinePatch panelFrame;

        private string style;
        private string Style { get => style; set { style = value; UpdateFrame(); } }

        public MethodInfo OnSelect { get; set; }

        protected GameSound CursorSound { get; set; } = GameSound.Cursor;

        protected GameSound SelectSound { get; set; } = GameSound.Selection;

        public RadioBox(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            UpdateFrame();
        }

        protected override void ItemsChanged()
        {
            base.ItemsChanged();
            if (selectionIndex >= ChildList.Count) selectionIndex = ChildList.Count - 1;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (CrossPlatformGame.TransitionShader != null) return;

            if (initializing) { initializing = false; return; }
            if (Terminated || !Enabled || !Visible || Transitioning || parent.Transitioning) return;

            var input = Input.CurrentInput;
            if (input.CommandPressed(Command.Up)) CursorUp();
            else if (input.CommandPressed(Command.Down)) CursorDown();
            else if (input.CommandPressed(Command.Confirm) && Selection != -1)
            {
                if (SelectSound != GameSound.None) Audio.PlaySound(SelectSound);
                Activate();
            }
        }

        private void UpdateFrame()
        {
            if (style != null)
            {
                if (panelFrame == null) panelFrame = new NinePatch(style, Depth);
                panelFrame.Bounds = currentWindow;
            }
        }

        public override void LoadChildren(XmlNodeList nodeList, float widgetDepth)
        {
            base.LoadChildren(nodeList, widgetDepth);

            if (!Input.MOUSE_MODE && ChildList.Count > 0)
            {
                selectionIndex = 0;
                (ChildList[selectionIndex] as RadioButton).RadioSelect();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            panelFrame?.Draw(spriteBatch, Position + scrollOffset);
        }

        public void CursorUp()
        {
            if (ChildList.Count() == 0) return;

            if (selectionIndex == -1) selectionIndex = 0;
            else if (selectionIndex > 0) selectionIndex--;
            else return;

            if (CursorSound != GameSound.None) Audio.PlaySound(CursorSound);

            (ChildList[selectionIndex] as RadioButton).RadioSelect();
            if (!IsChildVisible(ChildList[selectionIndex])) ScrollUp();

            object[] parameters = new object[] { selectionIndex };
            OnSelect?.Invoke(GetParent<ViewModel>(), parameters);
        }

        public void CursorDown()
        {
            if (ChildList.Count() == 0) return;

            if (selectionIndex == -1) selectionIndex = 0;
            else if (selectionIndex < ChildList.Count() - 1) selectionIndex++;
            else return;

            if (CursorSound != GameSound.None) Audio.PlaySound(CursorSound);

            (ChildList[selectionIndex] as RadioButton).RadioSelect();
            if (!IsChildVisible(ChildList[selectionIndex])) ScrollDown();

            object[] parameters = new object[] { selectionIndex };
            OnSelect?.Invoke(GetParent<ViewModel>(), parameters);
        }

        public void Activate()
        {
            (ChildList[selectionIndex] as RadioButton).Activate();
        }
    }
}
