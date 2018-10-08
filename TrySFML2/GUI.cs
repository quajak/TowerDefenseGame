using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;

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
            clickLayer = 10;
        }

        public override void Collision(Entity collided)
        {
            base.Collision(collided);
        }

        public override void OnClick(int x, int y)
        {
            base.OnClick(x, y);
        }

        public override Shape Update(double timeDiff)
        {
            return base.Update(timeDiff);
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
        }

        public override void OnClick(int x, int y)
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
            base.OnClick(x, y);
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


            return base.Update(timeDiff);
        }
    }
}
