using TowerDefenseGame.Towers;
using System.Collections.Generic;

namespace TowerDefenseGame.Maps
{
    internal abstract class Map
    {
        public string name;

        public Map(string name)
        {
            this.name = name;
        }

        public abstract List<Terrain> GenerateTerrain();
    }

    internal class EmptyMap : Map
    {
        public EmptyMap() : base("Empty Map")
        {
        }

        public override List<Terrain> GenerateTerrain()
        {
            return new List<Terrain>();
        }
    }

    internal class ForestMap : Map
    {
        public ForestMap() : base("Forest Map")
        {
        }

        public override List<Terrain> GenerateTerrain()
        {
            return new List<Terrain>
            {
                Terrain.GenerateLake(600, 300),
                Terrain.GenerateLake(300, 800),
                Terrain.GenerateRock(140, 80),
                Terrain.GenerateRock(900, 490),
                Terrain.GenerateRock(800, 300),
                Terrain.GenerateRock(200, 500),
                Terrain.GenerateRock(620, 120),
                Terrain.GenerateLake(1000, 200),
                Terrain.GenerateRock(1340, 700),
                Terrain.GenerateRock(1500, 200),
                Terrain.GenerateRock(1240, 650),
                Terrain.GenerateRock(1100, 580),
                Terrain.GenerateRock(1620, 700),
            };
        }
    }
}