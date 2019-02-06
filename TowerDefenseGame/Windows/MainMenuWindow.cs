using SFML.Graphics;
using System.Linq;
using System.Threading;

using static TowerDefenseGame.Program;

namespace TowerDefenseGame
{
    class MainMenuWindow : Window
    {
        MainMenuGUI mainMenu;
        public MainMenuWindow() : base()
        {
            mainMenu = new MainMenuGUI();
            Objects.Add(mainMenu);
        }

        public override Window Run(RenderWindow window)
        {
            while (window.IsOpen) //Pre game loop
            {
                window.DispatchEvents(); // Here all event handlers are called

                //Start rendering + entity updates
                window.Clear(Color.Blue);
                Objects = Objects.OrderByDescending(o => o.renderLayer).ToList();
                for (int i = 0; i < Objects.Count; i++)
                {
                    Entity item = Objects[i];
                    window.Draw(item.Update(0));
                }

                foreach (var item in Shapes)
                {
                    window.Draw(item);
                }
                foreach (var text in Texts)
                {
                    window.Draw(text);
                }
                window.Display();
                Thread.Sleep(10);
                if (gameEvent == GameEvent.Next)
                {
                    mainMenu.Delete();
                    Objects.Remove(mainMenu);
                    break;
                }
            }
            return new GameWindow(mainMenu.difficulty);
        }
    }
}
