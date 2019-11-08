using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;

namespace TowerDefenseGame
{
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
        internal bool canSelect = true;

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
            if (canSelect)
            {
                Program.TowerGUI.visible = true;
                Program.TowerGUI.Selected = this;
            }
            base.OnClick(x, y, button);
        }
    }
}