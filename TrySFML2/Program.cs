using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace TrySFML2
{
    class Program
    {
        static RenderWindow window;

        public static List<Entity> objects = new List<Entity>();
        public static List<Entity> toChange = new List<Entity>();
        public static List<Entity> enemies = new List<Entity>();
        public static List<Entity> playerBuildings = new List<Entity>();
        public static List<Shape> shapes = new List<Shape>(); //keep for debug reasons

        public static Random random = new Random();

        public static Vector2u size;

        public static Tower ToCreate = new MachineGun(0, 0);
        static Font font = new Font("arial.ttf");

        private static int money = 10;
        static Text moneyText = new Text("", font, 32)
        {
            Position = new Vector2f(800, 10)
        };

        public static int EnemiesKilled = 0;
        public static long timePlayed = 0;

        public static float EvolutionFactor {
            get
            {
                return 1 * Math.Max(1f, (float)Math.Log10(EnemiesKilled + 1)) * Math.Max(1f, timePlayed / 60000f); //60 Sekunden bevor die Enemies wegen der Zeit rampen
            }
        }

        public static string EvolutionFactorString
        {
            get
            {
                float evolutionFactor = EvolutionFactor;
                float eP = Math.Max(1f, (float)Math.Log10(EnemiesKilled + 1)) / evolutionFactor * 100;
                float tP = Math.Max(1f, timePlayed / 100000f) / evolutionFactor * 100;
                return $"{evolutionFactor.ToString("0.00")} - {eP.ToString("0")}% {tP.ToString("0")}%";
            }
        }

        public static MainBase mainBase;
        public static bool GameEnded = false;

        public static int Money
        {
            get => money; set
            {
                money = value;
                if (moneyText is null) return;
                moneyText.DisplayedString = $"$ {money}";
            }
        }

        static void Main(string[] args)
        {
            window = new RenderWindow(VideoMode.DesktopMode, "Game");
            size = window.Size;
            window.Closed += Window_Closed;
            window.GainedFocus += Window_GainedFocus;
            window.LostFocus += Window_LostFocus;
            window.MouseButtonPressed += Window_MouseButtonPressed;

            objects.Add(new MenuGUI());

            Text text = new Text("", font, 32)
            {
                Color = Color.Red,
                Position = new Vector2f(1000, 10)
            };

            Text evolutionFactorText = new Text("", font, 32)
            {
                Color = Color.Red,
                Position = new Vector2f(1200, 10)
            };

            //Start enemy spawner
            Timer timer = new Timer((x) => {
                if(!MainBase.Available && !GameEnded)
                    lock (toChange)
                    lock (playerBuildings) // cam this cause a deadlock? Maybe
                    {
                        int num = random.Next(3 + (int)EvolutionFactor);
                        for (int i = 0; i < num; i++)
                        {
                            int tries = 0;
                            while (tries++ < 100)
                            {
                                int posX = random.Next((int)size.X);
                                int posY = random.Next((int)size.Y);
                                var leastDistance = playerBuildings.Select(b => b.position).Select(p => Distance(p, new Vector2f(posX, posY))).Min();
                                if(leastDistance > 100)
                                {
                                    Enemy item1 = new Enemy(posX, posY);
                                    enemies.Add(item1);
                                    toChange.Add(item1);
                                    break;
                                }
                            }
                        }
                    }
                Console.WriteLine($"Debug at {timePlayed / 1000} - Enemies: {enemies.Count} - Killed: {EnemiesKilled} - Total Entities: {objects.Count} - Evolution Factor: {EvolutionFactorString}");
            }, null, 1000, 1000);

            int[] frames = new int[100];
            int c = 0;
            DateTime dateTime = DateTime.Now;
            while (window.IsOpen)
            {
                window.DispatchEvents();

                window.Clear(Color.Blue);
                TimeSpan timeSpan = DateTime.Now - dateTime;
                dateTime = DateTime.Now;

                if (!MainBase.Available) //While the game is running
                {
                    timePlayed += (long)timeSpan.TotalMilliseconds;
                }

                objects = objects.OrderByDescending(o => o.renderLayer).ToList();
                for (int i = 0; i < objects.Count; i++)
                {
                    Entity item = objects[i];
                    window.Draw(item.Update(timeSpan.TotalMilliseconds));
                }

                foreach (var item in shapes)
                {
                    window.Draw(item);
                }

                //Do collision detection
                foreach (var item in objects)
                {
                    foreach (var type in item.Collides)
                    {
                        var possible = from i in objects
                                       where i.GetType() == type
                                       select i;
                        foreach (var other in possible)
                        {
                            if (PolygonCollision(item, other))
                            {
                                item.Collision(other);
                            }
                        }
                    }
                }

                //now handle additions or deletions
                lock (toChange)
                {
                    foreach (var item in toChange)
                    {
                        if (objects.Exists(x => x == item)) objects.Remove(item);
                        else objects.Add(item);
                    }
                    toChange.Clear();
                }

                frames[c] = (int)(1000f / timeSpan.TotalMilliseconds);
                int fps = 0;
                for (int i = 0; i < frames.Length; i++)
                {
                    fps = (fps + frames[i])/2;
                }
                text.DisplayedString = $"{fps} FPS";
                evolutionFactorText.DisplayedString = EvolutionFactorString;
                window.Draw(text);
                window.Draw(evolutionFactorText);
                window.Draw(moneyText);

                c++;
                c %= 100;

                window.Display();
                if (GameEnded)
                {
                    break;
                }
                Thread.Sleep(2);
            }
            Text gameOver = new Text("Game Over!", font, 64)
            {
                Position = new Vector2f(size.X / 2 - 100, size.Y / 2 - 64),
                Color = Color.Red
            };
            window.Display();
            while (window.IsOpen)
            {
                Thread.Sleep(10);
                window.DispatchEvents();
                window.Clear(Color.Blue);
                for (int i = 0; i < objects.Count; i++)
                {
                    Entity item = objects[i];
                    window.Draw(item.Update(0));
                }
                window.Draw(gameOver);
                window.Display();
            }
        }

        public static bool IsFree(float X, float Y, float Width, float Height)
        {
            Entity entity = new Entity(X - Width / 2, Y - Height / 2, new RectangleShape(new Vector2f(Width, Height)));
            foreach (var en in objects)
            {
                if (PolygonCollision(en, entity))
                    return false;
            }
            return true;
        }

        public static Entity GetEntityAt(float X, float Y, List<Entity> list)
        {
            list = list.OrderBy(l => l.clickLayer).ToList();
            foreach (var item in list)
            {
                var rectangleShape = item.shape as RectangleShape;
                if (((item.position.X <= X) && (item.position.X + rectangleShape.Size.X > X)) && (item.position.Y <= Y && item.position.Y + rectangleShape.Size.Y > Y))
                {
                    if (!(item is GUI g) || g.shown)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private static void Window_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            var clicked = GetEntityAt(e.X, e.Y, objects);

            if (clicked is null)
            {
                if (ToCreate != null && (!(ToCreate is MachineGun) || MachineGun.Available))
                {
                    if(ToCreate.cost <= money && IsFree(e.X, e.Y, (ToCreate.shape as RectangleShape).Size.X, (ToCreate.shape as RectangleShape).Size.Y))
                    { 
                        Entity item = ToCreate.Create(e.X - 5, e.Y - 5);
                        lock (playerBuildings)
                        {
                            playerBuildings.Add(item);
                        }
                        toChange.Add(item);
                    }
                }
            }
            else
            {
                clicked.OnClick(e.X, e.Y);
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

        public static float Scale(float x, float y, float speed)
        {
            return (float)Math.Sqrt((Math.Pow(speed, 2) / ((Math.Pow(x, 2) + Math.Pow(y, 2)))));
        }
        public static float Distance(Vector2f a, Vector2f b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static float Distance(Entity a, Entity b)
        {
            return Distance(a.position, b.position);
        }

        public static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            else
            {
                return minA - maxB;
            }
        }

        static Vector2f Normalize(Vector2f source)
        {
            float length = (float)Math.Sqrt((source.X * source.X) + (source.Y * source.Y));
            if (length != 0)
                return new Vector2f(source.X / length, source.Y / length);
            else
                return source;
        }

        static float DotProduct(Vector2f a, Vector2f b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static void ProjectPolygon(Vector2f axis, List<Vector2f> polygon,
                           ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            float dotProduct = DotProduct(axis, polygon[0]);
            min = dotProduct;
            max = dotProduct;
            for (int i = 0; i < polygon.Count; i++)
            {
                dotProduct = DotProduct(polygon[i], axis);
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else if (dotProduct > max)
                {
                    max = dotProduct;
                }
            }
        }

        public static bool PolygonCollision(Entity polygonA,
                                    Entity polygonB)
        {
            bool intersect = true;

            List<Vector2f> pointsA = polygonA.Points.ToList();
            List<Vector2f> pointsB = polygonB.Points.ToList();
            int edgeCountA = pointsA.Count();
            int edgeCountB = pointsB.Count();
            Vector2f edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = new Vector2f(pointsA[edgeIndex].X - pointsA[edgeIndex - 1 >= 0 ? edgeIndex - 1 : pointsA.Count - 1].X,
                        pointsA[edgeIndex].Y - pointsA[edgeIndex - 1 >= 0 ? edgeIndex - 1 : pointsA.Count - 1].Y);//polygonA.Edges[edgeIndex];
                }
                else
                {
                    //edge = polygonB.Edges[edgeIndex - edgeCountA];
                    edge = new Vector2f(pointsB[edgeIndex - edgeCountA].X - pointsB[edgeIndex - edgeCountA - 1 >= 0 ? edgeIndex - edgeCountA - 1 : pointsB.Count - 1].X,
                       pointsB[edgeIndex - edgeCountA].Y - pointsB[edgeIndex - edgeCountA - 1 >= 0 ? edgeIndex - edgeCountA - 1 : pointsB.Count - 1].Y);
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector2f axis = new Vector2f(-edge.Y, edge.X);
                axis = Normalize(axis);

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, pointsA, ref minA, ref maxA);
                ProjectPolygon(axis, pointsB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(minA, maxA, minB, maxB) > 0)
                    intersect = false;

            }
            return intersect;
        }
        //View https://www.codeproject.com/Articles/15573/2D-Polygon-Collision-Detection?fid=340668&df=90&mpp=25&sort=Position&spc=Relaxed&prof=True&view=Normal&fr=26#xx0xx
        public static bool IsEntityInterceptingOld(Entity a, Entity b)
        {
            foreach (var polygon in new[] { a, b })
            {
                List<Vector2f> points = a.Points.ToList();
                for (int i1 = 0; i1 < points.Count; i1++)
                {
                    int i2 = (i1 + 1) % points.Count;
                    var p1 = points[i1];
                    var p2 = points[i2];

                    var normal = new Vector2f(p2.Y - p1.Y, p1.X - p2.X);

                    double? minA = null, maxA = null;
                    foreach (var p in a.Points)
                    {
                        var projected = normal.X * p.X + normal.Y * p.Y;
                        if (minA == null || projected < minA)
                            minA = projected;
                        if (maxA == null || projected > maxA)
                            maxA = projected;
                    }

                    double? minB = null, maxB = null;
                    foreach (var p in b.Points)
                    {
                        var projected = normal.X * p.X + normal.Y * p.Y;
                        if (minB == null || projected < minB)
                            minB = projected;
                        if (maxB == null || projected > maxB)
                            maxB = projected;
                    }

                    if (maxA < minB || maxB < minA)
                        return false;
                }
            }
            return true;
        }
    }
}
