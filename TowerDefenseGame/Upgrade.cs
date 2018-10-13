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
    { Speed, Amount, Range, Special }

    internal class Upgrade
    {
        public UpdateType Type;
        public Modifier Modifier;
        public int Cost;
        public string Name;
        public string Description;
        public List<Upgrade> Unlocks = new List<Upgrade>();

        public Upgrade(Modifier modifier, int cost, string name, string description, UpdateType type)
        {
            Modifier = modifier;
            Cost = cost;
            Name = name;
            Description = description;
            Type = type;
        }

        public virtual void Install(Tower tower)
        {
            switch (Type)
            {
                case UpdateType.Speed:
                    Console.WriteLine($"Upgrading speed of {tower.Name} by {Name}");
                    tower.AttackSpeed.modifiers.Add(Modifier);
                    break;

                case UpdateType.Amount:
                    Console.WriteLine($"Upgrading amount of {tower.Name} by {Name}");
                    tower.Amount.modifiers.Add(Modifier);
                    break;

                case UpdateType.Range:
                    Console.WriteLine($"Upgrading range of {tower.Name} by {Name}");
                    tower.Range.modifiers.Add(Modifier);
                    break;

                default:
                    break;
            }
        }
    }

    internal class CustomUpgrade : Upgrade
    {
        private readonly Action<Tower> install;

        public CustomUpgrade(Modifier modifier, int cost, string name, string description, Action<Tower> install) : base(modifier, cost, name, description, UpdateType.Special)
        {
            this.install = install;
        }

        public override void Install(Tower tower)
        {
            install.Invoke(tower);
            if (Modifier != null)
                base.Install(tower);
        }
    }

    internal class UpgradeGUI : GUI
    {
        private List<GUI> parts = new List<GUI>();
        public Upgrade Upgrade;
        public static float Height = 150;

        public UpgradeGUI(float x, float y, Upgrade upgrade, string name = "") : base(x, y, 170, Height, name)
        {
            Upgrade = upgrade;
            clickLayer = 1;
            shape.FillColor = Available ? new Color(59, 61, 61) : new Color(91, 59, 59);
            GUIText gT = new GUIText(x + 10, y + 10, upgrade.Name, Color.Black, maxWidth: 160, offset: 20);
            parts.Add(gT);
            gT = new GUIText(x + 10, y + 60, upgrade.Description, Color.Black, maxWidth: 160, offset: 18);
            parts.Add(gT);
            gT = new GUIText(x + 10, y + 120, $"${upgrade.Cost}", Color.Black, maxWidth: 160, offset: 18);
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
            get => Program.Money >= Upgrade.Cost;
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
            parts.ForEach(p => Program.Objects.Remove(p));
            base.Delete();
        }
    }
}