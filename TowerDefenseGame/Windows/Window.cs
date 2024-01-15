using SFML.Graphics;

namespace TowerDefenseGame
{
    internal abstract class Window
    {
        public Window()
        {
        }

        public abstract Window Run(RenderWindow window);
    }
}