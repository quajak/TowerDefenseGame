using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrySFML2
{
    class MainBase : Tower
    {
        private static int instances = 0;
        public static bool Available
        {
            get {
                return instances == 0;
            }
        }

        public MainBase(int X, int Y, bool count = true) : base(X, Y, new RectangleShape(new Vector2f(30, 30)), 0)
        {
            if (count)
            {
                instances++;
                Program.mainBase = this;
            }
            if (instances == 1)
                Program.ToCreate = new MachineGun(0, 0);
            shape.FillColor = Color.Red;
        }

        public override Entity Create(int x, int y)
        {
            return new MainBase(x, y);
        }

        double time = 0;
        public override Shape Update(double timeDiff)
        {
            time -= timeDiff;
            while (time < 0)
            {
                time += 1000;
                Program.Money++;
            }

            return base.Update(timeDiff);
        }
    }

    class Tower : Entity
    {
        public int cost = 0;

        public Tower(float ax, float ay, Shape shape, int cost) : base(ax, ay, shape)
        {
            this.cost = cost;
        }
    }

    class LazorGun : Tower
    {
        static int _cost = 20;
        public static bool Available
        {
            get
            {
                return Program.Money >= _cost && !MainBase.Available;
            }
        }
        double attackTime = 0;
        double maxAttackTime = 2500;

        public LazorGun(int aX, int aY, bool buy = false) : base(aX, aY, new RectangleShape(new Vector2f(10, 10)), _cost)
        {
            renderLayer = 90; // Cheap hack so that the lazor is below the tower
            if (buy)
                Program.Money -= cost;
            shape.FillColor = Color.Green;
        }

        override public Entity Create(int x, int y)
        {
            return new LazorGun(x, y, true);
        }

        public override Shape Update(double timeDiff)
        {
            attackTime -= timeDiff;
            if (attackTime < 0)
            {
                attackTime = maxAttackTime;
                lock (Program.toChange) lock (Program.enemies)
                    {
                        var possible = from enemy in Program.enemies
                                       where E.Distance(this, enemy) < 1000
                                       select enemy;
                        var list = possible.OrderBy(x => E.Distance(this, x)).Take(10).OrderBy(e => (e as Enemy).DistanceToGoal).ToList();
                        if (list.Count != 0)
                        {
                            Entity item = list[0];
                            //Generate bullet
                            float dX = item.position.X - (position.X + 5);
                            float dY = item.position.Y - (position.Y + 5);
                            float angle = (float)Math.Atan2(dY, dX) / (float)Math.PI * 180f;
                            Program.toChange.Add(new Lazor(position.X + 5, position.Y + 5, 600, 6, angle));
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }

    class Lazor : Entity
    {
        public double time;
        public Lazor(float x, float y, float length, float width, float angle) : base(x,y, new RectangleShape(new Vector2f(length, width)) { Position = new Vector2f(x, y), Rotation = angle, FillColor = new Color(140, 14, 0 , 255) })
        {

        }

        public override Shape Update(double timeDiff)
        {
            time += timeDiff;
            if (time > 1000)
                Program.toChange.Add(this);
            return base.Update(timeDiff);
        }
    }

    class MachineGun : Tower
    {
        static int _cost = 10;
        public static bool Available
        {
            get
            {
                return Program.Money >= _cost && !MainBase.Available;
            }
        }
        double attackTime = 0;
        double maxAttackTime = 180;

        public MachineGun(int aX, int aY, bool buy = false) : base(aX, aY, new RectangleShape(new Vector2f(10, 10)), _cost)
        {
            if(buy)
                Program.Money -= cost;
            shape.FillColor = Color.Cyan;
        }

        override public Entity Create(int x, int y)
        {
            return new MachineGun(x, y, true);
        }

        public override Shape Update(double timeDiff)
        {
            attackTime -= timeDiff;
            if(attackTime < 0)
            {
                attackTime = maxAttackTime;
                lock (Program.toChange) lock (Program.enemies) 
                {
                        var possible = from enemy in Program.enemies
                                       where E.Distance(this, enemy) < 200
                                       select enemy;
                        var list = possible.OrderBy(x => E.Distance(this, x)).Take(1).ToList();
                        if(list.Count != 0)
                        {
                            Entity item = list[0];
                            //Generate bullet
                            float dX = item.position.X - (position.X + Program.random.Next(10) - 5);
                            float dY = item.position.Y - (position.Y + Program.random.Next(10) - 5);
                            float vX = dX / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.random.NextDouble() - 0.5d) / 4f ;
                            float vY = dY / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.random.NextDouble() - 0.5d) / 4f ;
                            float scale = E.Scale(vX, vY, 1300);
                            Program.toChange.Add(new Bullet(position.X, position.Y, vX * scale, vY * scale, 1300));
                        }
                }
            }
            return base.Update(timeDiff);
        }
    }

    class Bullet : Entity
    {
        private readonly float maxDistance;
        float distance = 0;
        public Bullet(float aX, float aY, float vX, float vY, float maxDistance) : base(aX, aY, vX, vY, new RectangleShape(new Vector2f(10, 2)))
        {
            shape.Origin = new Vector2f(1, 3);
            shape.Rotation = (float)(Math.Atan(vY/vX) / (2 * Math.PI) * 360);
            this.maxDistance = maxDistance;
        }

        public override Shape Update(double timeDiff)
        {
            distance += (float)Math.Sqrt(Math.Pow(velocity.X * timeDiff / 1000f, 2) + Math.Pow(velocity.Y * timeDiff / 1000f, 2));
            if ((position.X < 0 || position.X > Program.size.X) || (position.Y < 0 || position.Y > Program.size.Y) || distance > maxDistance)
            {
                Program.toChange.Add(this);
            }
            return base.Update(timeDiff);
        }
    }
}
