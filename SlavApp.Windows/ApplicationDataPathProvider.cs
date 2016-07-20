using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Windows
{
    public class ApplicationDataPathProvider : IPathProvider
    {
        private readonly string basePath;

        public ApplicationDataPathProvider()
        {
            var log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Resembler");

            log.Info("ApplicationDataPathProvider");
            log.Info(basePath);
            if (!Directory.Exists(basePath))
            {
                try
                {
                    Directory.CreateDirectory(basePath);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Directory creation failed: {0}", basePath), ex);
                    throw;
                }
            }
        }

        public string BasePath
        {
            get { return basePath; }
        }
    }
}
