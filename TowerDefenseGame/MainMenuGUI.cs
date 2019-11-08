using TowerDefenseGame.Maps;
using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;

namespace TowerDefenseGame
{
    internal class MainMenuGUI : GUI
    {
        private readonly List<GUI> parts = new List<GUI>();
        private readonly GuiButton difficultyButton;
        public int difficulty = 1;
        public Map chosen;
        private readonly GuiButton mapButton;

        private readonly List<Map> maps = new List<Map>
        {
            new EmptyMap(),
            new ForestMap()
        };

        private int chosenIndex = 0;

        public MainMenuGUI() : base(0, 0, Program.GameSize.X, Program.GameSize.Y)
        {
            chosen = maps[chosenIndex];
            shape.FillColor = Color.White;
            shape.Texture = new Texture("./Resources/mainTitleBackground.png");
            GUIText text = new GUIText(Program.GameSize.X / 3, 200, "Tower Defense Game!", 64, maxWidth: 600);
            parts.Add(text);
            difficultyButton = new GuiButton(Program.GameSize.X / 2 - 200, 300, 150, 40, new Color(0, 0, 200, 100), "Difficult: 1", "difficultyButton");
            parts.Add(difficultyButton);
            Program.Objects.Add(difficultyButton);
            difficultyButton.Click += DifficultyButton_Click;
            GuiButton button = new GuiButton(Program.GameSize.X / 2 - 200, 400, 150, 40, new Color(0, 0, 200, 100), "Start Game");
            parts.Add(button);
            Program.Objects.Add(button);
            button.Click += Button_Click;
            mapButton = new GuiButton(Program.GameSize.X / 2 - 200, 500, 150, 40, new Color(0, 0, 200, 100), "Map: " + chosen.name);
            parts.Add(mapButton);
            Program.Objects.Add(mapButton);
            mapButton.Click += MapButton_Click;
        }

        private void MapButton_Click(object sender, MouseButtonEventArgs e)
        {
            chosenIndex = (chosenIndex + 1) % maps.Count;
            chosen = maps[chosenIndex];
            mapButton.Content = "Map: " + chosen.name;
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
                    Program.Objects.Remove(part);
                }
            }
            base.Delete();
        }
    }
}