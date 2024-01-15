using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefenseGame
{
    internal class Animation
    {
        public static Dictionary<string, Texture> preLoadedTextures = new Dictionary<string, Texture>
        {
            {"Explosion", new Texture("./Resources/explosion.png") } //http://animalia-life.club/other/animated-explosion-sprite.html
        };

        public Texture Texture;
        private readonly List<IntRect> positions = new List<IntRect>();
        public bool Playing;
        private readonly int frameNumber;
        private readonly float speed;
        private float timePassed;
        public bool Finished = false;

        public Animation(string file, int numX, int numY, float speed) : this(new Texture(file), numX, numY, speed)
        {
        }

        public Animation(Texture texture, int numX, int numY, float speed)
        {
            Texture = texture;
            float tWidth = Texture.Size.X;
            float tHeight = Texture.Size.Y;
            float fWidth = tWidth / numX;
            float fHeight = tHeight / numY;
            for (int y = 0; y < numY; y++)
            {
                for (int x = 0; x < numX; x++)
                {
                    positions.Add(new IntRect(new Vector2i((int)(x * fWidth), (int)(y * fHeight)), new Vector2i((int)fWidth, (int)fHeight)));
                }
            }
            frameNumber = positions.Count;
            this.speed = speed;
        }

        public IntRect Play(float time, bool allowRepeat = false)
        {
            if (Playing)
            {
                timePassed += time;
                if (timePassed >= frameNumber * speed)
                {
                    timePassed = 0;
                    Finished = true;
                }
            }
            if (Finished && !allowRepeat)
                return positions.Last();
            return positions[(int)(timePassed / speed)];
        }
    }

    internal class IceCircle : Entity
    {
        private readonly float radius;

        public IceCircle(float x, float y, float radius) : base(x, y, new CircleShape(0))
        {
            renderLayer = 10;
            shape.Position = new Vector2f(x, y);
            shape.FillColor = new Color(0, 0, 0, 0);
            shape.OutlineColor = new Color(80, 224, 240);
            shape.OutlineThickness = 2;
            this.radius = radius;
        }

        public override Drawable Update(double timeDiff)
        {
            float v = (float)timeDiff / 200f * radius;
            (shape as CircleShape).Radius += v;
            position = new Vector2f(position.X - v, position.Y - v);
            shape.Position = position;
            if ((shape as CircleShape).Radius > radius)
            {
                lock (Program.ToChange)
                {
                    Program.ToChange.Add(this);
                }
            }

            return shape;
        }
    }
}