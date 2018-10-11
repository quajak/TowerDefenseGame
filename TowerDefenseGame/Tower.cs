using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrySFML2
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

        public MainBase(int X, int Y, bool count = true) : base(X, Y, new RectangleShape(new Vector2f(30, 30)), 0, "Main Base", "Protect it at all costs", 0f, Program.moneyRate)
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
                Program.Money += (int)amount.Value;
            }

            return base.Update(timeDiff);
        }
    }

    internal class Tower : Entity
    {
        public int cost = 0;
        public string name;
        public string description;
        public Stat range;
        public Stat attackSpeed;
        public Stat amount;
        public List<Upgrade> installed = new List<Upgrade>();
        public List<Upgrade> available = new List<Upgrade>();
        public List<Upgrade> locked = new List<Upgrade>();

        public Tower(float ax, float ay, Shape shape, int cost, string name, string description, float range, float amount, float attackSpeed = 0) : base(ax, ay, shape)
        {
            this.cost = cost;
            this.name = name;
            this.description = description;
            this.range = new Stat(range);
            this.attackSpeed = new Stat(attackSpeed);
            this.amount = new Stat(amount);
            blocking = true;
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
            Program.towerGUI.visible = true;
            Program.towerGUI.selected = this;
            base.OnClick(x, y, button);
        }
    }

    internal class BankTower : Tower
    {
        private static int _cost = 20;

        public BankTower(float x, float y, bool buy = false) : base(x, y, new RectangleShape(new Vector2f(25, 25)), _cost,
            "Bank", $"Creates ${Program.moneyRate} every 3 seconds", 0f, Program.moneyRate, 3_000f)
        {
            if (buy)
                Program.Money -= cost;
            shape.Texture = new Texture("./Resources/BankTower.png");
            Upgrade item = new Upgrade(new Modifier(ModifierType.Value, 1), 20, "More Money", "Increases money gain by 1", UpdateType.Amount)
            {
                unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Value, 1), 30, "More Money II", "Increases money gain by 1", UpdateType.Amount),
                    new Upgrade(new Modifier(ModifierType.Percentage, -33), 10, "Print quicker", "Decreases time between funds by 33%", UpdateType.Speed)
                }
            };
            available.Add(item);
            Collides.Add(typeof(Enemy));
        }

        public static bool Available
        {
            get
            {
                return Program.Money >= _cost && !MainBase.Available;
            }
        }

        public override void Collision(Entity collided)
        {
            if (collided is Enemy e)
            {
                Program.toChange.Add(this);
            }
        }

        public override Entity Create(int x, int y)
        {
            return new BankTower(x, y, true);
        }

        private float moneyTime = 0;

        public override Shape Update(double timeDiff)
        {
            description = $"Creates ${(int)amount.Value} every 3 seconds";
            moneyTime += (float)timeDiff;
            if (moneyTime > attackSpeed.Value)
            {
                Program.Money += (int)amount.Value;
                moneyTime = 0;
            }
            return base.Update(timeDiff);
        }
    }

    internal class IceTower : Tower
    {
        private static int _cost = 15;

        public IceTower(float x, float y, bool buy = false) : base(x, y, new CircleShape(10f), _cost, "Ice Tower", "Slows down enemies in range by 50%", 150f, -40)
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

        private List<Enemy> affected = new List<Enemy>();

        public override Shape Update(double timeDiff)
        {
            lock (Program.enemies)
            {
                foreach (var enemy in Program.enemies)
                {
                    if (!affected.Contains(enemy) && E.Distance(enemy, this) < range.Value)
                    {
                        Enemy item = enemy as Enemy;
                        Stat speed = item.Speed;
                        speed.modifiers.Add(new Modifier(ModifierType.Percentage, amount.Value, "SlowDown"));
                        item.Speed = speed;
                        affected.Add(item);
                    }
                }
            }
            foreach (var enemy in affected)
            {
                if (E.Distance(enemy, this) > range.Value)
                    enemy.Speed.modifiers.RemoveAll(m => m.name == "SlowDown");
            }
            return base.Update(timeDiff);
        }
    }

    internal class Bomb : Tower
    {
        private static int _cost = 1;
        private static float maxRadius = 30;
        private float radius = 1;
        private float growth = 0.03f;
        private float timeAtMax = 0;
        private static float maxTimeAtMax = 800; //In milliseconds

        public Bomb(float x, float y, bool buy = false) : base(x, y, new CircleShape(3), _cost, "Bomb", "Kills enemy while exploding", 0f, 0f)
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

        public override void OnClick(int x, int y, Mouse.Button button)
        {
        }
    }

    internal class LazorGun : Tower
    {
        private static int _cost = 20;

        public static bool Available
        {
            get
            {
                return Program.Money >= _cost && !MainBase.Available;
            }
        }

        private double attackTime = 0;

        public LazorGun(int aX, int aY, bool buy = false) : base(aX, aY, new RectangleShape(new Vector2f(20, 20)), _cost, "Lazor Gun", "Shoots lazors", 400f, 0f, 2_500f)
        {
            renderLayer = 100; // Cheap hack so that the lazor is below the tower
            if (buy)
                Program.Money -= cost;
            shape.Origin = new Vector2f(10, 10);
            shape.Texture = new Texture("./Resources/LazorGun.png");
            Upgrade rangeI = new Upgrade(new Modifier(ModifierType.Value, 100), 5, "Further I", "Increases range by 100", UpdateType.Range)
            {
                unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Percentage, 20), 7, "Further II", "Increases range by 20%", UpdateType.Range)
                    {
                        unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Value, 600), 15, "Further III", "Shoots as far as you can see!", UpdateType.Range)
                        }
                    }
                }
            };
            Upgrade speedI = new Upgrade(new Modifier(ModifierType.Percentage, -20), 5, "Faster I", "Shoots 20% quicker.", UpdateType.Speed)
            {
                unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Percentage, -20), 10, "Faster II", "Shoots 20% quicker.", UpdateType.Speed)
                    {
                        unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Percentage, -20), 13, "Faster III", "Shoots 20% quicker.", UpdateType.Speed)
                        }
                    }
                }
            };
            available.Add(rangeI);
            available.Add(speedI);
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
                attackTime = attackSpeed.Value;
                lock (Program.toChange) lock (Program.enemies)
                    {
                        var possible = from enemy in Program.enemies
                                       where E.Distance(this, enemy) < range.Value
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
                            Program.toChange.Add(new Lazor(position.X, position.Y, range.Value, 2, angle));
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }

    internal class Lazor : Entity
    {
        public double time;

        public Lazor(float x, float y, float length, float width, float angle) : base(x, y, new RectangleShape(new Vector2f(length, width)) { Position = new Vector2f(x, y), Rotation = angle, FillColor = new Color(140, 14, 0, 255) })
        {
            renderLayer = 3;
        }

        public override Shape Update(double timeDiff)
        {
            time += timeDiff;
            if (time > 1000)
                Program.toChange.Add(this);
            return base.Update(timeDiff);
        }
    }

    internal class MachineGun : Tower
    {
        private static int _cost = 10;

        public static bool Available
        {
            get
            {
                return Program.Money >= _cost && !MainBase.Available;
            }
        }

        private double attackTime = 0;

        public MachineGun(int aX, int aY, bool buy = false) : base(aX, aY, new RectangleShape(new Vector2f(20, 20)), _cost, "Machine Gun",
            "Shoots bullets very quickly", 200f, 1f, 150f)
        {
            shape.Origin = new Vector2f(10, 10);
            if (buy)
                Program.Money -= cost;
            shape.Texture = new Texture("./Resources/MachineGun.png");
            Upgrade pierceI = new Upgrade(new Modifier(ModifierType.Value, 1), 3, "Pierce I", "Bullets do 1 damage more", UpdateType.Amount)
            {
                unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Value, 1), 5, "Pierce II", "Bullets do 1 damage more", UpdateType.Amount)
                    {
                        unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Value, 1), 7, "Pierce III", "Bullets do 1 damage more", UpdateType.Amount)
                            {
                                unlocks =
                                {
                                    new Upgrade(new Modifier(ModifierType.Value, 3), 10, "Pierce IV", "Bullets 3 damage more", UpdateType.Amount)
                                }
                            }
                        }
                    }
                }
            };
            Upgrade rangeI = new Upgrade(new Modifier(ModifierType.Value, 50), 5, "Further I", "Increases range by 100", UpdateType.Range)
            {
                unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Value, 50), 6, "Further II", "Increases range by 50", UpdateType.Range)
                    {
                        unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Value, 100), 10, "Further III", "Increases range by 100", UpdateType.Range)
                        }
                    }
                }
            };
            Upgrade speedI = new Upgrade(new Modifier(ModifierType.Value, -30), 2, "Faster I", "Shoots bullets 20% quicker", UpdateType.Speed)
            {
                unlocks = {
                    new Upgrade(new Modifier(ModifierType.Value, -30), 2, "Faster II", "Shoots bullets 25% quicker", UpdateType.Speed)
                    {
                        unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Absolute, 50), 10, "Super Fast", "Shoots 20 bullets a second", UpdateType.Speed)
                        }
                    }
                }
            };
            available.Add(pierceI);
            available.Add(rangeI);
            available.Add(speedI);
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
                attackTime = attackSpeed.Value;
                lock (Program.toChange) lock (Program.enemies)
                    {
                        var possible = from enemy in Program.enemies
                                       where E.Distance(this, enemy) < range.Value
                                       select enemy;
                        var list = possible.OrderBy(x => E.Distance(this, x)).Take(1).ToList();
                        if (list.Count != 0)
                        {
                            Entity item = list[0];
                            //Generate bullet
                            var size = (shape as RectangleShape).Size;
                            float dX = item.position.X - (position.X + Program.random.Next(10) - 5);
                            float dY = item.position.Y - (position.Y + Program.random.Next(10) - 5);
                            float vX = dX / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.random.NextDouble() - 0.5d) / 4f;
                            float vY = dY / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.random.NextDouble() - 0.5d) / 4f;
                            float scale = E.Scale(vX, vY, 1300);
                            //Rotate the gun
                            shape.Rotation = (float)Math.Atan2(vY, vX) / (2f * (float)Math.PI) * 360f - 90f;
                            Program.toChange.Add(new Bullet(position.X, position.Y, vX * scale, vY * scale, 1300, amount.Value));
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }

    internal class Bullet : Entity
    {
        private readonly float maxDistance;
        public readonly float damage;
        private float distance = 0;

        public Bullet(float aX, float aY, float vX, float vY, float maxDistance, float damage) : base(aX, aY, vX, vY, new RectangleShape(new Vector2f(10, 2)))
        {
            shape.Origin = new Vector2f(1, 3);
            shape.Rotation = (float)(Math.Atan(vY / vX) / (2 * Math.PI) * 360);
            this.maxDistance = maxDistance;
            this.damage = damage;
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