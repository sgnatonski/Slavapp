using Caliburn.Micro;
using SimpleInjector;
using SlavApp.Minion.Resembler;
using SlavApp.Minion.ViewModels;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SlavApp.SA.Resembler
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
            ContainerInstance.Register(() => new SequentialResult(Enumerable.Empty<IResult>().GetEnumerator()));
            ContainerInstance.Register(() => log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType));
#if DEBUG
            ContainerInstance.RegisterSingleton<IPathProvider, AssemblyPathProvider>();
#else
            ContainerInstance.RegisterSingleton<IPathProvider, ApplicationDataPathProvider>();
#endif
            Bootstrap.RegisterSingleton().ForEach(x => ContainerInstance.RegisterSingleton(x, x));

            ContainerInstance.Verify();
        }
 
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            IServiceProvider provider = ContainerInstance;
            Type collectionType = typeof(IEnumerable<>).MakeGenericType(service);
            var services = (IEnumerable<object>)provider.GetService(collectionType);
            return services ?? Enumerable.Empty<object>();
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

            Application.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            Bootstrap.RegisterSingleton().ForEach(x => ((IDisposable)ContainerInstance.GetInstance(x)).Dispose());
            base.OnExit(sender, e);
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "SlavApp.Minion.Resembler.dll")), Assembly.GetExecutingAssembly() };
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ContainerInstance.GetInstance<log4net.ILog>().Fatal("Unhandled exception", e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                ContainerInstance.GetInstance<log4net.ILog>().Fatal("Unhandled exception", ex);
            }
        }
    }
}
