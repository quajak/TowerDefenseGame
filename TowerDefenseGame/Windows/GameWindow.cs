using SFML.Graphics;
using SFML.System;
using System;
using System.Linq;
using System.Threading;

using static TowerDefenseGame.Program;

namespace TowerDefenseGame
{
    class GameWindow : Window
    {
        private static float baseEvolutionFactor = 1f;

        public static Stat Speed = new Stat(1.0f);

        public static float EvolutionFactor
        {
            get
            {
                return baseEvolutionFactor * Math.Max(1f, (float)Math.Log10(EnemiesKilled + 1)) * Math.Max(1f, TimePlayed / 60000f); //60 Sekunden bevor die Enemies wegen der Zeit rampen
            }
        }

        public static string EvolutionFactorString
        {
            get
            {
                float evolutionFactor = EvolutionFactor;
                float eP = baseEvolutionFactor * Math.Max(1f, (float)Math.Log10(EnemiesKilled + 1)) / evolutionFactor * 100;
                float tP = baseEvolutionFactor * Math.Max(1f, TimePlayed / 100000f) / evolutionFactor * 100;
                return $"{evolutionFactor.ToString("0.00")} - {eP.ToString("0")}% {tP.ToString("0")}%";
            }
        }

        Text evolutionFactorText;
        Text fpsCounter;
        GuiButton pause;

        int frameLength;
        int[] frames;
        int c;
        DateTime dateTime;

        Sprite background;

        private static Text moneyText;

        public GameWindow(int difficulty) : base()
        {
            Money = difficulty * 8;
            baseEvolutionFactor = (difficulty + 1) / 2f;
            MoneyRate = (difficulty + 2) / 3;

            TowerGUI = new TowerOverViewGUI();
            Objects.Add(TowerGUI);
            Objects.Add(new MenuGUI());

            fpsCounter = new Text("", Program.Font, 32)
            {
                Color = Color.Red,
                Position = new Vector2f(1000, 10)
            };

            evolutionFactorText = new Text("", Program.Font, 32)
            {
                Color = Color.Red,
                Position = new Vector2f(1200, 10)
            };

            moneyText = new Text("", Program.Font, 32)
            {
                Position = new Vector2f(800, 10)
            };

            pause = new GuiButton(120, 10, 30, 30, new Color(0, 0, 0, 0), "||");
            pause.Click += Pause_Click;
            Objects.Add(pause);

            frameLength = 100; // When two few frames are used, the fluctuations make it unreadable
            frames = new int[frameLength];
            c = 0;
            dateTime = DateTime.Now;

            //Setup background graphics
            background = new Sprite(new Texture("./Resources/background.png"));
            background.Texture.Repeated = true;
            background.TextureRect = new IntRect(0, 0, (int)GameSize.X, (int)GameSize.Y);
        }

        private void Pause_Click(object sender, SFML.Window.MouseButtonEventArgs e)
        {
            if (Speed.Modifiers.Exists(m => m.name == "Pause"))
            {
                Speed.Modifiers.RemoveAll(m => m.name == "Pause");
                pause.Content = "||";
            }
            else
            {
                pause.Content = "|>";
                Speed.Modifiers.Add(new Modifier(ModifierType.Absolute, 0f, "Pause"));
            }
        }

        public override Window Run(RenderWindow window)
        {
            while (window.IsOpen) // Main game loop
            {
                window.DispatchEvents(); // Here all event handlers are called

                TimeSpan timeSpan = DateTime.Now - dateTime;
                dateTime = DateTime.Now;
                double totalMilliseconds = timeSpan.TotalMilliseconds; //Extreme lags or other interruptions cause too large delays for the game to handle
                totalMilliseconds = totalMilliseconds > 100 ? 100 : totalMilliseconds; //so we cut at 0.1s
                totalMilliseconds *= Speed.Value;
                timeSpan = new TimeSpan(0, 0, 0, 0, (int)totalMilliseconds);

                if (!MainBase.Available) //While the game is running
                {
                    TimePlayed += (long)timeSpan.TotalMilliseconds;
                    Enemy.GenerateEnemies((float)timeSpan.TotalMilliseconds);
                }

                //Start rendering + entity updates
                window.Clear(Color.Blue);

                window.Draw(background);
                Objects = Objects.OrderBy(o => o.renderLayer).ToList();
                for (int i = 0; i < Objects.Count; i++)
                {
                    Entity item = Objects[i];
                    window.Draw(item.Update(totalMilliseconds));
                }

                foreach (var item in Shapes)
                {
                    window.Draw(item);
                }

                //Do collision detection
                foreach (var item in Objects)
                {
                    foreach (var type in item.Collides)
                    {
                        var possible = from i in Objects
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

                foreach (var text in Texts)
                {
                    window.Draw(text);
                }

                //now handle additions or deletions
                lock (ToChange)
                {
                    if (ToChange.Count != ToChange.Distinct().Count())
                        throw new Exception("Dublicates exist in to change!");
                    foreach (var item in ToChange)
                    {
                        if (Objects.Exists(x => x == item)) Objects.Remove(item);
                        else Objects.Add(item);
                    }
                    ToChange.Clear();
                }

                //Get FPS counter
                frames[c] = (int)(1000 / timeSpan.TotalMilliseconds);
                int fps = 0;
                for (int i = 0; i < frames.Length; i++)
                {
                    fps = (fps + frames[i]) / 2;
                }
                c++;
                c %= frameLength;
                //Dirty hack so we dont show stupid numbers
                fps = Math.Max(0, Math.Min(fps, 500));
                fpsCounter.DisplayedString = $"{fps} FPS";

                //Update texts
                evolutionFactorText.DisplayedString = EvolutionFactorString;
                moneyText.DisplayedString = $"${Money}";
                window.Draw(fpsCounter);
                window.Draw(evolutionFactorText);
                window.Draw(moneyText);

                window.Display();
                if (GameEnded)
                {
                    return new GameOverWindow();
                }

                //Debug checks
                if (Enemies.Exists(e => (e as Enemy).Size == 0))
                {
                    throw new Exception("Enemy with 0 size found!");
                }

                Thread.Sleep(2);
            }
            return null;
        }
    }
}
