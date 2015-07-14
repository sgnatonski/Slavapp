using System;
using System.IO;
using System.Reflection;

namespace SlavApp.ImageFinder
{
    public static class Utils
    {
        public static string GetAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            return path;
        }
    }
}