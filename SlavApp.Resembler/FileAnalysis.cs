using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Resembler
{
    public class FileAnalysis
    {
        public string Name { get; set; }
        public DateTime LastModified { get; set; }
        public string StoredHash { get; set; }
        public string CurrentHash { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Data { get; set; }
    }
}
