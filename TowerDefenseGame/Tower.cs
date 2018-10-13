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

    internal class Tower : Entity
    {
        public int Cost = 0;
        public string Name;
        public string Description;
        public Stat Range;
        public Stat AttackSpeed;
        public Stat Amount;
        public List<Upgrade> Installed = new List<Upgrade>();
        public List<Upgrade> AvailableUpgrades = new List<Upgrade>();
        public List<Upgrade> Locked = new List<Upgrade>();

        public Tower(float ax, float ay, Shape shape, int cost, string name, string description, float range, float amount, float attackSpeed = 0) : base(ax, ay, shape)
        {
            Cost = cost;
            Name = name;
            Description = description;
            Range = new Stat(range);
            AttackSpeed = new Stat(attackSpeed);
            Amount = new Stat(amount);
            blocking = true;
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
            Program.TowerGUI.visible = true;
            Program.TowerGUI.Selected = this;
            base.OnClick(x, y, button);
        }
    }

    internal class BankTower : Tower
    {
        private static int _cost = 20;

        public BankTower(float x, float y, bool buy = false) : base(x, y, new RectangleShape(new Vector2f(25, 25)), _cost,
            "Bank", $"Creates ${Program.MoneyRate} every 3 seconds", 0f, Program.MoneyRate, 3_000f)
        {
            if (buy)
                Program.Money -= Cost;
            shape.Texture = new Texture("./Resources/BankTower.png");
            Upgrade item = new Upgrade(new Modifier(ModifierType.Value, 1), 20, "More Money", "Increases money gain by 1", UpdateType.Amount)
            {
                Unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Value, 1), 30, "More Money II", "Increases money gain by 1", UpdateType.Amount),
                    new Upgrade(new Modifier(ModifierType.Percentage, -20), 15, "Print quicker", "Decreases time between funds by 20%", UpdateType.Speed)
                }
            };
            base.AvailableUpgrades.Add(item);
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
                Program.ToChange.Add(this);
            }
        }

        public override Entity Create(int x, int y)
        {
            return new BankTower(x, y, true);
        }

        private float moneyTime = 0;

        public override Shape Update(double timeDiff)
        {
            Description = $"Creates ${(int)Amount.Value} every {(AttackSpeed.Value / 1000f).ToString("0.0")} seconds";
            moneyTime += (float)timeDiff;
            if (moneyTime > AttackSpeed.Value)
            {
                Program.Money += (int)Amount.Value;
                moneyTime = 0;
            }
            return base.Update(timeDiff);
        }
    }

    internal class IceTower : Tower
    {
        private static int _cost = 15;
        public int Damage = 0;

        public IceTower(float x, float y, bool buy = false) : base(x, y, new CircleShape(10f), _cost, "Ice Tower", "Slows down enemies in range by 50%", 150f, -40, 600f)
        {
            if (buy)
                Program.Money -= Cost;
            shape.Texture = new Texture("./Resources/IceTower.png");
            Upgrade rangeI = new Upgrade(new Modifier(ModifierType.Percentage, 30), 5, "Range I", "Area of effect increases by 30%", UpdateType.Range)
            {
                Unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Percentage, 30), 7, "Range II", "Area of effect increases by 30%", UpdateType.Range)
                }
            };
            Upgrade damageI = new CustomUpgrade(null, 3, "Ice snap", "Will attack enemies in its range", t => (t as IceTower).Damage += 1)
            {
                Unlocks =
                {
                    new CustomUpgrade(new Modifier(ModifierType.Percentage, -10), 5, "Ice snap II", "Deals additional 2 damage", t => (t as IceTower).Damage += 2)
                    {
                        Unlocks =
                        {
                            new CustomUpgrade(null, 20, "Ice snap III", "Deals additional 10 damage", t => (t as IceTower).Damage += 10),
                            new Upgrade(new Modifier(ModifierType.Percentage, -66), 25, "Turbo Speed", "Attacks very quick", UpdateType.Speed)
                        }
                    }
                }
            };
            AvailableUpgrades.Add(damageI);
            AvailableUpgrades.Add(rangeI);
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
        float time = 0;

        public override Shape Update(double timeDiff)
        {
            lock (Program.Enemies)
            {
                affected.RemoveAll(a => !Program.Enemies.Contains(a));

                foreach (var enemy in Program.Enemies)
                {
                    if (!affected.Contains(enemy) && E.Distance(enemy, this) < Range.Value)
                    {
                        Enemy item = enemy as Enemy;
                        Stat speed = item.Speed;
                        speed.modifiers.Add(new Modifier(ModifierType.Percentage, Amount.Value, "SlowDown"));
                        item.Speed = speed;
                        affected.Add(item);
                    }
                }
            }
            foreach (var enemy in affected)
            {
                if (E.Distance(enemy, this) > Range.Value)
                    enemy.Speed.modifiers.RemoveAll(m => m.name == "SlowDown");
            }

            //Ice attack
            if(Damage != 0 && affected.Count != 0)
            {
                time -= (float)timeDiff;
                if(time < 0)
                {
                    time = AttackSpeed.Value;
                    List<Enemy> toRemove = new List<Enemy>();
                    foreach (var enemy in affected)
                    {
                        int dealtDamage = Math.Min(Damage, enemy.Size);
                        if (dealtDamage == 0)
                            throw new Exception("Something when wrong!");
                        enemy.Pop(typeof(IceTower), dealtDamage);
                        if (enemy.Size <= 0)
                            toRemove.Add(enemy);
                    }
                    toRemove.ForEach(e => affected.Remove(e));
                    lock (Program.ToChange)
                    {
                        Program.ToChange.Add(new IceCircle(position.X + 10, position.Y + 10, Range.Value));
                    }
                }
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
                Program.Money -= Cost;
            shape.Origin = new Vector2f(10, 10);
            shape.Texture = new Texture("./Resources/LazorGun.png");
            position = new Vector2f(position.X + 10, position.Y + 10);
            shape.Position = position;
            Upgrade rangeI = new Upgrade(new Modifier(ModifierType.Value, 100), 5, "Further I", "Increases range by 100", UpdateType.Range)
            {
                Unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Percentage, 20), 7, "Further II", "Increases range by 20%", UpdateType.Range)
                    {
                        Unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Value, 600), 15, "Further III", "Shoots as far as you can see!", UpdateType.Range)
                        }
                    }
                }
            };
            Upgrade speedI = new Upgrade(new Modifier(ModifierType.Percentage, -20), 5, "Faster I", "Shoots 20% quicker.", UpdateType.Speed)
            {
                Unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Percentage, -20), 10, "Faster II", "Shoots 20% quicker.", UpdateType.Speed)
                    {
                        Unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Percentage, -20), 13, "Faster III", "Shoots 20% quicker.", UpdateType.Speed)
                        }
                    }
                }
            };
            AvailableUpgrades.Add(rangeI);
            AvailableUpgrades.Add(speedI);
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
                attackTime = AttackSpeed.Value;
                lock (Program.ToChange) lock (Program.Enemies)
                    {
                        var possible = from enemy in Program.Enemies
                                       where E.Distance(this, enemy) < Range.Value
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
                            Program.ToChange.Add(new Lazor(position.X, position.Y, Range.Value, 2, angle));
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }

    internal class MachineGun : Tower
    {
        private static int _cost = 10;
        public float BulletSize = 1;

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
            position = new Vector2f(position.X + 10, position.Y + 10);
            shape.Position = position;
            if (buy)
                Program.Money -= Cost;
            shape.Texture = new Texture("./Resources/MachineGun.png");
            Upgrade pierceI = new Upgrade(new Modifier(ModifierType.Value, 1), 3, "Damage I", "Bullets do 1 damage more", UpdateType.Amount)
            {
                Unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Value, 1), 5, "Damage II", "Bullets do 1 damage more", UpdateType.Amount)
                    {
                        Unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Value, 1), 7, "Damage III", "Bullets do 1 damage more", UpdateType.Amount)
                            {
                                Unlocks =
                                {
                                    new Upgrade(new Modifier(ModifierType.Value, 3), 10, "Damage IV", "Bullets 3 damage more", UpdateType.Amount)
                                }
                            }
                        }
                    }
                }
            };
            Upgrade rangeI = new Upgrade(new Modifier(ModifierType.Value, 25), 5, "Further I", "Increases range by 25", UpdateType.Range)
            {
                Unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Value, 50), 6, "Further II", "Increases range by 50", UpdateType.Range)
                    {
                        Unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Value, 100), 15, "Further III", "Increases range by 100", UpdateType.Range)
                        }
                    }
                }
            };
            Upgrade speedI = new Upgrade(new Modifier(ModifierType.Value, -30), 2, "Faster I", "Shoots bullets 20% quicker", UpdateType.Speed)
            {
                Unlocks = {
                    new Upgrade(new Modifier(ModifierType.Value, -30), 2, "Faster II", "Shoots bullets 25% quicker", UpdateType.Speed)
                    {
                        Unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Absolute, 50), 10, "Super Fast", "Shoots 20 bullets a second", UpdateType.Speed)
                        }
                    }
                }
            };
            AvailableUpgrades.Add(pierceI);
            AvailableUpgrades.Add(rangeI);
            AvailableUpgrades.Add(speedI);
            AvailableUpgrades.Add(new CustomUpgrade(null, 5, "Bigger bullets", "The bullets are bigger", (tower) => (tower as MachineGun).BulletSize += 0.5f));
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
                attackTime = AttackSpeed.Value;
                lock (Program.ToChange) lock (Program.Enemies)
                    {
                        var possible = from enemy in Program.Enemies
                                       where E.Distance(this, enemy) < Range.Value
                                       select enemy;
                        var list = possible.OrderBy(x => E.Distance(this, x)).Take(1).ToList();
                        if (list.Count != 0)
                        {
                            Entity item = list[0];
                            //Generate bullet
                            var size = (shape as RectangleShape).Size;
                            float dX = item.position.X - (position.X + Program.Random.Next(10) - 5);
                            float dY = item.position.Y - (position.Y + Program.Random.Next(10) - 5);
                            float vX = dX / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.Random.NextDouble() - 0.5d) / 4f;
                            float vY = dY / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.Random.NextDouble() - 0.5d) / 4f;
                            float scale = E.Scale(vX, vY, 1300);
                            //Rotate the gun
                            shape.Rotation = (float)Math.Atan2(vY, vX) / (2f * (float)Math.PI) * 360f - 90f;
                            Program.ToChange.Add(new Bullet(position.X, position.Y, vX * scale, vY * scale, 1300, Amount.Value,
                                Amount.Value, BulletSize, new Vector2f(10, 2), typeof(MachineGun)));
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }
}