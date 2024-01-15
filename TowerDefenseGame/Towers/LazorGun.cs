using TowerDefenseGame.Towers;
using SFML.Graphics;
using SFML.System;
using System;
using System.Linq;

namespace TowerDefenseGame
{
    internal class LazorGun : Tower
    {
        private const int _cost = 20;

        public static bool Available
        {
            get
            {
                return Program.Money >= _cost && !MainBase.Available;
            }
        }

        private double attackTime = 0;

        public LazorGun(int aX, int aY, bool buy = false) : base(aX, aY, new RectangleShape(new Vector2f(20, 20)), _cost, "Lazor Gun", "Shoots lazors", 400f, 10f, 2_500f)
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
            Upgrade amountI = new Upgrade(new Modifier(ModifierType.Value, 10), 3, "Stonger Rays I", "Increases ray damage by 10", UpdateType.Amount)
            {
                Unlocks =
                {
                    new Upgrade(new Modifier(ModifierType.Percentage, 30), 12, "Stronger Rays II", "Increases ray damage by 30", UpdateType.Range)
                    {
                        Unlocks =
                        {
                            new Upgrade(new Modifier(ModifierType.Value, 30), 15, "Stronger Rays III", "Increase ray damage by 30", UpdateType.Range)
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
            AvailableUpgrades.Add(amountI);
        }

        override public Entity Create(int x, int y)
        {
            return new LazorGun(x, y, true);
        }

        public override Drawable Update(double timeDiff)
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
                            //Do a simple raycast to determine collision with land
                            float distance = Program.DoRayCast(position, dX, dY, Range.Value, Program.Terrains.Where(t => (t as Terrain).blocksBullets).ToList());
                            Program.ToChange.Add(new Lazor(position.X, position.Y, distance, 2, angle, (int)Amount.Value));
                        }
                    }
            }
            return base.Update(timeDiff);
        }
    }
}