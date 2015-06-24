using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Notifications
{
    public class AssemblyProvider
    {
        public IEnumerable<Type> GetTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GlobalAssemblyCache).SelectMany(x => x.GetTypes());
        }

        public IEnumerable<Type> GetTypes(string assemblyCompanyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GlobalAssemblyCache && a.GetCustomAttributes<AssemblyCompanyAttribute>().Any(c => c.Company == assemblyCompanyName)).SelectMany(x => x.GetTypes());
        }
    }
}
