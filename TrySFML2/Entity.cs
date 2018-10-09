﻿using System;
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
                RectangleShape rectangleShape = (shape as RectangleShape);
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
                return points.AsEnumerable();

            }
        }
        internal Vector2f position;
        internal Vector2f velocity;
        internal Shape shape;
        public List<Type> Collides = new List<Type>();
        public int clickLayer = 100;
        public int renderLayer = 100;

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

    class ExpandingCircle : Entity
    {
        private readonly int endSize;

        public ExpandingCircle(int aX, int aY, int intialSize, int endSize) : base(aX, aY, new CircleShape(intialSize))
        {
            this.endSize = endSize;
        }

        public override Shape Update(double timeDiff)
        {
            CircleShape circle = (shape as CircleShape);
            circle.Radius += 1;
            position.X -= 1; 
            position.Y -= 1;
            if (circle.Radius > endSize)
                Program.toChange.Add(this);
            return base.Update(timeDiff);
        }
    }
}
