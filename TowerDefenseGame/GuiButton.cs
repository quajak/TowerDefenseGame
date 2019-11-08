using SFML.Graphics;
using SFML.Window;
using System;

namespace TowerDefenseGame
{
    internal class GuiButton : GUI
    {
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                text.Content = content;
            }
        }

        private readonly GUIText text;
        private string content;

        public GuiButton(float x, float y, float width, float height, Color color, string content, string name = "") : base(x, y, width, height, name)
        {
            shape.FillColor = color;
            renderLayer = 10;
            clickLayer = 1;
            text = new GUIText(x + 10, y + 10, content);
            this.content = content;
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
            MouseButtonEventArgs args = new MouseButtonEventArgs(new MouseButtonEvent() { X = x, Y = y, Button = button });
            Click.Invoke(this, args);
        }

        public override void Delete()
        {
            text.Delete();
            base.Delete();
        }

        public event EventHandler<MouseButtonEventArgs> Click;
    }
}