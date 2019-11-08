using TowerDefenseGame.Maps;
using TowerDefenseGame.Towers;
using SFML.Graphics;
using SFML.System;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using static TowerDefenseGame.Program;

namespace TowerDefenseGame
{
    internal class GameWindow : Window, IDisposable
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

        private readonly Text evolutionFactorText;
        private readonly Text fpsCounter;
        private readonly GuiButton pause;
        private readonly int frameLength;
        private readonly int[] frames;
        private int c;
        private DateTime dateTime;
        private readonly Sprite background;
        public static NavMeshQuery navMeshQuery;
        private static NavQueryFilter filter;

        private static Text moneyText;

        public GameWindow(int difficulty, Map map) : base()
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
#pragma warning disable CA2000 // Dispose objects before losing scope
            background = new Sprite(new Texture("./Resources/background.png"));
#pragma warning restore CA2000 // Dispose objects before losing scope
            background.Texture.Repeated = true;
            background.TextureRect = new IntRect(0, 0, (int)GameSize.X, (int)GameSize.Y);

            //Generate terrain
            foreach (var item in map.GenerateTerrain())
            {
                Objects.Add(item);
                Terrains.Add(item);
            }

            //Generate path finding mesh

            List<Vertex> vertexes = new List<Vertex>();
            int step = 20;
            int columnNumber = (int)window.Size.X / step;
            int rowNumber = (int)window.Size.Y / step;
            for (int y = 0; y < rowNumber; y++)
            {
                for (int x = 0; x < columnNumber; x++)
                {
                    int xOffset = y % 2 == 0 ? 10 : 0;
                    bool b = GetEntityAt(x * step + xOffset, y * step, Terrains) == null;
                    vertexes.Add(new Vertex(new Vector2f(x * step + xOffset, y * step), b ? Color.Yellow : Color.Black));
                }
            }
            List<Triangle3> triangleValues = new List<Triangle3>();
            for (int y = 1; y < rowNumber - 1; y++)
            {
                // Triangles are facing downward
                for (int x = 0; x < columnNumber - 1; x++)
                {
                    Vector3 vec1 = vertexes[x + y * columnNumber].Position.ToV3();
                    Vector3 vec2 = vertexes[x + 1 + y * columnNumber].Position.ToV3();
                    Vector3 vec3 = vertexes[x + (1 + y) * columnNumber].Position.ToV3();
                    if (vertexes[x + y * columnNumber].Color == Color.Black ||
                        vertexes[x + y * columnNumber].Color == Color.Black ||
                        vertexes[x + y * columnNumber].Color == Color.Black)
                    {
                    }
                    else
                    {
                        triangleValues.Add(new Triangle3(vec1, vec2, vec3));
                    }
                }
                //Triangles are facing upward
                for (int x = 0; x < columnNumber - 1; x++)
                {
                    Vector3 vec1 = vertexes[x + y * columnNumber].Position.ToV3();
                    Vector3 vec2 = vertexes[x + 1 + y * columnNumber].Position.ToV3();
                    Vector3 vec3 = vertexes[x + 1 + (y - 1) * columnNumber].Position.ToV3();
                    if (vertexes[x + y * columnNumber].Color == Color.Black ||
                        vertexes[x + y * columnNumber].Color == Color.Black ||
                        vertexes[x + y * columnNumber].Color == Color.Black)
                    {
                    }
                    else
                    {
                        triangleValues.Add(new Triangle3(vec1, vec2, vec3));
                    }
                }
            }
            filter = new NavQueryFilter();
            Thread th = new Thread(() => GenerateMesh(triangleValues));
            th.Start();
            //foreach (var item in mesh.Polys)
            //{
            //    ConvexShape convex = new ConvexShape((uint)item.VertexCount)
            //    {
            //        FillColor = Color.Yellow
            //    };
            //    for (int i = 0; i < item.VertexCount; i++)
            //    {
            //        PolyVertex polyVertex = mesh.Verts[item.Vertices[i]];
            //        convex.SetPoint((uint)i, new Vector2f(polyVertex.X, polyVertex.Z));
            //    }
            //    Shapes.Add(convex);
            //}
            //for (int i = 0; i < 200; i++)
            //{
            //    Vector3 center = new Vector3(Program.Random.Next(50, (int)Program.window.Size.X), 1,
            //        Program.Random.Next(50, (int)Program.window.Size.X));
            //    Vector3 end = new Vector3(Program.Random.Next(50, (int)Program.window.Size.X), 1,
            //        Program.Random.Next(50, (int)Program.window.Size.X));
            //    try
            //    {
            //        ShowPath(filter, center, end);
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
            //VertexArray vertexArray = new VertexArray(PrimitiveType.Points, (uint)vertexes.Count);
            //for (uint i = 0; i < vertexes.Count; i++)
            //{
            //    vertexArray[i] = vertexes[(int)i];
            //}
            //Terrain t = new Terrain(0, 0, vertexArray, new List<Vector2f>()) { renderLayer = 120 };
            //Objects.Add(t);
            //VertexArray triangles = new VertexArray(PrimitiveType.Triangles, (uint)triangleValues.Count * 3);
            //for (int i = 0; i < triangleValues.Count * 3; i += 3)
            //{
            //    Triangle3 triangle = triangleValues[i / 3];
            //    Vertex v = new Vertex(triangle.A.ToV2(), Color.Red);
            //    triangles[(uint)i] = v;
            //    triangles[(uint)i + 1] = new Vertex(triangle.B.ToV2(), Color.Red);
            //    triangles[(uint)i + 2] = new Vertex(triangle.C.ToV2(), Color.Red);
            //}
            //Terrain t = new Terrain(0, 0, triangles, new List<Vector2f>());
            //Objects.Add(t);
        }

        ~GameWindow()
        {
            background.Dispose();
            evolutionFactorText.Dispose();
            evolutionFactorText.Dispose();
            fpsCounter.Dispose();
        }

        private static void GenerateMesh(List<Triangle3> triangleValues)
        {
            var tria = TriangleEnumerable.FromTriangle(triangleValues.ToArray(), 0, triangleValues.Count);
            BBox3 bounds = tria.GetBoundingBox();
            var settings = NavMeshGenerationSettings.Default;
            settings.AgentHeight = 1f;
            settings.AgentRadius = 1f;
            settings.CellHeight = 1f;
            settings.CellSize = 1f;
            var heightfield = new Heightfield(bounds, settings);
            heightfield.RasterizeTriangles(triangleValues, Area.Default);
            heightfield.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            heightfield.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);
            heightfield.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);
            var compactHeightfield = new CompactHeightfield(heightfield, settings);
            compactHeightfield.Erode(settings.VoxelAgentRadius);
            compactHeightfield.BuildDistanceField();
            compactHeightfield.BuildRegions(0, settings.MinRegionSize, settings.MergedRegionSize);
            var contourSet = compactHeightfield.BuildContourSet(settings);

            var mesh = new PolyMesh(contourSet, settings);
            var polyMeshDetail = new PolyMeshDetail(mesh, compactHeightfield, settings);
            var buildData = new NavMeshBuilder(mesh, polyMeshDetail, Array.Empty<OffMeshConnection>(), settings);
            var tiledNavMesh = new TiledNavMesh(buildData);
            Console.WriteLine("Finished creating the mesh!");
            navMeshQuery = new NavMeshQuery(tiledNavMesh, 2048);
        }

