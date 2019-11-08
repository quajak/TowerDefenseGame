using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace TowerDefenseGame
{
    internal class Entity
    {
        public IEnumerable<Vector2f> Points
        {
            get
            {
                if (vertexArray != null)
                {
                    for (int i = 0; i < logicalPoints.Count; i++)
                    {
                        yield return logicalPoints[i];
                    }
                }
                void RotateVector2d(ref float _x, ref float _y, double degrees)
                {
                    float tx = _x;
                    _x = _x * (float)Math.Cos(degrees) - _y * (float)Math.Sin(degrees);
                    _y = tx * (float)Math.Sin(degrees) + _y * (float)Math.Cos(degrees);
                }

                if (shape is RectangleShape rectangleShape)
                {
                    yield return new Vector2f(position.X - shape.Origin.X, position.Y - shape.Origin.Y);
                    double angle = shape.Rotation / 180f * (float)Math.PI;
                    float x = 0;
                    float y = rectangleShape.Size.Y;
                    RotateVector2d(ref x, ref y, angle);
                    yield return new Vector2f(position.X - shape.Origin.X + x, position.Y - shape.Origin.Y + y);
                    x = rectangleShape.Size.X;
                    y = rectangleShape.Size.Y;
                    RotateVector2d(ref x, ref y, angle);
                    yield return new Vector2f(position.X - shape.Origin.X + x, position.Y - shape.Origin.Y + y);
                    x = rectangleShape.Size.X;
                    y = 0;
                    RotateVector2d(ref x, ref y, angle);
                    yield return new Vector2f(position.X - shape.Origin.X + x, position.Y - shape.Origin.Y + y);
                }
                else if (shape is CircleShape circleShape)
                {
                    // We approximate the circle with 36 points, the position is the top left corner, so we have to center it
                    for (int i = 0; i < 36; i++)
                    {
                        yield return new Vector2f(position.X + circleShape.Radius / 2 + circleShape.Radius * (float)Math.Cos(i / 18f * Math.PI),
                            position.Y + circleShape.Radius / 2 + circleShape.Radius * (float)Math.Sin((float)i / 18f * Math.PI));
                    }
                }
            }
        }

        internal Vector2f position;
        internal Vector2f velocity;
        internal Shape shape;
        internal VertexArray vertexArray;
        internal List<Vector2f> logicalPoints;
        public List<Type> Collides = new List<Type>();
        public int clickLayer = 100;

        /// <summary>
        /// Higher is later in the rendering order
        /// </summary>
        public int renderLayer = 100;

        public bool blocking = false;

        public Entity(float aX, float aY, Shape aShape)
        {
            position = new Vector2f(aX, aY);
            shape = aShape;
            if (shape.Position.X == 0)
            {
                shape.Position = position;
            }
        }

        public Entity(float aX, float aY, VertexArray vertex)
        {
            position = new Vector2f(aX, aY);
            vertexArray = vertex;
        }

        public Entity(int aX, int aY, Shape aShape)
        {
            position = new Vector2f(aX, aY);
            shape = aShape;
            shape.Position = position;
        }

        public Entity(int aX, int aY, int vX, int vY, Shape aShape) : this(aX, aY, aShape)
        {
            velocity = new Vector2f(vX, vY);
        }

        public Entity(float aX, float aY, float vX, float vY, Shape aShape) : this(aX, aY, aShape)
        {
            velocity = new Vector2f(vX, vY);
        }

        virtual public Drawable Update(double timeDiff)
        {
            if (shape != null)
            {
                position.X += velocity.X * (float)timeDiff / 1000f;
                position.Y += velocity.Y * (float)timeDiff / 1000f;
                shape.Position = position;
                return shape;
            }
            else
            {
                return vertexArray;
            }
        }

        virtual public void Collision(Entity collided)
        {
        }

        virtual public void OnClick(int x, int y, Mouse.Button button)
        {
        }

        virtual public Entity Create(int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}