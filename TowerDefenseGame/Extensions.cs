using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrySFML2
{
    static class Extensions
    {
        public static bool NextBool(this Random random)
        {
            return random.Next(0, 2) == 1;
        }
    }
}
