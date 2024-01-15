using SFML.Graphics;
using SFML.System;
using System.Threading;

using static TowerDefenseGame.Program;

namespace TowerDefenseGame
{
    internal class GameOverWindow : Window, System.IDisposable
    {
        private readonly Text gameOver;
        private readonly Sprite background;

        public GameOverWindow() : base()
        {
            Statistics.WriteStats();
            //After game is over code
            gameOver = new Text("Game Over!", Program.Font, 64)
            {
                Position = new Vector2f(GameSize.X / 2 - 100, GameSize.Y / 2 - 64),
                Color = Color.Red
            };

            //Setup background graphics
            var t = new Texture("./Resources/background.png");
            background = new Sprite(t);
            t.Dispose();
            background.Texture.Repeated = true;
            background.TextureRect = new IntRect(0, 0, (int)GameSize.X, (int)GameSize.Y);
        }

        public override Window Run(RenderWindow window)
        {
            window.Display();
            while (window.IsOpen)
            {
                Thread.Sleep(10);
                window.DispatchEvents();
                window.Clear(Color.Blue);
                window.Draw(background);
                for (int i = 0; i < Objects.Count; i++)
                {
                    Entity item = Objects[i];
                    window.Draw(item.Update(0));
                }
                window.Draw(gameOver);
                window.Display();
            }
            return null;
        }

        public void Dispose()
        {
            gameOver.Dispose();
            background.Dispose();
        }
    }
}