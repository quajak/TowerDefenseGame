using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrySFML2
{
    internal static class Statistics
    {
        private static Dictionary<Type, int> damageCounter = new Dictionary<Type, int>();

        public static void WriteStats()
        {
            foreach (var item in damageCounter)
            {
                Console.WriteLine($"{item.Key.Name}: {item.Value}");
            }
        }

        public static void AddDamage(int damage, Type dealer)
        {
            if (damageCounter.ContainsKey(dealer))
                damageCounter[dealer] += damage;
            else
                damageCounter.Add(dealer, damage);
        }
    }
}