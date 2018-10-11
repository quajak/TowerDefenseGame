using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrySFML2
{
    internal abstract class GUI : Entity
    {
        public string name;
        internal readonly Vector2f size;

        public Vector2f Size
        {
            get
            {
                return (shape as RectangleShape).Size;
            }
            set
            {
                (shape as RectangleShape).Size = value;
            }
        }

        public bool visible = true;

        public GUI(float x, float y, float width, float height, string name = "", Vector2f? size = null) : base(x, y, new RectangleShape(new Vector2f(width, height)))
        {
            if (size is null)
            {
                //we predict from current
                this.size = (shape as RectangleShape).Size;
            }
            else
            {
                this.size = size.Value;
            }
            blocking = true;
            clickLayer = 10;
            this.name = name;
        }

        public override void Collision(Entity collided)
        {
            base.Collision(collided);
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
            base.OnClick(x, y, button);
        }

        public override Shape Update(double timeDiff)
        {
            var _size = (shape as RectangleShape).Size;
            _size.X = visible ? size.X : 0;
            (shape as RectangleShape).Size = _size;
            return base.Update(timeDiff);
        }

        public virtual void Delete()
        {
        }
    }

    internal class GUIField : GUI
    {
        public GUIField(float x, float y, float width, float height, Color color, string name = "") : base(x, y, width, height, name)
        {
            shape.FillColor = color;
        }
    }

    internal class GUICircle : GUI
    {
        public GUICircle(float x, float y, float radius, Color fill, Color border, int borderThickness, string name = "") : base(x, y, 0, 0, name)
        {
            shape = new CircleShape(radius)
            {
                Position = new Vector2f(x - radius, y - radius),
                FillColor = fill,
                OutlineColor = border,
                OutlineThickness = borderThickness
            };
        }

        public override Shape Update(double timeDiff)
        {
            return shape;
        }
    }

    internal class GUIText : GUI
    {
        private List<Text> texts = new List<Text>();
        private string content;
        private Vector2f correctPosition;
        private readonly int font;
        private readonly float maxWidth;
        private readonly float offset;

        public GUIText(float x, float y, string content, Color color, int font = 16, string name = "", float maxWidth = 200, float offset = 40) : base(x, y, 1, 1, name)
        {
            shape = null;
            texts = Wrap(content, font, maxWidth, x, y, offset);
            Program.texts.AddRange(texts);
            renderLayer = 1;
            this.font = font;
            this.maxWidth = maxWidth;
            this.offset = offset;
            this.Content = content;
            correctPosition = position;
        }

        public static List<Text> Wrap(string text, int font, float width, float x, float y, float offset)
        {
            List<Text> texts = new List<Text>();
            Text active;
            Text nextActive;
            List<string> words = text.Split(' ').ToList();
            string v = words[0];
            while (words.Count != 0)
            {
                active = new Text("", Program.font, (uint)font);
                nextActive = new Text(v, Program.font, (uint)font);
                while (nextActive.GetLocalBounds().Width < width && words.Count > 0)
                {
                    active.DisplayedString = nextActive.DisplayedString;
                    words.Remove(v);
                    if (words.Count != 0)
                    {
                        v = words[0];
                        nextActive.DisplayedString += " " + v;
                    }
                }
                //v is used to start the next
                texts.Add(active);

                if (active.DisplayedString.Length == 0 && words.Count != 0)
                    throw new Exception("Word is too long to be wrapped!");
            }
            //now we position the texts correctly
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i].Position = new Vector2f(x, i * offset + y);
            }
            return texts;
        }

        public string Content
        {
            get => content; set
            {
                content = value;
                texts.ForEach(t => Program.texts.Remove(t));
                texts = Wrap(content, font, maxWidth, position.X, position.Y, offset);
                texts.ForEach(t => Program.texts.Add(t));
            }
        }

        public void UpdateVisibility()
        {
            if (visible)
            {
                position = correctPosition;
            }
            else
            {
                position = new Vector2f(-300, 0);
            }
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i].Position = new Vector2f(position.X, position.Y + i * offset);
            }
        }

        public override void Delete()
        {
            texts.ForEach(t => Program.texts.RemoveAll(p => p.DisplayedString == t.DisplayedString));
        }
    }

    internal class GuiButton : GUI
    {
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                text.Content = content;
            }
        }

        private GUIText text;
        private string content;

        public GuiButton(float x, float y, float width, float height, Color color, string content, string name = "") : base(x, y, width, height, name)
        {
            shape.FillColor = color;
            renderLayer = 10;
            clickLayer = 1;
            text = new GUIText(x + 10, y + 10, content, Color.Red);
            this.content = content;
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
            MouseButtonEventArgs args = new MouseButtonEventArgs(new MouseButtonEvent() { X = x, Y = y, Button = button });
            Click.Invoke(this, args);
        }

        public override void Delete()
        {
            text.Delete();
            base.Delete();
        }

        public event EventHandler<MouseButtonEventArgs> Click;
    }

    internal class MenuGUI : GUI
    {
        private List<GUI> parts = new List<GUI>();

        public MenuGUI() : base(0, 0, 100, 1000)
        {
            clickLayer = 1;
            renderLayer = 80;
            shape.FillColor = new Color(110, 114, 114, 200);
            GUIField item = new GUIField(80, 0, 20, 20, Color.Red, "Hide");
            item.renderLayer = 90;
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 20, 60, 60, Color.White, "Controller");
            item.renderLayer = 90;
            item.shape.Texture = new Texture("./Resources/mainBase.jpg");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 100, 60, 60, Color.White, "MachineGun");
            item.renderLayer = 90;
            item.shape.Texture = new Texture("./Resources/MachineGun.png");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 180, 60, 60, Color.White, "LazorGun");
            item.renderLayer = 90;
            item.shape.Texture = new Texture("./Resources/LazorGun.png");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 260, 60, 60, Color.Black, "Bomb");
            item.renderLayer = 90;
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 340, 60, 60, Color.White, "Bank");
            item.renderLayer = 1900;
            item.shape.Texture = new Texture("./Resources/BankTower.png");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 420, 60, 60, Color.White, "IceTower");
            item.renderLayer = 90;
            item.shape.Texture = new Texture("./Resources/IceTower.png");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 500, 60, 60, Color.White, "Deselect");
            item.renderLayer = 90;
            item.shape.Texture = new Texture("./Resources/DeselectTool.png");
            parts.Add(item);
            Program.objects.Add(item);
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
            GUI clicked = Program.GetEntityAt(x, y, parts.Select(p => p as Entity).ToList()) as GUI;

            Vector2f size;
            switch (clicked)
            {
                case GUIField field:
                    switch (field.name)
                    {
                        case "Hide":
                            Program.objects.RemoveAll(o => parts.Exists(p => p == o as GUI));
                            size = Size;
                            size.X = 10;
                            Size = size;
                            break;

                        case "Controller":
                            if (MainBase.Available)
                                Program.ToCreate = new MainBase(x, y, false);
                            break;

                        case "MachineGun":
                            if (MachineGun.Available)
                                Program.ToCreate = new MachineGun(x, y);
                            break;

                        case "LazorGun":
                            if (LazorGun.Available)
                                Program.ToCreate = new LazorGun(x, y);
                            break;

                        case "Bomb":
                            if (Bomb.Available)
                                Program.ToCreate = new Bomb(x, y);
                            break;

                        case "Bank":
                            if (BankTower.Available)
                                Program.ToCreate = new BankTower(x, y);
                            break;

                        case "IceTower":
                            if (IceTower.Available)
                                Program.ToCreate = new IceTower(x, y);
                            break;

                        case "Deselect":
                            Program.ToCreate = null;
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    size = Size;
                    if (size.X == 10)
                    {
                        size.X = 100;
                        Program.objects.AddRange(parts);
                    }
                    Size = size;

                    break;
            }
            base.OnClick(x, y, button);
        }

        public override Shape Update(double timeDiff)
        {
            Shape s = parts.Find(p => p.name == "Controller").shape;
            Color color = s.FillColor;
            color.A = MainBase.Available ? (byte)255 : (byte)100;
            s.FillColor = color;
            s.OutlineThickness = Program.ToCreate != null && Program.ToCreate.GetType() == typeof(MainBase) ? 2 : 0;

            s = parts.Find(p => p.name == "MachineGun").shape;
            color = s.FillColor;
            color.A = MachineGun.Available ? (byte)255 : (byte)100;
            s.FillColor = color;
            s.OutlineThickness = Program.ToCreate != null && Program.ToCreate.GetType() == typeof(MachineGun) ? 2 : 0;

            s = parts.Find(p => p.name == "LazorGun").shape;
            color = s.FillColor;
            color.A = LazorGun.Available ? (byte)255 : (byte)100;
            s.FillColor = color;
            s.OutlineThickness = Program.ToCreate != null && Program.ToCreate.GetType() == typeof(LazorGun) ? 2 : 0;

            s = parts.Find(p => p.name == "Bomb").shape;
            color = s.FillColor;
            color.A = Bomb.Available ? (byte)255 : (byte)100;
            s.FillColor = color;
            s.OutlineThickness = Program.ToCreate != null && Program.ToCreate.GetType() == typeof(Bomb) ? 2 : 0;

            s = parts.Find(p => p.name == "Bank").shape;
            color = s.FillColor;
            color.A = BankTower.Available ? (byte)255 : (byte)100;
            s.FillColor = color;
            s.OutlineThickness = Program.ToCreate != null && Program.ToCreate.GetType() == typeof(BankTower) ? 2 : 0;

            s = parts.Find(p => p.name == "IceTower").shape;
            color = s.FillColor;
            color.A = IceTower.Available ? (byte)255 : (byte)100;
            s.FillColor = color;
            s.OutlineThickness = Program.ToCreate != null && Program.ToCreate.GetType() == typeof(IceTower) ? 2 : 0;

            return base.Update(timeDiff);
        }
    }

    internal class TowerOverViewGUI : GUI
    {
        private List<GUI> parts = new List<GUI>();
        public Tower Selected = null;

        public TowerOverViewGUI() : base(Program.gameSize.X - 200, 0, 200, Program.gameSize.Y)
        {
            float x = Program.gameSize.X;
            shape.FillColor = new Color(110, 114, 114, 200);
            visible = false;
            GUIText gText = new GUIText(x - 190, 10, "Name", Color.Black, 20, "Name", 180);
            parts.Add(gText);
            gText = new GUIText(x - 190, 100, "Description", Color.Black, name: "Desc", maxWidth: 180, offset: 20);
            parts.Add(gText);
            clickLayer = 2;
            renderLayer = 90;
        }

        public override Shape Update(double timeDiff)
        {
            var toRemove = parts.Where(p => p.name.Contains("upgrade")).ToList();
            toRemove.ForEach(p => p.Delete());
            toRemove.ForEach(p => parts.Remove(p));
            toRemove.ForEach(p => Program.objects.Remove(p));
            var rI = parts.FirstOrDefault(p => p.name == "RangeIndicator");
            if (rI != null)
            {
                parts.Remove(rI);
                Program.objects.Remove(rI);
            }
            if (Selected != null)
            {
                GUIText t = parts.First(p => p.name == "Name") as GUIText;
                t.Content = Selected.name;
                t = parts.First(p => p.name == "Desc") as GUIText;
                t.Content = Selected.description;
                float x = Program.gameSize.X - 190;
                float y = UpgradeGUI.Height;
                if (Selected.range.Value != 0)
                {
                    GUICircle rangeIndicator = new GUICircle(Selected.position.X, Selected.position.Y, Selected.range.Value, new Color(128, 125, 125, 100),
                        Color.Black, 2, "RangeIndicator")
                    {
                        clickLayer = 100,
                        renderLayer = 10,
                        blocking = false
                    };
                    parts.Add(rangeIndicator);
                    Program.objects.Add(rangeIndicator);
                }
                foreach (var upgrade in Selected.available)
                {
                    UpgradeGUI gU = new UpgradeGUI(x, y, upgrade, $"upgrade{upgrade.Name}");
                    gU.Click += GU_Click;
                    y += UpgradeGUI.Height;
                    parts.Add(gU);
                    Program.objects.Add(gU);
                }
            }
            else
            {
            }
            foreach (var p in parts)
            {
                p.visible = visible;
                if (p is GUIText t)
                    t.UpdateVisibility();
            }
            return base.Update(timeDiff);
        }

        private void GU_Click(object sender, MouseButtonEventArgs e)
        {
            Upgrade u = (sender as UpgradeGUI).Upgrade;
            Selected.installed.Add(u);
            Selected.available.Remove(u);
            Selected.available.AddRange(u.Unlocks);
            Program.Money -= u.Cost;
            u.Install(Selected);
        }
    }
}