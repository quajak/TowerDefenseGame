using SFML.Graphics;
using SFML.System;
using System;
using System.Linq;

namespace TowerDefenseGame
{
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