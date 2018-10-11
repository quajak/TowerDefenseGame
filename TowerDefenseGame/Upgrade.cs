using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrySFML2
{
    internal enum UpdateType
    { Speed, Amount, Range }

    internal class Upgrade
    {
        public UpdateType type;
        public Modifier modifier;
        public int cost;
        public string name;
        public string description;
        public List<Upgrade> unlocks = new List<Upgrade>();

        public Upgrade(Modifier modifier, int cost, string name, string description, UpdateType type)
        {
            this.modifier = modifier;
            this.cost = cost;
            this.name = name;
            this.description = description;
            this.type = type;
        }

        public void Install(Tower tower)
        {
            switch (type)
            {
                case UpdateType.Speed:
                    Console.WriteLine($"Upgrading speed of {tower.name} by {name}");
                    tower.attackSpeed.modifiers.Add(modifier);
                    break;

                case UpdateType.Amount:
                    Console.WriteLine($"Upgrading amount of {tower.name} by {name}");
                    tower.amount.modifiers.Add(modifier);
                    break;

                case UpdateType.Range:
                    Console.WriteLine($"Upgrading range of {tower.name} by {name}");
                    tower.range.modifiers.Add(modifier);
                    break;

                default:
                    break;
            }
        }
    }

    internal class UpgradeGUI : GUI
    {
        private List<GUI> parts = new List<GUI>();
        public Upgrade upgrade;

        public UpgradeGUI(float x, float y, Upgrade upgrade, string name = "") : base(x, y, 170, 180, name)
        {
            this.upgrade = upgrade;
            clickLayer = 1;
            shape.FillColor = Available ? new Color(59, 61, 61) : new Color(91, 59, 59);
            GUIText gT = new GUIText(x + 10, y + 10, upgrade.name, Color.Black, maxWidth: 160, offset: 20);
            parts.Add(gT);
            gT = new GUIText(x + 10, y + 60, upgrade.description, Color.Black, maxWidth: 160, offset: 18);
            parts.Add(gT);
            gT = new GUIText(x + 10, y + 160, $"${upgrade.cost}", Color.Black, maxWidth: 160, offset: 18);
            parts.Add(gT);
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
            if (Available)
            {
                MouseButtonEventArgs args = new MouseButtonEventArgs(new MouseButtonEvent() { X = x, Y = y, Button = button });
                Click.Invoke(this, args);
            }
        }

        private bool Available
        {
            get => Program.Money > upgrade.cost;
        }

        public override Shape Update(double timeDiff)
        {
            shape.FillColor = Available ? new Color(59, 61, 61) : new Color(91, 59, 59);
            return base.Update(timeDiff);
        }

        public event EventHandler<MouseButtonEventArgs> Click;

        public override void Delete()
        {
            parts.ForEach(p => p.Delete());
            parts.ForEach(p => Program.objects.Remove(p));
            base.Delete();
        }
    }
}