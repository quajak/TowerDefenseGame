using SFML.Graphics;
using SFML.System;

namespace TowerDefenseGame
{
    internal class BankTower : Tower
    {
        private const int _cost = 20;

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
            if (collided is Enemy)
            {
                Program.ToChange.Add(this);
            }
        }

        public override Entity Create(int x, int y)
        {
            return new BankTower(x, y, true);
        }

        private float moneyTime = 0;

        public override Drawable Update(double timeDiff)
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
}