﻿using TowerDefenseGame.Towers;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefenseGame
{
    internal class Cannon : Tower
    {
        private const int _cost = 8;

        public int ExtraPierce = 0;
        public Stat ExplosionSize = new Stat(60);
        public int ExplosionDamage = 1;
        private readonly List<Entity> blockingTerrain;

        public Cannon(float x, float y, bool buy = false) : base(x, y, new RectangleShape(new Vector2f(20, 20)), _cost, "Cannon", "Shoot bullets which explode",
            200, 2, 500)
        {
            blockingTerrain = Program.Terrains.Where(e => (e as Terrain).blocksBullets).ToList();
            if (buy)
                Program.Money -= _cost;
            shape.Texture = new Texture("./Resources/cannon.png");
            shape.Origin = new Vector2f(10, 10);
            position = new Vector2f(position.X + 10, position.Y + 10);
            shape.Position = position;
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
                t => (t as Cannon).ExplosionSize.Modifiers.Add(new Modifier(ModifierType.Percentage, 50)))
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
            AvailableUpgrades.Add(pierce);
            AvailableUpgrades.Add(explosionSize);
            AvailableUpgrades.Add(rangeI);
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

        public override Drawable Update(double timeDiff)
        {
            attackTime -= (float)timeDiff;
            if (attackTime < 0)
            {
                attackTime = AttackSpeed.Value;
                lock (Program.ToChange)
                    lock (Program.Enemies)
                    {
                        var possible = from enemy in Program.Enemies
                                       where E.Distance(this, enemy) < Range.Value
                                       select enemy;
                        Entity item = possible.OrderBy(x => E.Distance(this, x))
                            .FirstOrDefault(e => !Program.HitRayCast(position, e.position, blockingTerrain));
                        if (item != null)
                        {
                            //Generate bullet
                            var size = (shape as RectangleShape).Size;
                            float dX = item.position.X - (position.X + Program.Random.Next(2) - 1);
                            float dY = item.position.Y - (position.Y + Program.Random.Next(2) - 1);
                            float vX = dX / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.Random.NextDouble() - 0.5d) / 4f;
                            float vY = dY / Math.Max(Math.Abs(dX), Math.Abs(dY)) + (float)(Program.Random.NextDouble() - 0.5d) / 4f;
                            float scale = E.Scale(vX, vY, 800);
                            //Rotate the gun
                            shape.Rotation = (float)Math.Atan2(vY, vX) / (2f * (float)Math.PI) * 360f - 90f;
                            Bullet bullet = new Bullet(position.X, position.Y, vX * scale, vY * scale, Range.Value * 1.5f, Amount.Value, Amount.Value,
                                1, new Vector2f(6, 6), typeof(Cannon),
                                (p) => Program.ToChange.Add(new Explosion(p.X - ExplosionSize.Value / 2, p.Y - ExplosionSize.Value / 2,
                                    ExplosionSize.Value, ExplosionDamage)));
                            bullet.shape.FillColor = Color.Black;
                            Program.ToChange.Add(bullet);
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }
}