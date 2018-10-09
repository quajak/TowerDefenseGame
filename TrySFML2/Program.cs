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
        public static GameEvent gameEvent = GameEvent.None;
        static RenderWindow window;

        public static List<Entity> objects = new List<Entity>();
        public static List<Entity> toChange = new List<Entity>();
        public static List<Entity> enemies = new List<Entity>();
        public static List<Entity> playerBuildings = new List<Entity>();
        public static List<Shape> shapes = new List<Shape>(); //keep for debug reasons
        public static List<Text> texts = new List<Text>();

        public static Random random = new Random();

        public static Vector2u gameSize;

        public static Tower ToCreate = new MachineGun(0, 0);
        public static Font font = new Font("arial.ttf");

        private static int money = 10;
        static Text moneyText = new Text("", font, 32)
        {
            Position = new Vector2f(800, 10)
        };

        public static int EnemiesKilled = 0;
        public static long timePlayed = 0;

        static float baseEvolutionFactor = 1f;

        public static float EvolutionFactor {
            get
            {
                return baseEvolutionFactor * Math.Max(1f, (float)Math.Log10(EnemiesKilled + 1)) * Math.Max(1f, timePlayed / 60000f); //60 Sekunden bevor die Enemies wegen der Zeit rampen
            }
        }

        public static string EvolutionFactorString
        {
            get
            {
                float evolutionFactor = EvolutionFactor;
                float eP = baseEvolutionFactor * Math.Max(1f, (float)Math.Log10(EnemiesKilled + 1)) / evolutionFactor * 100;
                float tP = baseEvolutionFactor * Math.Max(1f, timePlayed / 100000f) / evolutionFactor * 100;
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
            gameSize = window.Size;
            window.Closed += Window_Closed;
            window.GainedFocus += Window_GainedFocus;
            window.LostFocus += Window_LostFocus;
            window.MouseButtonPressed += Window_MouseButtonPressed;

            MainMenuGUI mainMenu = new MainMenuGUI();
            objects.Add(mainMenu);
            while (window.IsOpen) //Pre game loop
            {
                window.DispatchEvents(); // Here all event handlers are called
                window.Clear(Color.Black);

                //Start rendering + entity updates
                window.Clear(Color.Blue);
                objects = objects.OrderByDescending(o => o.renderLayer).ToList();
                for (int i = 0; i < objects.Count; i++)
                {
                    Entity item = objects[i];
                    window.Draw(item.Update(0));
                }

                foreach (var item in shapes)
                {
                    window.Draw(item);
                }
                foreach (var text in texts)
                {
                    window.Draw(text);
                }
                window.Display();
                Thread.Sleep(10);
                if(gameEvent == GameEvent.Next)
                {
                    mainMenu.Delete();
                    objects.Remove(mainMenu);
                    break;
                }
            }
            // now update to fit difficulty
            money += mainMenu.difficulty * 8;
            baseEvolutionFactor = mainMenu.difficulty / 2f;

            objects.Add(new MenuGUI());

            Text fpsCounter = new Text("", font, 32)
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
                    lock (playerBuildings) // cam this cause a deadlock? Maybe - most likely not
                    {
                        int num = random.Next(3 + (int)EvolutionFactor);
                        for (int i = 0; i < num; i++)
                        {
                            int eSize = random.Next(1, (int)EvolutionFactor);
                            int tries = 0;
                            while (tries++ < 100) // So we dont get stuck for ever
                            {
                                int posX = random.Next((int)gameSize.X);
                                int posY = random.Next((int)gameSize.Y);
                                var leastDistance = playerBuildings.Select(b => b.position).Select(p => E.Distance(p, new Vector2f(posX, posY))).Min();
                                if(leastDistance > 100) // TOOO: different towers have different regions of effect
                                {
                                    Enemy item1 = new Enemy(posX, posY, eSize);
                                    enemies.Add(item1);
                                    toChange.Add(item1);
                                    break;
                                }
                            }
                        }
                    }
                Console.WriteLine($"Debug at {timePlayed / 1000} - Enemies: {enemies.Count} - Killed: {EnemiesKilled} - Total Entities: {objects.Count} - Evolution Factor: {EvolutionFactorString}");
            }, null, 1000, 1000);

            int frameLength = 100; // When two few frames are used, the fluctuations make it unreadable
            int[] frames = new int[frameLength];
            int c = 0;
            DateTime dateTime = DateTime.Now;
            while (window.IsOpen) // Main game loop
            {
                window.DispatchEvents(); // Here all event handlers are called


                TimeSpan timeSpan = DateTime.Now - dateTime;
                dateTime = DateTime.Now;
                double totalMilliseconds = timeSpan.TotalMilliseconds; //Extreme lags or other interruptions cause too large delays for the game to handle
                totalMilliseconds = totalMilliseconds > 100 ? 100 : totalMilliseconds; //so we cut at 0.1s
                timeSpan = new TimeSpan(0, 0, 0, 0, (int)totalMilliseconds);

                if (!MainBase.Available) //While the game is running
                {
                    timePlayed += (long)timeSpan.TotalMilliseconds;
                }

                //Start rendering + entity updates
                window.Clear(Color.Blue);
                objects = objects.OrderByDescending(o => o.renderLayer).ToList();
                for (int i = 0; i < objects.Count; i++)
                {
                    Entity item = objects[i];
                    window.Draw(item.Update(totalMilliseconds));
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

                //Get FPS counter
                frames[c] = (int)(1000 / timeSpan.TotalMilliseconds);
                int fps = 0;
                for (int i = 0; i < frames.Length; i++)
                {
                    fps = (fps + frames[i])/2;
                }
                c++;
                c %= frameLength;
                fpsCounter.DisplayedString = $"{fps} FPS";

                //Update texts
                evolutionFactorText.DisplayedString = EvolutionFactorString;
                window.Draw(fpsCounter);
                window.Draw(evolutionFactorText);
                window.Draw(moneyText);


                window.Display();
                if (GameEnded)
                {
                    break;
                }
                Thread.Sleep(2);
            }
            //After game is over code
            Text gameOver = new Text("Game Over!", font, 64)
            {
                Position = new Vector2f(gameSize.X / 2 - 100, gameSize.Y / 2 - 64),
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

        public enum GameEvent { Next, None };
    }
}
