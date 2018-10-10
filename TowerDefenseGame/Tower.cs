﻿using SFML.Graphics;
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
            get
            {
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
            shape.Texture = new Texture("./Resources/mainBase.jpg");
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
                Program.Money += Program.moneyRate;
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
            blocking = true;
        }
    }

    class BankTower : Tower
    {
        static int _cost = 20;

        public BankTower(float x, float y, bool buy = false) : base(x, y, new RectangleShape(new Vector2f(25, 25)), _cost)
        {
            if (buy)
                Program.Money -= cost;
            shape.Texture = new Texture("./Resources/BankTower.png");
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
            return new BankTower(x, y, true);
        }

        static float maxMoneyTime = 3_000;
        float moneyTime = 0;
        public override Shape Update(double timeDiff)
        {
            moneyTime += (float)timeDiff;
            if (moneyTime > maxMoneyTime)
            {
                Program.Money += Program.moneyRate;
                moneyTime = 0;
            }
            return base.Update(timeDiff);
        }
    }

    class IceTower : Tower
    {
        static int _cost = 15;

        public IceTower(float x, float y, bool buy = false) : base(x, y, new CircleShape(10f), _cost)
        {
            if (buy)
                Program.Money -= cost;
            shape.Texture = new Texture("./Resources/IceTower.png");
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
            return new IceTower(x, y, true);
        }

        static float range = 150;
        List<Enemy> affected = new List<Enemy>();
        Modifier slowDown = new Modifier(ModifierType.Percentage, -40);
        public override Shape Update(double timeDiff)
        {
            lock (Program.enemies)
            {

                foreach (var enemy in Program.enemies)
                {
                    if (!affected.Contains(enemy) && E.Distance(enemy, this) < range)
                    {
                        Enemy item = enemy as Enemy;
                        Stat speed = item.Speed;
                        speed.modifiers.Add(slowDown);
                        item.Speed = speed;
                        affected.Add(item);
                    }
                }
            }
            foreach (var enemy in affected)
            {
                if (E.Distance(enemy, this) > range)
                    enemy.Speed.modifiers.Remove(slowDown);
            }
            return base.Update(timeDiff);
        }
    }

    class Bomb : Tower
    {
        static int _cost = 1;
        static float maxRadius = 30;
        float radius = 1;
        float growth = 0.03f;
        float timeAtMax = 0;
        static float maxTimeAtMax = 800; //In milliseconds
        public Bomb(float x, float y, bool buy = false) : base(x, y, new CircleShape(3), _cost)
        {
            renderLayer = 90; // Cheap hack so that the lazor is below the tower
            if (buy)
                Program.Money -= cost;
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
                    Program.toChange.Add(this);
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

        public LazorGun(int aX, int aY, bool buy = false) : base(aX, aY, new RectangleShape(new Vector2f(20, 20)), _cost)
        {
            renderLayer = 90; // Cheap hack so that the lazor is below the tower
            if (buy)
                Program.Money -= cost;
            shape.Origin = new Vector2f(5, 5);
            shape.Texture = new Texture("./Resources/LazorGun.png");
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
                            float dX = item.position.X - position.X;
                            float dY = item.position.Y - position.Y;
                            float angle = (float)Math.Atan2(dY, dX) / (float)Math.PI * 180f;
                            shape.Rotation = angle - 90f;
                            Program.toChange.Add(new Lazor(position.X, position.Y, 600, 2, angle));
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }

    class Lazor : Entity
    {
        public double time;
        public Lazor(float x, float y, float length, float width, float angle) : base(x, y, new RectangleShape(new Vector2f(length, width)) { Position = new Vector2f(x, y), Rotation = angle, FillColor = new Color(140, 14, 0, 255) })
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

        public MachineGun(int aX, int aY, bool buy = false) : base(aX, aY, new RectangleShape(new Vector2f(20, 20)), _cost)
        {
            shape.Origin = new Vector2f(5, 5);
            if (buy)
                Program.Money -= cost;
            shape.Texture = new Texture("./Resources/MachineGun.png");
        }

        override public Entity Create(int x, int y)
        {
            return new MachineGun(x, y, true);
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
                                       where E.Distance(this, enemy) < 200
                                       select enemy;
                        var list = possible.OrderBy(x => E.Distance(this, x)).Take(1).ToList();
                        if (list.Count != 0)
                        {
                            Entity item = list[0];
                            //Generate bullet
                            var size = (shape as RectangleShape).Size;
                            float dX = item.position.X  - (position.X + Program.random.Next(10) - 5);
                            float dY = item.position.Y - (position.Y + Program.random.Next(10) - 5);
                            float vX = dX / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.random.NextDouble() - 0.5d) / 4f;
                            float vY = dY / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.random.NextDouble() - 0.5d) / 4f;
                            float scale = E.Scale(vX, vY, 1300);
                            //Rotate the gun
                            shape.Rotation = (float)Math.Atan2(vY, vX) / (2f * (float)Math.PI) * 360f - 90f;
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
            shape.Rotation = (float)(Math.Atan(vY / vX) / (2 * Math.PI) * 360);
            this.maxDistance = maxDistance;
        }

        public override Shape Update(double timeDiff)
        {
            distance += (float)Math.Sqrt(Math.Pow(velocity.X * timeDiff / 1000f, 2) + Math.Pow(velocity.Y * timeDiff / 1000f, 2));
            if ((position.X < 0 || position.X > Program.gameSize.X) || (position.Y < 0 || position.Y > Program.gameSize.Y) || distance > maxDistance)
            {
                Program.toChange.Add(this);
            }
            return base.Update(timeDiff);
        }
    }
}