#pragma warning disable IDE0051 // Remove unused private members

        private static Terrain ShowPath(Vector3 start, Vector3 end)
#pragma warning restore IDE0051 // Remove unused private members
        {
            List<Vector3> p = FindPath(start, end);
            VertexArray vP = new VertexArray(PrimitiveType.LinesStrip, (uint)(p.Count / 10));
            for (int i = 0; i < p.Count / 10; i++)
            {
                Vertex vertex = new Vertex(p[10 * i].ToV2(), Color.Green);
                vP[(uint)i] = vertex;
            }
            Terrain t = new Terrain(0, 0, vP, new List<Vector2f>()) { renderLayer = 120 };
            Objects.Add(t);
            return t;
        }

        public static List<Vector3> FindPath(Vector3 start, Vector3 end)
        {
            Vector3 e = new Vector3(40, 40, 40);
            navMeshQuery.FindNearestPoly(ref start, ref e, out NavPoint startPt);
            navMeshQuery.FindNearestPoly(ref end, ref e, out NavPoint endPt);

            Path path = new Path();
            navMeshQuery.FindPath(ref startPt, ref endPt, filter, path);
            return FindPath(startPt, endPt, path);
        }

        private static List<Vector3> FindPath(NavPoint startPt, NavPoint endPt, Path path)
        {
            //find a smooth path over the mesh surface
            int npolys = path.Count;
            Vector3 iterPos = new Vector3();
            Vector3 targetPos = new Vector3();
            navMeshQuery.ClosestPointOnPoly(startPt.Polygon, startPt.Position, ref iterPos);
            navMeshQuery.ClosestPointOnPoly(path[npolys - 1], endPt.Position, ref targetPos);

            var smoothPath = new List<Vector3>(2048)
            {
                iterPos
            };

            float STEP_SIZE = 5f;
            float SLOP = 0.1f;
            while (npolys > 0 && smoothPath.Count < smoothPath.Capacity)
            {
                //find location to steer towards
                Vector3 steerPos = new Vector3();
                StraightPathFlags steerPosFlag = 0;
                NavPolyId steerPosRef = NavPolyId.Null;

                if (!GetSteerTarget(navMeshQuery, iterPos, targetPos, SLOP, path, ref steerPos, ref steerPosFlag, ref steerPosRef))
                    break;

                bool endOfPath = (steerPosFlag & StraightPathFlags.End) != 0 ? true : false;
                bool offMeshConnection = (steerPosFlag & StraightPathFlags.OffMeshConnection) != 0 ? true : false;

                //find movement delta
                Vector3 delta = steerPos - iterPos;
                float len = (float)Math.Sqrt(Vector3.Dot(delta, delta));

                //if steer target is at end of path or off-mesh link
                //don't move past location
                if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                    len = 1;
                else
                    len = STEP_SIZE / len;

                Vector3 moveTgt = new Vector3();
                E.VMad(ref moveTgt, iterPos, delta, len);

                //move
                List<NavPolyId> visited = new List<NavPolyId>(16);
                NavPoint startPoint = new NavPoint(path[0], iterPos);
                navMeshQuery.MoveAlongSurface(ref startPoint, ref moveTgt, out Vector3 result, visited);
                path.FixupCorridor(visited);
                npolys = path.Count;
                float h = 0;
                navMeshQuery.GetPolyHeight(path[0], result, ref h);
                result.Y = h;
                iterPos = result;

                //handle end of path when close enough
                if (endOfPath && E.InRange(iterPos, steerPos, SLOP, 1.0f))
                {
                    //reached end of path
                    iterPos = targetPos;
                    if (smoothPath.Count < smoothPath.Capacity)
                    {
                        smoothPath.Add(iterPos);
                    }
                    break;
                }

                //store results
                if (smoothPath.Count < smoothPath.Capacity)
                {
                    smoothPath.Add(iterPos);
                }
            }
            return smoothPath;
        }

        private static bool GetSteerTarget(NavMeshQuery navMeshQuery, Vector3 startPos, Vector3 endPos, float minTargetDist, Path path,
            ref Vector3 steerPos, ref StraightPathFlags steerPosFlag, ref NavPolyId steerPosRef)
        {
            StraightPath steerPath = new StraightPath();
            navMeshQuery.FindStraightPath(startPos, endPos, path, steerPath, 0);
            int nsteerPath = steerPath.Count;
            if (nsteerPath == 0)
                return false;

            //find vertex far enough to steer to
            int ns = 0;
            while (ns < nsteerPath)
            {
                if ((steerPath[ns].Flags & StraightPathFlags.OffMeshConnection) != 0 ||
                    !E.InRange(steerPath[ns].Point.Position, startPos, minTargetDist, 1000.0f))
                    break;

                ns++;
            }

            //failed to find good point to steer to
            if (ns >= nsteerPath)
                return false;

            steerPos = steerPath[ns].Point.Position;
            steerPos.Y = startPos.Y;
            steerPosFlag = steerPath[ns].Flags;
            if (steerPosFlag == StraightPathFlags.None && ns == (nsteerPath - 1))
                steerPosFlag = StraightPathFlags.End; // otherwise seeks path infinitely!!!
            steerPosRef = steerPath[ns].Point.Polygon;

            return true;
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            background.Dispose();
            evolutionFactorText.Dispose();
            evolutionFactorText.Dispose();
            fpsCounter.Dispose();
        }
    }
}