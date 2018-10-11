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

    internal class MainMenuGUI : GUI
    {
        private List<GUI> parts = new List<GUI>();
        private GuiButton difficultyButton;
        public int difficulty = 1;

        public MainMenuGUI() : base(0, 0, Program.gameSize.X, Program.gameSize.Y)
        {
            shape.FillColor = new Color(200, 0, 0, 100);
            GUIText text = new GUIText(Program.gameSize.X / 3, 200, "Tower Defense Game!", Color.Blue, 64, maxWidth: 600);
            parts.Add(text);
            difficultyButton = new GuiButton(Program.gameSize.X / 2 - 200, 300, 150, 40, new Color(0, 0, 200, 100), "Difficult: 1", "difficultyButton");
            parts.Add(difficultyButton);
            Program.objects.Add(difficultyButton);
            difficultyButton.Click += DifficultyButton_Click;
            GuiButton button = new GuiButton(Program.gameSize.X / 2 - 200, 400, 150, 40, new Color(0, 0, 200, 100), "Start Game");
            parts.Add(button);
            Program.objects.Add(button);
            button.Click += Button_Click;
        }

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            Program.gameEvent = Program.GameEvent.Next;
        }

        private void DifficultyButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == Mouse.Button.Right)
                difficulty--;
            else
                difficulty++;
            difficulty = difficulty < 1 ? 1 : difficulty;
            difficultyButton.Content = $"Difficulty: {difficulty}";
        }

        public override void Delete()
        {
            foreach (var part in parts)
            {
                part.Delete();
                if (part is GuiButton)
                {
                    Program.objects.Remove(part);
                }
            }
            base.Delete();
        }
    }

    internal class MenuGUI : GUI
    {
        private List<GUI> parts = new List<GUI>();

        public MenuGUI() : base(0, 0, 100, 1000)
        {
            clickLayer = 1;
            renderLayer = 1;
            shape.FillColor = new Color(110, 114, 114, 200);
            GUIField item = new GUIField(80, 0, 20, 20, Color.Red, "Hide");
            item.renderLayer = 10;
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 20, 60, 60, Color.White, "Controller");
            item.renderLayer = 10;
            item.shape.Texture = new Texture("./Resources/mainBase.jpg");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 100, 60, 60, Color.White, "MachineGun");
            item.renderLayer = 10;
            item.shape.Texture = new Texture("./Resources/MachineGun.png");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 180, 60, 60, Color.White, "LazorGun");
            item.renderLayer = 10;
            item.shape.Texture = new Texture("./Resources/LazorGun.png");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 260, 60, 60, Color.Black, "Bomb");
            item.renderLayer = 10;
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 340, 60, 60, Color.White, "Bank");
            item.renderLayer = 10;
            item.shape.Texture = new Texture("./Resources/BankTower.png");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 420, 60, 60, Color.White, "IceTower");
            item.renderLayer = 10;
            item.shape.Texture = new Texture("./Resources/IceTower.png");
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

            s = parts.Find(p => p.name == "MachineGun").shape;
            color = s.FillColor;
            color.A = MachineGun.Available ? (byte)255 : (byte)100;
            s.FillColor = color;

            s = parts.Find(p => p.name == "LazorGun").shape;
            color = s.FillColor;
            color.A = LazorGun.Available ? (byte)255 : (byte)100;
            s.FillColor = color;

            s = parts.Find(p => p.name == "Bomb").shape;
            color = s.FillColor;
            color.A = Bomb.Available ? (byte)255 : (byte)100;
            s.FillColor = color;

            s = parts.Find(p => p.name == "Bank").shape;
            color = s.FillColor;
            color.A = BankTower.Available ? (byte)255 : (byte)100;
            s.FillColor = color;

            s = parts.Find(p => p.name == "IceTower").shape;
            color = s.FillColor;
            color.A = IceTower.Available ? (byte)255 : (byte)100;
            s.FillColor = color;

            return base.Update(timeDiff);
        }
    }

    internal class TowerOverViewGUI : GUI
    {
        private List<GUI> parts = new List<GUI>();
        public Tower selected = null;

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
            if (selected != null)
            {
                GUIText t = parts.First(p => p.name == "Name") as GUIText;
                t.Content = selected.name;
                t = parts.First(p => p.name == "Desc") as GUIText;
                t.Content = selected.description;
                var toRemove = parts.Where(p => p.name.Contains("upgrade")).ToList();
                toRemove.ForEach(p => p.Delete());
                toRemove.ForEach(p => parts.Remove(p));
                toRemove.ForEach(p => Program.objects.Remove(p));
                float x = Program.gameSize.X - 190;
                float y = 200;
                foreach (var upgrade in selected.available)
                {
                    UpgradeGUI gU = new UpgradeGUI(x, y, upgrade, $"upgrade{upgrade.name}");
                    gU.Click += GU_Click;
                    y += 200;
                    parts.Add(gU);
                    Program.objects.Add(gU);
                }
            }
            else
            {
                //Cleanup
                var toRemove = parts.Where(p => p.name.Contains("upgrade")).ToList();
                toRemove.ForEach(p => p.Delete());
                toRemove.ForEach(p => parts.Remove(p));
                toRemove.ForEach(p => Program.objects.Remove(p));
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
            Upgrade u = (sender as UpgradeGUI).upgrade;
            selected.installed.Add(u);
            selected.available.Remove(u);
            selected.available.AddRange(u.unlocks);
            Program.Money -= u.cost;
            u.Install(selected);
        }
    }
}