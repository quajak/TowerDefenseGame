using SFML.Graphics;
using SFML.System;
using System;

namespace TrySFML2
{
    internal class Explosion : Entity
    {
        private Animation animation;
        public bool Active = true;
        public readonly int Damage;

        public Explosion(float x, float y, float size, int damage) : base(x, y, new RectangleShape(new Vector2f(size, size)))
        {
            animation = new Animation(Animation.preLoadedTextures["Explosion"], 9, 9, 10)
            {
                Playing = true
            };
            shape.Texture = animation.Texture;
            Damage = damage;
        }

        public override Shape Update(double timeDiff)
        {
            Active = false;
            shape.TextureRect = animation.Play((float)timeDiff);
            if (animation.Finished)
                Program.toChange.Add(this);
            return base.Update(timeDiff);
        }
    }

    internal class Bullet : Entity
    {
        private readonly float maxDistance;
        public readonly float damage;
        public float pierce;
        private readonly Action<Vector2f> onFinish;
        private float distance = 0;

        public Bullet(float aX, float aY, float vX, float vY, float maxDistance, float damage, float pierce, float bulletSize, Vector2f bulletTemplate, Action<Vector2f> OnFinish = null) : base(aX, aY, vX, vY, new RectangleShape(new Vector2f(bulletTemplate.X * bulletSize, bulletTemplate.Y * bulletSize)))
        {
            shape.Origin = new Vector2f(bulletTemplate.X / 2 * bulletSize, bulletTemplate.Y / 2 * bulletSize);
            shape.Rotation = (float)(Math.Atan(vY / vX) / (2 * Math.PI) * 360);
            this.maxDistance = maxDistance;
            this.damage = damage;
            this.pierce = pierce;
            onFinish = OnFinish;
        }

        public override Shape Update(double timeDiff)
        {
            distance += (float)Math.Sqrt(Math.Pow(velocity.X * timeDiff / 1000f, 2) + Math.Pow(velocity.Y * timeDiff / 1000f, 2));
            if ((position.X < 0 || position.X > Program.gameSize.X) || (position.Y < 0 || position.Y > Program.gameSize.Y) || distance > maxDistance)
            {
                lock (Program.toChange)
                {
                    Program.toChange.Add(this);
                    if (onFinish != null)
                        onFinish.Invoke(position);
                }
            }
            return base.Update(timeDiff);
        }

        public void End()
        {
            if (onFinish != null)
            {
                lock (Program.toChange)
                {
                    onFinish.Invoke(position);
                }
            }
        }
    }
}