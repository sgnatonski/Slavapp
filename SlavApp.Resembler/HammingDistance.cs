using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Resembler
{
    public static class HammingDistance
    {
        const ulong m1 = 0x5555555555555555UL;
        const ulong m2 = 0x3333333333333333UL;
        const ulong h01 = 0x0101010101010101UL;
        const ulong m4 = 0x0f0f0f0f0f0f0f0fUL;

        public static int GetDistance(ulong hash1, ulong hash2)
        {
            ulong x = hash1 ^ hash2;
            x -= (x >> 1) & m1;
            x = (x & m2) + ((x >> 2) & m2);
            x = (x + (x >> 4)) & m4;
            return (int)((x * h01) >> 56);
        }
    }
}
