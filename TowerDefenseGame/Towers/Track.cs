using SFML.Graphics;
using System;
using System.Collections.Generic;
using TowerDefenseGame;

namespace TowerDefenseGame.Towers
{
    internal class Track : Tower
    {
        private Track connectedTo = null;
        private static Track last = null;
        public static int _cost = 15;
        private TrackBar trackBar = null;
        public Track next;
        public int currentNum = 0;

        internal Track ConnectedTo
        {
            get => connectedTo; set
            {
                connectedTo = value;
                const int offset = 10;
                trackBar = new TrackBar(position.X + offset, position.Y + offset, connectedTo.position.X + offset
                    , connectedTo.position.Y + 5, this);
                Program.ToChange.Add(trackBar);
            }
        }

        public static bool Available { get => Program.Money >= _cost && !MainBase.Available; }

        public Track(float ax, float ay, bool buy) : base(ax, ay, new CircleShape(10),
            _cost, "Track", "A start or end point of a track, which enemies will follow", 0, 10)
        {
            //shape.Origin = new SFML.System.Vector2f(10, 10);

            var amountI = new Upgrade(new Modifier(ModifierType.Absolute, 10), 5, "Stronger beam I", "Doubles the number of enemies it can hold", UpdateType.Amount)
            {
                Unlocks = new List<Upgrade>
                {
                    new Upgrade(new Modifier(ModifierType.Absolute, 15), 15, "Stronger beam II", "Increases trackable enemies by 15", UpdateType.Amount)
                    {
                        Unlocks = new List<Upgrade>
                        {
                            new Upgrade(new Modifier(ModifierType.Percentage, 2), 30, "Stronger beam III", "Doubles the number of controlled enemies", UpdateType.Amount)
                        }
                    }
                }
            };
            AvailableUpgrades.Add(amountI);
            shape.FillColor = Color.Red;
            if (buy)
            {
                Program.Money -= _cost;
                if (last != null)
                {
                    ConnectedTo = last;
                    last.next = this;
                }
                last = this;
            }
        }

        public override Entity Create(int x, int y)
        {
            return new Track(x, y, true);
        }
    }

    internal class TrackBar : Tower
    {
        public Track end;

        public TrackBar(float ax, float ay, float bx, float by, Track end) : base(ax, ay,
            new RectangleShape(new SFML.System.Vector2f(10, (int)Math.Sqrt(Math.Pow(ax - bx, 2) + Math.Pow(ay - by, 2)))),
            0, "TrackBar", "TrackBar", 0, 0, 0)
        {
            canSelect = false;
            this.end = end;
            shape.FillColor = new Color(shape.FillColor) { A = 100 };
            shape.Rotation = (float)Math.Atan2(by - ay, bx - ax) / (2f * (float)Math.PI) * 360f - 90f;
            renderLayer = 8; // So it is drawn before the enemy
            Collides.Add(typeof(Terrain));
        }

        public override void Collision(Entity collided)
        {
            switch (collided)
            {
                case Terrain _:
                    if (!Program.ToChange.Contains(this))
                    {
                        Program.ToChange.Add(this);
                    }
                    return;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}