using SFML.Graphics;
using SFML.System;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefenseGame.Towers;

namespace TowerDefenseGame
{
    partial class GameWindow : Window
    {
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
            Program.Objects.Add(t);
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

    }
}
