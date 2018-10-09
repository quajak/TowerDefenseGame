using SFML.Graphics;
using SFML.System;

namespace TrySFML2
{
    class Enemy : Entity
    {
        int size;
        public float DistanceToGoal
        {
            get
            {
                if (!MainBase.Available)
                {
                    return E.Distance(this, Program.mainBase);
                }
                return 100f;
            }
        }

        public int Size
        {
            get => size; set
            {
                size = value;
                if(size != 0)
                {
                    var s = shape as RectangleShape;
                    s.Size = new Vector2f(size * 5, size * 5);
                    shape = s;
                    shape.FillColor = new Color((byte)Program.random.Next(255), (byte)Program.random.Next(255), (byte)Program.random.Next(255));
                }
            }
        }

        public Enemy(int aX, int aY, int size = 1) : base(aX, aY, new RectangleShape(new Vector2f(5, 5)))
        {
            float vX;
            float vY;
            if (Program.mainBase is null)
            {
                vX = (float)Program.random.NextDouble() - 0.5f;
                vY = (float)Program.random.NextDouble() - 0.5f;
            }
            else
            {
                vX = Program.mainBase.position.X - aX;
                vY = Program.mainBase.position.Y - aY;
            }
            float speed = 20 * Program.EvolutionFactor;
            float s = E.Scale(vX, vY, speed);
            velocity = new Vector2f(s * vX, s * vY);
            Size = size;
            Collides.Add(typeof(Bullet));
            Collides.Add(typeof(MainBase));
            Collides.Add(typeof(Lazor));
        }

        public override void Collision(Entity collided)
        {
            switch (collided)
            {
                case Bullet b:
                    lock (Program.toChange)
                    {
                        Program.toChange.Add(b);
                    }
                    Pop();
                    break;
                case MainBase m:
                    Program.GameEnded = true;
                    break;
                case Lazor l:
                    Pop();
                    break;
                case Entity e:

                default:
                    break;
            }
        }

        public void Pop(int amount = 1)
        {
            Size -= amount;
            if(size == 0)
            {
                lock (Program.toChange)
                {
                    Program.toChange.Add(this);
                    Program.enemies.Remove(this);
                }
                Program.EnemiesKilled++;
            }
        }

        public override Shape Update(double timeDiff)
        {
            if((position.X < 0 || position.X > Program.gameSize.X) || ( position.Y < 0 || position.Y > Program.gameSize.Y))
            {
                Program.toChange.Add(this);
                Program.enemies.Remove(this);
            }
            return base.Update(timeDiff);
        }
    }
}
