using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefenseGame
{
    internal class Program
    {
        public static GameEvent gameEvent = GameEvent.None;
        public static RenderWindow window;

        public static List<Entity> Objects = new List<Entity>();
        public static List<Entity> ToChange = new List<Entity>();
        public static List<Entity> Enemies = new List<Entity>();
        public static List<Entity> Terrains = new List<Entity>();
        public static List<Entity> PlayerBuildings = new List<Entity>();
        public static List<Shape> Shapes = new List<Shape>(); //keep for debug reasons
        public static List<Text> Texts = new List<Text>();

        public static Random Random = new Random();

        public static Vector2u GameSize;

        public static Tower ToCreate;
        public static Font Font = new Font("./Resources/TitilliumWeb-Regular.ttf"); //from https://www.1001freefonts.com/titillium-web.font

        private static int money = 10;
        public static int MoneyRate = 1;

        public static int EnemiesKilled = 0;
        public static long TimePlayed = 0;

        public static TowerOverViewGUI TowerGUI;

        public static MainBase mainBase;
        public static bool GameEnded = false;

        public static int Money
        {
            get => money; set
            {
                money = value;
            }
        }

        private static void Main(string[] _)
        {
            window = new RenderWindow(VideoMode.DesktopMode, "Tower Defense Game", Styles.Default, new ContextSettings(0, 0, 8));
            GameSize = window.Size;
            window.Closed += Window_Closed;
            window.GainedFocus += Window_GainedFocus;
            window.LostFocus += Window_LostFocus;
            window.MouseButtonPressed += Window_MouseButtonPressed;

            MainMenuWindow menuWindow = new MainMenuWindow();
            Window running = menuWindow.Run(window);
            do
            {
                running = running.Run(window);
            } while (running != null);
        }

        public static bool IsFree(float x, float y, Shape s)
        {
            if (s is RectangleShape rectangle)
            {
                return IsFree(x, y, rectangle.Size.X, rectangle.Size.Y);
            }
            else if (s is CircleShape circle)
            {
                return IsFree(x, y, circle.Radius);
            }
            throw new Exception();
        }

        public static bool IsFree(float X, float Y, float Width, float Height)
        {
            Entity entity = new Entity(X, Y, new RectangleShape(new Vector2f(Width, Height)));
            foreach (var en in Objects)
            {
                if (en.blocking && PolygonCollision(en, entity))
                    return false;
            }
            return true;
        }

        public static bool IsFree(float x, float y, float radius)
        {
            Entity entity = new Entity(x - radius / 2, y - radius / 2, new CircleShape(radius));
            foreach (var en in Objects)
            {
                if (en.blocking && PolygonCollision(en, entity))
                    return false;
            }
            return true;
        }

        public static Entity GetEntityAt(float X, float Y, List<Entity> list)
        {
            list = list.OrderBy(l => l.clickLayer).ToList();
            foreach (var item in list)
            {
                if (item.shape is RectangleShape rectangleShape)
                {
                    if (((item.position.X - item.shape.Origin.X <= X) && (item.position.X + rectangleShape.Size.X - item.shape.Origin.X > X))
                        && (item.position.Y - item.shape.Origin.Y <= Y && item.position.Y + rectangleShape.Size.Y - item.shape.Origin.Y > Y))
                    {
                        if (!(item is GUI g) || g.visible)
                        {
                            return item;
                        }
                    }
                }
                else if (item.shape is CircleShape circleShape)
                {
                    //Cheap way to fix the offset of the visible circle from its actual position
                    if (circleShape.Radius > E.Distance(new Vector2f(X - circleShape.Radius, Y - circleShape.Radius), item.position))
                    {
                        if (!(item is GUI g) || g.visible)
                        {
                            return item;
                        }
                    }
                }
                else if (item.vertexArray != null) //Assume that it is not convex
                {
                    if (pnpoly(item.logicalPoints, X, Y))
                    {
                        return item;
                    }
                    bool pnpoly(List<Vector2f> points, double testx, double testy)
                    {
                        int nvert = points.Count;
                        int i, j;
                        bool c = false;
                        for (i = 0, j = nvert - 1; i < nvert; j = i++)
                        {
                            if (((points[i].Y > testy) != (points[j].Y > testy)) &&
                                    (testx < (points[j].X - points[i].X) * (testy - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
                                c = !c;
                        }
                        return c;
                    }
                }
            }
            return null;
        }

        private static void Window_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            var clicked = GetEntityAt(e.X, e.Y, Objects.Where(o => o.blocking).ToList());

            if (clicked is null)
            {
                if (ToCreate != null && (!(ToCreate is MachineGun) || MachineGun.Available))
                {
                    if (ToCreate.Cost <= money && IsFree(e.X, e.Y, ToCreate.shape))
                    {
                        Entity item = ToCreate.Create(e.X - 5, e.Y - 5);
                        lock (PlayerBuildings)
                        {
                            PlayerBuildings.Add(item);
                        }
                        ToChange.Add(item);
                    }
                }
                if (TowerGUI != null)
                {
                    TowerGUI.visible = false;
                    TowerGUI.Selected = null;
                }
            }
            else
            {
                Console.WriteLine($"Clicked on {clicked.GetType().Name}");
                clicked.OnClick(e.X, e.Y, e.Button);
            }
        }

        private static void Window_LostFocus(object sender, EventArgs e)
        {
        }

        private static void Window_GainedFocus(object sender, EventArgs e)
        {
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            window.Close();
        }

        //View https://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection
        public static bool PolygonCollision(Entity polygonA, Entity polygonB)
        {
            //assume that the shapes are regular
            float maxLengthA = 0;
            if (polygonA.shape is RectangleShape r)
            {
                maxLengthA = Math.Max(r.Size.X, r.Size.Y);
            }
            else if (polygonA.shape is CircleShape c)
            {
                maxLengthA = c.Radius;
            }
            float maxLengthB = 0;
            if (polygonB.shape is RectangleShape r1)
            {
                maxLengthB = Math.Max(r1.Size.X, r1.Size.Y);
            }
            else if (polygonB.shape is CircleShape c)
            {
                maxLengthB = c.Radius;
            }
            //this should make skipping some cases quicker, but should give no false negatives
            if (Math.Abs(E.Distance(polygonA, polygonB)) > Math.Pow((maxLengthA + maxLengthB), 2))
                return false;

            bool intersect = true;

            List<Vector2f> pointsA = polygonA.Points.ToList();
            List<Vector2f> pointsB = polygonB.Points.ToList();
            int edgeCountA = pointsA.Count;
            int edgeCountB = pointsB.Count;
            Vector2f edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = new Vector2f(pointsA[edgeIndex].X - pointsA[edgeIndex - 1 >= 0 ? edgeIndex - 1 : pointsA.Count - 1].X,
                        pointsA[edgeIndex].Y - pointsA[edgeIndex - 1 >= 0 ? edgeIndex - 1 : pointsA.Count - 1].Y);
                }
                else
                {
                    edge = new Vector2f(pointsB[edgeIndex - edgeCountA].X - pointsB[edgeIndex - edgeCountA - 1 >= 0 ? edgeIndex - edgeCountA - 1 : pointsB.Count - 1].X,
                       pointsB[edgeIndex - edgeCountA].Y - pointsB[edgeIndex - edgeCountA - 1 >= 0 ? edgeIndex - edgeCountA - 1 : pointsB.Count - 1].Y);
                }

                // Find the axis perpendicular to the current edge
                Vector2f axis = new Vector2f(-edge.Y, edge.X);
                axis = E.Normalize(axis);

                // Find the projection of the polygon on the current axis
                float minA = 0;
                float minB = 0;
                float maxA = 0;
                float maxB = 0;
                E.ProjectPolygon(axis, pointsA, ref minA, ref maxA);
                E.ProjectPolygon(axis, pointsB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (E.IntervalDistance(minA, maxA, minB, maxB) > 0)
                    intersect = false;
            }
            return intersect;
        }

        internal static bool HitRayCast(Vector2f position, Vector2f p, List<Entity> toCheck)
        {
            float length = E.Distance(p, position);
            return DoRayCast(position, p.X - position.X, p.Y - position.Y, length, toCheck) < length;
        }

        internal static float DoRayCast(Vector2f position, Vector2f p, List<Entity> toCheck)
        {
            return DoRayCast(position, p.X - position.X, p.Y - position.Y, E.Distance(p, position), toCheck);
        }

        public static float DoRayCast(Vector2f position, float dX, float dY, float length, List<Entity> toCheck)
        {
            float m = Math.Max(Math.Abs(dX), Math.Abs(dY));
            float x = dX / m * 4;
            float y = dY / m * 4;
            float pX = position.X;
            float pY = position.Y;
            float distance = 0;
            while (distance < length)
            {
                pX += x;
                pY += y;
                if (GetEntityAt(pX, pY, toCheck) != null)
                    break;
                distance = (float)Math.Sqrt((pX - position.X) * (pX - position.X) + (pY - position.Y) * (pY - position.Y));
            }

            return distance;
        }

        public enum GameEvent { Next, None };
    }
}