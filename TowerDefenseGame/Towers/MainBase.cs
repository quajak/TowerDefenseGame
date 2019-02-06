using SFML.Graphics;
using SFML.System;

namespace TowerDefenseGame
{
    internal class MainBase : Tower
    {
        private static int instances = 0;

        public static bool Available
        {
            get
            {
                return instances == 0;
            }
        }

        public MainBase(int X, int Y, bool count = true) : base(X, Y, new RectangleShape(new Vector2f(30, 30)), 0, "Main Base", "Protect it at all costs", 0f, Program.MoneyRate)
        {
            if (count)
            {
                instances++;
                Program.mainBase = this;
            }
            if (instances == 1)
                Program.ToCreate = new MachineGun(0, 0);
            shape.Texture = new Texture("./Resources/mainBase.jpg");
        }

        public override Entity Create(int x, int y)
        {
            return new MainBase(x, y);
        }

        private double time = 0;

        public override Shape Update(double timeDiff)
        {
            time -= timeDiff;
            while (time < 0)
            {
                time += 1000;
                Program.Money += (int)Amount.Value;
            }

            return base.Update(timeDiff);
        }
    }
}