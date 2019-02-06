using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefenseGame
{
    internal class Enemy : Entity
    {
        private Stat vX;
        private Stat vY;
        private int size;
        private bool colorSet = false;
        private Stat speed;
        public readonly Vector2f spawnPoint;
        private List<Type> damagedBy = new List<Type>();
        bool _removed = false;

        public float DistanceToGoal
        {
            get
            {
                if (!MainBase.Available)
                {
                    return E.Distance(this, Program.mainBase);
                }
                return 100f;
            }
        }

        public int Size
        {
            get => size; set
            {
                size = value;
                if (size != 0)
                {
                    var s = shape as RectangleShape;
                    s.Size = new Vector2f(size * 2 + 5, size * 2 + 5);
                    shape = s;
                    if (!colorSet)
                    {
                        shape.FillColor = new Color((byte)Program.Random.Next(255), (byte)Program.Random.Next(255), (byte)Program.Random.Next(255));
                    }
                }
            }
        }

        internal Stat VX
        {
            get => vX; set
            {
                vX = value;
                if (speed != null && vY != null)
                {
                    float s = E.Scale(value.baseValue, vY.Value, Speed.Value);
                    vY.baseValue = value.baseValue * s;
                    vX.baseValue = vX.baseValue * s;
                    velocity = new Vector2f(vX.Value, vY.Value);
                }
            }
        }

        internal Stat VY
        {
            get => vY; set
            {
                vY = value;
                if (speed != null && vX != null)
                {
                    float s = E.Scale(vY.baseValue, value.baseValue, Speed.Value);
                    vY.baseValue = value.baseValue * s;
                    vX.baseValue = vX.baseValue * s;
                    velocity = new Vector2f(vX.Value, vY.Value);
                }
            }
        }

        internal Stat Speed
        {
            get => speed; set
            {
                speed = value;
                if (vX != null && vY != null)
                {
                    float s = E.Scale(vX.baseValue, vY.Value, speed.Value);
                    vY.baseValue = vY.baseValue * s;
                    vX.baseValue = vX.baseValue * s;
                    velocity = new Vector2f(vX.Value, vY.Value);
                }
            }
        }

        public Enemy(int aX, int aY, int size = 1) : base(aX, aY, new RectangleShape(new Vector2f(5, 5)))
        {
            renderLayer = 1;
            if (Program.mainBase is null)
            {
                VX = new Stat((float)Program.Random.NextDouble() - 0.5f);
                VY = new Stat((float)Program.Random.NextDouble() - 0.5f);
            }
            else
            {
                VX = new Stat(Program.mainBase.position.X - aX);
                VY = new Stat(Program.mainBase.position.Y - aY);
            }
            Speed = new Stat(20 * GameWindow.EvolutionFactor);
            float s = E.Scale(VX.Value, VY.Value, Speed.Value);
            VX.baseValue *= s;
            VY.baseValue *= s;
            velocity = new Vector2f(VX.Value, VY.Value);
            Size = size;
            spawnPoint = position;
            Collides.Add(typeof(Bullet));
            Collides.Add(typeof(MainBase));
            Collides.Add(typeof(Lazor));
            Collides.Add(typeof(Bomb));
            Collides.Add(typeof(Explosion));
            Collides.Add(typeof(Mine));
        }

        public Enemy(int aX, int aY, Color color, int size = 1) : this(aX, aY, size)
        {
            colorSet = true;
            shape.FillColor = color;
        }

        public override void Collision(Entity collided)
        {
            switch (collided)
            {
                case Bullet b:
                    if (_removed)
                        break;
                    float damageDealt = Math.Min(Math.Min(b.damage, b.pierce), Size);
                    b.pierce -= damageDealt;
                    if (b.pierce <= 0)
                    {
                        if (!Program.ToChange.Contains(b))
                        {
                            b.End();
                            lock (Program.ToChange)
                            {
                                Program.ToChange.Add(b);
                            }
                        }
                    }
                    Pop(b.CreatorType, (int)damageDealt);
                    break;

                case MainBase m:
                    Console.WriteLine(" --- Killer Info --- ");
                    Console.WriteLine($"Spawnpoint: {spawnPoint.X}, {spawnPoint.Y}");
                    Console.WriteLine($"Current position: {position.X}, {position.Y}");
                    Console.WriteLine($"Size: {size}");
                    Console.WriteLine($"Speed: {Speed.Value}");
                    Console.WriteLine($"Affected by: {string.Join(" ", damagedBy.ConvertAll(d => d.Name))}");
                    Console.WriteLine(" ------------------- ");
                    Program.GameEnded = true;
                    break;

                case Mine m:
                    int damage = Math.Min(Size, m.DamageToDo);
                    m.DamageToDo -= damage;
                    Pop(m.GetType(), damage);

                    break;

                case Lazor l:
                    Pop(typeof(LazorGun));
                    break;

                case Bomb b:
                    Pop(typeof(Bomb));
                    break;

                case Explosion e:
                    damageDealt = Math.Min(e.Damage, Size);
                    if (e.Active)
                    {
                        Pop(typeof(Explosion), (int)damageDealt);
                    }
                    break;

                default:
                    break;
            }
        }

        public void Pop(Type dealer, int amount = 1)
        {
            Statistics.AddDamage(amount, dealer);
            damagedBy.Add(dealer);
            Size -= amount;
            if (Size <= 0)
            {
                lock (Program.ToChange)
                {
                    if (!Program.ToChange.Contains(this))
                    {
                        _removed = true;
                        Program.ToChange.Add(this);
                        Program.Enemies.Remove(this);
                    }
                }
                Program.EnemiesKilled++;
            }
        }

        public override Shape Update(double timeDiff)
        {
            if ((position.X < 0 || position.X > Program.GameSize.X) || (position.Y < 0 || position.Y > Program.GameSize.Y))
            {
                Program.ToChange.Add(this);
                Program.Enemies.Remove(this);
            }
            return base.Update(timeDiff);
        }

        static float timePassed = 0;
        static float timeSinceBoss = 0;
        private static int bossSize = 5;
        private static float timeBetweenBosses = 60_000;
        private static float lastTimePlayed = 0;
        public static void GenerateEnemies(float timeDiff)
        {
            timePassed += timeDiff;
            float interval = 1000f * GameWindow.Speed.Value;
            if (timePassed >= interval)
            {
                timePassed = 0;
                // the spawning rate oscialltes on a 60 second period - we shift the return valie of sin to always be above 0
                int num = 0;
                if (!MainBase.Available && !Program.GameEnded && GameWindow.Speed.Value != 0)
                {
                    lock (Program.ToChange)
                        lock (Program.Enemies)
                            lock (Program.PlayerBuildings) // cam this cause a deadlock? Maybe - most likely not
                            {

                                float val2 = 1.2f * (float)(Math.Sin((Program.TimePlayed / 1000) % 30f / 30f * 2f * (float)Math.PI) + 1f); //Modify the multiplier until it works well
                                num = Program.Random.Next((int)(GameWindow.EvolutionFactor / 10), 1 + Math.Max(1, (int)(GameWindow.EvolutionFactor * val2)));
                                for (int i = 0; i < num; i++)
                                {
                                    int eSize = Program.Random.Next(1, (int)GameWindow.EvolutionFactor);
                                    int tries = 0;
                                    bool spawnInField = false; //We normally spawn the enemies at the borders for a cooler effect but if you want to play with them spawning everywhere set this to true
                                    if (spawnInField)
                                    {
                                        while (tries++ < 100) // So we dont get stuck for ever
                                        {
                                            int posX = Program.Random.Next((int)Program.GameSize.X);
                                            int posY = Program.Random.Next((int)Program.GameSize.Y);
                                            var leastDistance = Program.PlayerBuildings.Select(b => b.position).Select(p => E.Distance(p, new Vector2f(posX, posY))).Min();
                                            if (leastDistance > 100) // TOOO: different towers have different regions of effect
                                            {
                                                Enemy item1 = new Enemy(posX, posY, eSize);
                                                Program.Enemies.Add(item1);
                                                Program.ToChange.Add(item1);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //We spawn on the border
                                        int posX = 0;
                                        int posY = 0;
                                        if (Program.Random.NextBool())
                                        {
                                            //spawn on x border
                                            posX = Program.Random.NextBool() ? 0 : (int)Program.GameSize.X;
                                            posY = Program.Random.Next((int)Program.GameSize.Y);
                                        }
                                        else
                                        {
                                            //spawn on y border
                                            posX = Program.Random.Next((int)Program.GameSize.X);
                                            posY = Program.Random.NextBool() ? 0 : (int)Program.GameSize.Y;
                                        }
                                        Enemy item1 = new Enemy(posX, posY, eSize);
                                        Program.Enemies.Add(item1);
                                        Program.ToChange.Add(item1);
                                    }
                                }

                                //Spawn boss every 60s
                                timeSinceBoss += Program.TimePlayed - lastTimePlayed;
                                lastTimePlayed = Program.TimePlayed;
                                if (timeSinceBoss > timeBetweenBosses)
                                {
                                    timeSinceBoss = 0;
                                    Console.WriteLine($"Spawning boss! Size {bossSize}");
                                    //Spawn boss at random location on field

                                    int tries = 0;
                                    while (tries++ < 100) // So we dont get stuck for ever
                                    {
                                        int posX = Program.Random.Next((int)Program.GameSize.X);
                                        int posY = Program.Random.Next((int)Program.GameSize.Y);
                                        var leastDistance = Program.PlayerBuildings.Select(b => b.position).Select(p => E.Distance(p, new Vector2f(posX, posY))).Min();
                                        if (leastDistance > 100) // TOOO: different towers have different regions of effect
                                        {
                                            Enemy boss = new Enemy(posX, posY, new Color(0, 0, 0), bossSize);
                                            Program.Enemies.Add(boss);
                                            Program.ToChange.Add(boss);
                                            //Spawn minions close to the boss
                                            for (int i = 1; i < bossSize; i++)
                                            {
                                                int spread = 16;
                                                Enemy item1 = new Enemy(posX + Program.Random.Next(bossSize * spread) - bossSize * spread / 2,
                                                        posY + Program.Random.Next(bossSize * spread) - bossSize * spread / 2, new Color(0, 0, 0), i);
                                                Program.Enemies.Add(item1);
                                                Program.ToChange.Add(item1);
                                            }
                                            bossSize += 5;
                                            break;
                                        }
                                    }
                                }

                            }
                }
                Console.WriteLine($"Debug at {Program.TimePlayed / 1000} - Enemies: {Program.Enemies.Count} + {num}- Killed: {Program.EnemiesKilled} - Total Entities: {Program.Objects.Count} - Evolution Factor: {GameWindow.EvolutionFactorString}");
            }
        }
    }
}