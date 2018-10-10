using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace TrySFML2
{
    abstract class GUI : Entity
    {
        public string name;
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
        public bool shown = true;
        public GUI(float x, float y, float width, float height) : base(x,y,new RectangleShape(new Vector2f(width, height)))
        {
            blocking = true;
            clickLayer = 10;
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
            return base.Update(timeDiff);
        }

        public virtual void Delete()
        {

        }
    }

    class GUIField : GUI
    {
        public GUIField(float x, float y, float width, float height, Color color, string name = "") : base(x,y,width, height)
        {
            shape.FillColor = color;
            this.name = name;
            renderLayer = 1;
        }
    }

    class GUIText : GUI
    {
        
        readonly Text text;
        private string content;

        public GUIText(float x, float y, string content, Color color, int font = 16) : base(x,y, 1, 1)
        {
            shape = null;
            text = new Text(content, Program.font, (uint)font)
            {
                Color = color,
                Position = new Vector2f(x, y)
            };
            Program.texts.Add(text);
            renderLayer = 1;
            this.Content = content;
        }

        public string Content
        {
            get => content; set
            {
                content = value;
                text.DisplayedString = content;
            }
        }

        public override void Delete()
        {
            Program.texts.Remove(text);
        }
    }

    class GuiButton : GUI
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

        GUIText text;
        private string content;

        public GuiButton(float x, float y, float width, float height, Color color, string content, string name = "") : base(x,y, width, height)
        {
            shape.FillColor = color;
            this.name = name;
            renderLayer = 10;
            clickLayer = 1;
            text = new GUIText(x + 10, y + 10, content, Color.Red);
            this.content = content;
        }

        public override void OnClick(int x, int y, Mouse.Button button)
        {
            MouseButtonEventArgs args = new MouseButtonEventArgs(new MouseButtonEvent() { X = x, Y = y, Button=button });
            Click.Invoke(this, args);
        }

        public override void Delete()
        {
            text.Delete();
            base.Delete();
        }

        public event EventHandler<MouseButtonEventArgs> Click;
    }

    class MainMenuGUI : GUI
    {
        List<GUI> parts = new List<GUI>();
        GuiButton difficultyButton;
        public int difficulty = 1;
        public MainMenuGUI() : base(0, 0, Program.gameSize.X, Program.gameSize.Y)
        {
            shape.FillColor = new Color(200, 0, 0, 100);
            GUIText text = new GUIText(Program.gameSize.X / 3, 200, "Tower Defense Game!", Color.Blue, 64);
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
                if(part is GuiButton)
                {
                    Program.objects.Remove(part);
                }
            }
            base.Delete();
        }

    }

    class MenuGUI : GUI
    {
        List<GUI> parts = new List<GUI>();
        public MenuGUI() : base(0, 0, 100, 1000)
        {
            clickLayer = 1;
            renderLayer = 10;
            shape.FillColor = new Color(255, 255, 255, 200);
            GUIField item = new GUIField(80, 0, 20, 20, Color.Red, "Hide");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 20, 60, 60, Color.Red, "Controller");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 100, 60, 60, Color.Cyan, "MachineGun");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 180, 60, 60, Color.Green, "LazorGun");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 260, 60, 60, Color.Black, "Bomb");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 340, 60, 60, Color.Yellow, "Bank");
            parts.Add(item);
            Program.objects.Add(item);
            item = new GUIField(20, 420, 60, 60, new Color(66, 244, 241), "IceTower");
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
                            if(MachineGun.Available)
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
}
