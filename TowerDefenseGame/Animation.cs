using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrySFML2
{
    internal class Animation
    {
        public Texture Texture;
        private List<IntRect> positions = new List<IntRect>();
        public bool Playing;
        private readonly int frameNumber;
        private readonly float speed;
        private float timePassed;
        public bool Finished = false;

        public Animation(string file, int numX, int numY, float speed)
        {
            Texture = new Texture(file);
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

        public IntRect Play(float time)
        {
            if (Playing)
            {
                timePassed += time;
                if (timePassed > frameNumber * speed)
                {
                    timePassed = 0;
                    Finished = true;
                }
            }
            return positions[(int)(timePassed / speed)];
        }
    }
}