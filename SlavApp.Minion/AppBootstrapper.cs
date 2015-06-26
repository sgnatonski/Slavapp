using Caliburn.Micro;
using SimpleInjector;
using SlavApp.Minion.Plugin;
using SlavApp.Minion.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SlavApp.Minion
{
    internal class AppBootstrapper : BootstrapperBase
    {
        /// <summary>
        /// The global container.
        /// </summary>
        public static readonly Container ContainerInstance = new Container();

        public AppBootstrapper()
        {
            Initialize();
        }
 
        protected override void Configure()
        {
            ContainerInstance.Register<IWindowManager, WindowManager>();
            ContainerInstance.Register<IEventAggregator, EventAggregator>(Lifestyle.Singleton);
            ContainerInstance.Register<IPluginManager, PluginManager>(Lifestyle.Singleton);
            ContainerInstance.Register<SequentialResult>(() => new SequentialResult(Enumerable.Empty<IResult>().GetEnumerator()));
            RegisterPlugins();

            ContainerInstance.Verify();
        }
 
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return ContainerInstance.GetAllInstances(service);
        }
 
        protected override object GetInstance(System.Type service, string key)
        {
            return ContainerInstance.GetInstance(service);
        }

         protected override void BuildUp(object instance)
        {
            var registration = ContainerInstance.GetRegistration(instance.GetType(), true);
            registration.Registration.InitializeInstance(instance);
        }

         protected override void OnStartup(object sender, StartupEventArgs e)
         {
             DisplayRootViewFor<MainWindowViewModel>();
         }

         protected override IEnumerable<Assembly> SelectAssemblies()
         {
             return this.LoadAssemblies(Path.Combine(Environment.CurrentDirectory, "Plugins")).Concat(new[] { Assembly.GetExecutingAssembly() });
         }

         public void RegisterPlugins()
         {
             if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Plugins")))
             {
                 Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Plugins"));
             }
             var pluginAssemblies = this.LoadAssemblies(Path.Combine(Environment.CurrentDirectory, "Plugins"));

             var pluginTypes =
                 from dll in pluginAssemblies
                 from type in dll.GetExportedTypes()
                 where typeof(IPlugin).IsAssignableFrom(type)
                 where !type.IsAbstract
                 where !type.IsGenericTypeDefinition
                 select type;

             ContainerInstance.RegisterCollection<IPlugin>(pluginTypes);

             AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromSameFolder);
         }

         static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
         {
             string folderPath = Path.Combine(Environment.CurrentDirectory, "Plugins");
             string assemblyPath = Directory.GetFiles(folderPath, new AssemblyName(args.Name).Name + ".dll", SearchOption.AllDirectories).FirstOrDefault();
             if (string.IsNullOrEmpty(assemblyPath)) return null;
             Assembly assembly = Assembly.LoadFrom(assemblyPath);
             return assembly;
         }

         private IEnumerable<Assembly> LoadAssemblies(string folder)
         {
             IEnumerable<string> dlls =
                 from file in new DirectoryInfo(folder).GetFiles("*.dll", SearchOption.AllDirectories)
                 select file.FullName;

             IList<Assembly> assemblies = new List<Assembly>();

             foreach (string dll in dlls)
             {
                 try
                 {
                     assemblies.Add(Assembly.LoadFile(dll));
                 }
                 catch { }
             }

             return assemblies;
         }
    }
}
