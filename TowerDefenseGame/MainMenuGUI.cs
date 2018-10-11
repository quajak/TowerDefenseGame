using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;

namespace TrySFML2
{
    internal class MainMenuGUI : GUI
    {
        private List<GUI> parts = new List<GUI>();
        private GuiButton difficultyButton;
        public int difficulty = 1;

        public MainMenuGUI() : base(0, 0, Program.gameSize.X, Program.gameSize.Y)
        {
            shape.FillColor = Color.White;
            shape.Texture = new Texture("./Resources/mainTitleBackground.png");
            GUIText text = new GUIText(Program.gameSize.X / 3, 200, "Tower Defense Game!", Color.Blue, 64, maxWidth: 600);
            parts.Add(text);
            difficultyButton = new GuiButton(Program.gameSize.X / 2 - 200, 300, 150, 40, new Color(0, 0, 200, 100), "Difficult: 1", "difficultyButton");
            parts.Add(difficultyButton);
            Program.objects.Add(difficultyButton);
            difficultyButton.Click += DifficultyButton_Click;
            GuiButton button = new GuiButton(Program.gameSize.X / 2 - 200, 400, 150, 40, new Color(0, 0, 200, 100), "Start Game");
            parts.Add(button);
            Program.objects.Add(button);
            button.Click += Button_Click;
        }

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            Program.gameEvent = Program.GameEvent.Next;
        }

        private void DifficultyButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == Mouse.Button.Right)
                difficulty--;
            else
                difficulty++;
            difficulty = difficulty < 1 ? 1 : difficulty;
            difficultyButton.Content = $"Difficulty: {difficulty}";
        }

        public override void Delete()
        {
            foreach (var part in parts)
            {
                part.Delete();
                if (part is GuiButton)
                {
                    Program.objects.Remove(part);
                }
            }
            base.Delete();
        }
    }
}