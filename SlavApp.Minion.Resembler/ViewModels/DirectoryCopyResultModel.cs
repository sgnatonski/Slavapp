using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.Resembler.ViewModels
{
    public class DirectoryCopyResultModel
    {
        public string Directory1 { get; set; }
        public string Directory2 { get; set; }
        public int Directory1Files { get; set; }
        public int Directory2Files { get; set; }
        public int CopyCount { get; set; }

        public double CopyPercentage
        {
            get
            {
                return (double)CopyCount / (double)Math.Max(Directory1Files, Directory2Files);
            }
        }

        public bool ExactCopy { get { return CopyPercentage == 1; } }
        public bool Dir1ContainsDir2 { get { return CopyCount == Directory2Files && Directory1Files > Directory2Files; } }
        public bool Dir2ContainsDir1 { get { return CopyCount == Directory1Files && Directory2Files > Directory1Files; } }
        public bool SubsetCopy { get { return CopyPercentage < 1 && Directory1Files > CopyCount && Directory2Files > CopyCount; } }
    }
}
