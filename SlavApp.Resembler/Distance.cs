using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Resembler
{
    public class Distance
    {
        public string Filename1 { get; set; }
        public string Filename2 { get; set; }
        public ulong Hash1 { get; set; }
        public ulong Hash2 { get; set; }
        public int DistanceBetween { get; set; }
    }
}
