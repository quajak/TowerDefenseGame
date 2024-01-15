using System;

namespace TowerDefenseGame
{
    internal static class Extensions
    {
        public static bool NextBool(this Random random)
        {
            return random.Next(0, 2) == 1;
        }
    }
}