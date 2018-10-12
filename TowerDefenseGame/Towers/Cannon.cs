using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;

namespace TrySFML2
{
    internal class Cannon : Tower
    {
        private static int _cost = 8;

        public int ExtraPierce = 0;
        public Stat ExplosionSize = new Stat(60);
        public int ExplosionDamage = 1;

        public Cannon(float x, float y, bool buy = false) : base(x, y, new RectangleShape(new Vector2f(20, 20)), _cost, "Cannon", "Shoot bullets which explode",
            200, 2, 400)
        {
            if (buy)
                Program.Money -= _cost;
            shape.Texture = new Texture("./Resources/cannon.png");
            shape.Origin = new Vector2f(10, 10);
            CustomUpgrade pierce = new CustomUpgrade(null, 5, "Pierce I", "Each shell has 1 more pierce", (t) => (t as Cannon).ExtraPierce += 1)
            {
                Unlocks =
                {
                     new CustomUpgrade(null, 12, "Pierce II", "Each shell has 2 more pierce", (t) => (t as Cannon).ExtraPierce += 2)
                     {
                         Unlocks =
                         {
                              new CustomUpgrade(null, 20, "Pierce III", "Each shell has 3 more pierce", (t) => (t as Cannon).ExtraPierce += 3)
                         }
                     }
                }
            };
            CustomUpgrade explosionSize = new CustomUpgrade(null, 4, "Explosions I", "Exposions are 50% stronger",
                t => (t as Cannon).ExplosionSize.modifiers.Add(new Modifier(ModifierType.Percentage, 50)))
            {
                Unlocks =
                {
                    new CustomUpgrade(null, 4, "Explosions II", "Exposions deal 3 more damage",
                t => (t as Cannon).ExplosionDamage += 3)
                }
            };
            Upgrade rangeI = new Upgrade(new Modifier(ModifierType.Value, 50), 8, "Further I", "Increases range by 50", UpdateType.Range)
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
            available.Add(pierce);
            available.Add(explosionSize);
            available.Add(rangeI);
        }

        public override Entity Create(int x, int y)
        {
            return new Cannon(x, y, true);
        }

        public static bool Available
        {
            get
            {
                return _cost <= Program.Money && !MainBase.Available;
            }
        }

        private float attackTime = 0;

        public override Shape Update(double timeDiff)
        {
            attackTime -= (float)timeDiff;
            if (attackTime < 0)
            {
                attackTime = attackSpeed.Value;
                lock (Program.toChange)
                    lock (Program.enemies)
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
                            float dX = item.position.X - (position.X + Program.random.Next(2) - 1);
                            float dY = item.position.Y - (position.Y + Program.random.Next(2) - 1);
                            float vX = dX / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.random.NextDouble() - 0.5d) / 4f;
                            float vY = dY / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.random.NextDouble() - 0.5d) / 4f;
                            float scale = E.Scale(vX, vY, 800);
                            //Rotate the gun
                            shape.Rotation = (float)Math.Atan2(vY, vX) / (2f * (float)Math.PI) * 360f - 90f;
                            Bullet bullet = new Bullet(position.X, position.Y, vX * scale, vY * scale, 600, amount.Value, amount.Value,
                                1, new Vector2f(6, 6), typeof(Cannon),
                                (p) => Program.toChange.Add(new Explosion(p.X - ExplosionSize.Value / 2, p.Y - ExplosionSize.Value / 2,
                                    ExplosionSize.Value, ExplosionDamage)));
                            bullet.shape.FillColor = Color.Black;
                            Program.toChange.Add(bullet);
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }
}