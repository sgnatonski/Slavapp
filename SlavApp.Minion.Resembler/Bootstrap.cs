using SlavApp.Resembler.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.Resembler
{
    public static class Bootstrap
    {
        public static List<Type> RegisterSingleton()
        {
            return new[] { typeof(DBreezeInstance) }.ToList();
        }
    }
}
