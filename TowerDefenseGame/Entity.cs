using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace TrySFML2
{
    class Entity
    {
        public IEnumerable<Vector2f> Points
        {
            get {
                void RotateVector2d(ref float _x, ref float _y, double degrees)
                {
                    float tx = _x;
                    _x = _x * (float)Math.Cos(degrees) - _y * (float)Math.Sin(degrees);
                    _y = tx * (float)Math.Sin(degrees) + _y * (float)Math.Cos(degrees);
                }
                List<Vector2f> points = new List<Vector2f>();

                if(shape is RectangleShape rectangleShape)
                {
                    points.Add(new Vector2f(position.X, position.Y));
                    double angle = shape.Rotation/ 180f  * (float)Math.PI;
                    float x = 0;
                    float y = rectangleShape.Size.Y;
                    RotateVector2d(ref x, ref y, angle);
                    points.Add(new Vector2f(position.X + x, position.Y + y));
                    x = rectangleShape.Size.X;
                    y = rectangleShape.Size.Y;
                    RotateVector2d(ref x, ref y, angle);
                    points.Add(new Vector2f(position.X + x, position.Y + y));
                    x = rectangleShape.Size.X;
                    y = 0;
                    RotateVector2d(ref x, ref y, angle);
                    points.Add(new Vector2f(position.X + x, position.Y + y));
                } else if(shape is CircleShape circleShape)
                {
                    // We approximate the circle with 36 points, the position is the top left corner, so we have to center it
                    for (int i = 0; i < 36; i++)
                    {
                        points.Add(new Vector2f(position.X + circleShape.Radius / 2 + circleShape.Radius * (float)Math.Cos(i / 18f * Math.PI),
                            position.Y + circleShape.Radius / 2 + circleShape.Radius * (float)Math.Sin((float)i / 18f * Math.PI)));
                    }
                }
                return points.AsEnumerable();

            }
        }
        internal Vector2f position;
        internal Vector2f velocity;
        internal Shape shape;
        public List<Type> Collides = new List<Type>();
        public int clickLayer = 100;
        public int renderLayer = 100;
        public bool blocking = false;

        public Entity(float aX, float aY, Shape aShape)
        {
            position = new Vector2f(aX, aY);
            shape = aShape;
            if(shape.Position.X == 0)
            {
                shape.Position = position;
            }
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

        virtual public Shape Update(double timeDiff)
        {
            position.X += velocity.X * (float)timeDiff / 1000f;
            position.Y += velocity.Y * (float)timeDiff / 1000f;
            shape.Position = position;
            return shape;
        }

        virtual public void Collision(Entity collided)
        {

        }

        virtual public void OnClick(int x, int y, Mouse.Button button)
        {
        }

        virtual public Entity Create(int x,int y)
        {
            throw new NotImplementedException();
        }
    }
}
