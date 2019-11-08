using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using TowerDefenseGame;

namespace TowerDefenseGame.Towers
{
    internal class Terrain : Entity
    {
        private const float extraBoundary = 5;
        public bool blocksBullets = true;

        public Terrain(float aX, float aY, float width = 200, float height = 100) : base(aX, aY, new VertexArray(PrimitiveType.TrianglesStrip, 4))
        {
            vertexArray[0] = new Vertex(new Vector2f(aX, aY), Color.Green);
            vertexArray[1] = new Vertex(new Vector2f(aX + width, aY), Color.Black);
            vertexArray[2] = new Vertex(new Vector2f(aX, aY + height), Color.Red);
            vertexArray[3] = new Vertex(new Vector2f(aX + width, aY + height), Color.Cyan);
            renderLayer = 40;
            logicalPoints = new List<Vector2f>
            {
                new Vector2f(aX - extraBoundary , aY - extraBoundary ),
                new Vector2f(aX + width + extraBoundary , aY - extraBoundary ),
                new Vector2f(aX + width + extraBoundary , aY + height + extraBoundary ),
                new Vector2f(aX - extraBoundary , aY + height +extraBoundary )
            };
        }

        public Terrain(float x, float y, VertexArray vertexArray, List<Vector2f> points) : base(x, y, vertexArray)
        {
            logicalPoints = points;
            if (blocksBullets)
            {
                Collides.Add(typeof(Bullet));
            }
        }

        public override void Collision(Entity collided)
        {
            switch (collided)
            {
                case Bullet b:
                    if (blocksBullets)
                    {
                        lock (Program.ToChange)
                        {
                            if (!Program.ToChange.Contains(b))
                                Program.ToChange.Add(b);
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public static Terrain GenerateLake(int x, int y)
        {
            VertexArray vertexArray = new VertexArray(PrimitiveType.TrianglesStrip, 6);
            vertexArray[0] = new Vertex(new Vector2f(x, y), Color.Blue);
            vertexArray[1] = new Vertex(new Vector2f(x - 60, y + 100), Color.Blue);
            vertexArray[2] = new Vertex(new Vector2f(x + 90, y + 10), Color.Blue);
            vertexArray[3] = new Vertex(new Vector2f(x + 60, y + 180), Color.Blue);
            vertexArray[4] = new Vertex(new Vector2f(x + 180, y + 100), Color.Blue);
            vertexArray[5] = new Vertex(new Vector2f(x + 100, y + 170), Color.Blue);
            List<Vector2f> points = new List<Vector2f>
            {
                new Vector2f(x - extraBoundary, y - extraBoundary),
                new Vector2f(x - 60 - extraBoundary * 4, y + 100),
                new Vector2f(x + 60, y + 180 + extraBoundary),
                new Vector2f(x + 100 +extraBoundary, y + 170 + extraBoundary),
                new Vector2f(x + 180 + extraBoundary, y + 100 + extraBoundary),
                new Vector2f(x + 90 + extraBoundary, y + 10 - extraBoundary)
            };
            return new Terrain(x, y, vertexArray, points) { renderLayer = 5, blocksBullets = false }; //Before enemies and projectiles
        }

        public static Terrain GenerateRock(int x, int y)
        {
            VertexArray vertexArray = new VertexArray(PrimitiveType.TrianglesFan, 8);
            vertexArray[0] = new Vertex(new Vector2f(x, y), new Color(150, 150, 150));
            vertexArray[1] = new Vertex(new Vector2f(x + 50, y), new Color(100, 100, 100));
            vertexArray[2] = new Vertex(new Vector2f(x + 10, y + 40), new Color(100, 100, 100));
            vertexArray[3] = new Vertex(new Vector2f(x - 40, y + 10), new Color(100, 100, 100));
            vertexArray[4] = new Vertex(new Vector2f(x - 20, y - 40), new Color(100, 100, 100));
            vertexArray[5] = new Vertex(new Vector2f(x + 35, y - 20), new Color(100, 100, 100));
            vertexArray[6] = new Vertex(new Vector2f(x + 40, y - 15), new Color(100, 100, 100));
            vertexArray[7] = new Vertex(new Vector2f(x + 50, y), new Color(100, 100, 100));
            List<Vector2f> points = new List<Vector2f>
            {
                new Vector2f(x + 50 + extraBoundary, y - extraBoundary),
                new Vector2f(x + 10 + extraBoundary, y + 40+ extraBoundary),
                new Vector2f(x - 40 - extraBoundary, y + 10 + extraBoundary),
                new Vector2f(x - 20 - extraBoundary, y - 40 - extraBoundary),
                new Vector2f(x + 35 + extraBoundary, y - 20 -extraBoundary),
                new Vector2f(x + 40 + extraBoundary, y - 15 - extraBoundary),
                new Vector2f(x + 50 + extraBoundary, y - extraBoundary)
            };
            return new Terrain(x, y, vertexArray, points);
        }
    }
}