using SFML.Graphics;
using SFML.System;

namespace TrySFML2
{
    internal class Enemy : Entity
    {
        private Stat vX;
        private Stat vY;
        private int size;
        private bool colorSet = false;
        private Stat speed;

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
                if (size != 0)
                {
                    var s = shape as RectangleShape;
                    s.Size = new Vector2f(size * 2 + 5, size * 2 + 5);
                    shape = s;
                    if (!colorSet)
                    {
                        shape.FillColor = new Color((byte)Program.random.Next(255), (byte)Program.random.Next(255), (byte)Program.random.Next(255));
                    }
                }
            }
        }

        internal Stat VX
        {
            get => vX; set
            {
                vX = value;
                if (speed != null && vY != null)
                {
                    float s = E.Scale(value.baseValue, vY.Value, Speed.Value);
                    vY.baseValue = value.baseValue * s;
                    vX.baseValue = vX.baseValue * s;
                    velocity = new Vector2f(vX.Value, vY.Value);
                }
            }
        }

        internal Stat VY
        {
            get => vY; set
            {
                vY = value;
                if (speed != null && vX != null)
                {
                    float s = E.Scale(vY.baseValue, value.baseValue, Speed.Value);
                    vY.baseValue = value.baseValue * s;
                    vX.baseValue = vX.baseValue * s;
                    velocity = new Vector2f(vX.Value, vY.Value);
                }
            }
        }

        internal Stat Speed
        {
            get => speed; set
            {
                speed = value;
                if (vX != null && vY != null)
                {
                    float s = E.Scale(vX.baseValue, vY.Value, speed.Value);
                    vY.baseValue = vY.baseValue * s;
                    vX.baseValue = vX.baseValue * s;
                    velocity = new Vector2f(vX.Value, vY.Value);
                }
            }
        }

        public Enemy(int aX, int aY, int size = 1) : base(aX, aY, new RectangleShape(new Vector2f(5, 5)))
        {
            if (Program.mainBase is null)
            {
                VX = new Stat((float)Program.random.NextDouble() - 0.5f);
                VY = new Stat((float)Program.random.NextDouble() - 0.5f);
            }
            else
            {
                VX = new Stat(Program.mainBase.position.X - aX);
                VY = new Stat(Program.mainBase.position.Y - aY);
            }
            Speed = new Stat(20 * Program.EvolutionFactor);
            float s = E.Scale(VX.Value, VY.Value, Speed.Value);
            VX.baseValue *= s;
            VY.baseValue *= s;
            velocity = new Vector2f(VX.Value, VY.Value);
            Size = size;
            Collides.Add(typeof(Bullet));
            Collides.Add(typeof(MainBase));
            Collides.Add(typeof(Lazor));
            Collides.Add(typeof(Bomb));
        }

        public Enemy(int aX, int aY, Color color, int size = 1) : this(aX, aY, size)
        {
            colorSet = true;
            shape.FillColor = color;
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
                    Pop((int)b.damage);
                    break;

                case MainBase m:
                    Program.GameEnded = true;
                    break;

                case Lazor l:
                    Pop();
                    break;

                case Bomb b:
                    Pop();
                    break;

                default:
                    break;
            }
        }

        public void Pop(int amount = 1)
        {
            Size -= amount;
            if (size <= 0)
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
            if ((position.X < 0 || position.X > Program.gameSize.X) || (position.Y < 0 || position.Y > Program.gameSize.Y))
            {
                Program.toChange.Add(this);
                Program.enemies.Remove(this);
            }
            return base.Update(timeDiff);
        }
    }
}