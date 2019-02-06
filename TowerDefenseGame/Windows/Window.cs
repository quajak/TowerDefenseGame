using SFML.Graphics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefenseGame
{
    abstract class Window
    {
        public Window()
        {

        }

        public abstract Window Run(RenderWindow window);
    }
}
