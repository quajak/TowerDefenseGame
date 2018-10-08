using SFML.Graphics;
using SFML.System;

namespace TrySFML2
{
    class Enemy : Entity
    {
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

        public Enemy(int aX, int aY) : base(aX, aY, new RectangleShape(new Vector2f(5, 5)))
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
                        Program.toChange.Add(this);
                        Program.enemies.Remove(this);
                    }
                    Program.EnemiesKilled++;
                    break;
                case MainBase m:
                    Program.GameEnded = true;
                    break;
                case Lazor l:
                    lock (Program.toChange)
                    {
                        Program.toChange.Add(this);
                        Program.enemies.Remove(this);
                    }
                    Program.EnemiesKilled++;
                    break;
                default:
                    break;
            }
        }

        public override Shape Update(double timeDiff)
        {
            if((position.X < 0 || position.X > Program.size.X) || ( position.Y < 0 || position.Y > Program.size.Y))
            {
                Program.toChange.Add(this);
                Program.enemies.Remove(this);
            }
            return base.Update(timeDiff);
        }
    }
}
