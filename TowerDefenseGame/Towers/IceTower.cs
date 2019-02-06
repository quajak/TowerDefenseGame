using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefenseGame
{
    internal class IceTower : Tower
    {
        private static int _cost = 12;
        public int Damage = 0;

        public IceTower(float x, float y, bool buy = false) : base(x, y, new CircleShape(10f), _cost, "Ice Tower", "Slows down enemies in range by 50%", 150f, -10, 600f)
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
                        speed.Modifiers.Add(new Modifier(ModifierType.Percentage, Amount.Value, "SlowDown"));
                        item.Speed = speed;
                        affected.Add(item);
                    }
                }
            }
            foreach (var enemy in affected)
            {
                if (E.Distance(enemy, this) > Range.Value)
                    enemy.Speed.Modifiers.RemoveAll(m => m.name == "SlowDown");
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
}