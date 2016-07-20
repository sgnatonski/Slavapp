using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Windows
{
    public class AssemblyPathProvider : IPathProvider
    {
        private readonly string basePath;
        public AssemblyPathProvider()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            basePath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }

        public string BasePath
        {
            get { return basePath; }            
        }
    }
}
