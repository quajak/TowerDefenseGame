using SFML.Graphics;
using SFML.Window;
using System;

namespace TowerDefenseGame
{
    internal class Bomb : Tower
    {
        private static int _cost = 1;
        private static float maxRadius = 30;
        private float radius = 1;
        private float growth = 0.03f;
        private float timeAtMax = 0;
        private static float maxTimeAtMax = 700; //In milliseconds

        public Bomb(float x, float y, bool buy = false) : base(x, y, new CircleShape(3), _cost, "Bomb", "Kills enemy while exploding", 0f, 0f)
        {
            renderLayer = 90; // Cheap hack so that the lazor is below the tower
            if (buy)
                Program.Money -= Cost;
            shape.FillColor = Color.Black;
        }

        public static bool Available
        {
            get
            {
                return Program.Money >= _cost && !MainBase.Available;
            }
        }

        public override Entity Create(int x, int y)
        {
            return new Bomb(x, y, true);
        }

        public override Shape Update(double timeDiff)
        {
            if (radius > maxRadius)
            {
                if (timeAtMax > maxTimeAtMax)
                    Program.ToChange.Add(this);
                timeAtMax += (float)timeDiff;
            }
            else
            {
                float diff = growth * (float)timeDiff;
                radius += diff;
                (shape as CircleShape).Radius = radius;
                position.X -= diff;
                position.Y -= diff;
                shape.Position = position;
            }
            return base.Update(timeDiff);
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
        }
    }

    internal class Mine : Tower
    {
        private static int _cost = 2;
        private static float maxTimeAtMax = 800; //In milliseconds
        private int damageToDo = 20;

        public Mine(float x, float y, bool buy = false) : base(x, y, new CircleShape(10), _cost, "Mine", "Does 20 damage", 0f, 0f)
        {
            renderLayer = 90;
            if (buy)
                Program.Money -= Cost;
            shape.FillColor = new Color(100, 100, 100);
        }

        public static bool Available
        {
            get
            {
                return Program.Money >= _cost && !MainBase.Available;
            }
        }

        public int DamageToDo
        {
            get => damageToDo; set
            {
                damageToDo = value;
                if(damageToDo == 0)
                {
                    int size = 80;
                    lock (Program.ToChange)
                    {
                        Program.ToChange.Add(this); 
                        Program.ToChange.Add(new Explosion(position.X - size / 2, position.Y - size / 2,
                                        size, 3));
                    }
                } else if(damageToDo < 0)
                {
                    throw new Exception("Damage still to be dealt can not be negative!");
                }
            }
        }

        public override Entity Create(int x, int y)
        {
            return new Mine(x, y, true);
        }

        public override Shape Update(double timeDiff)
        {
            return base.Update(timeDiff);
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
        }
    }
}