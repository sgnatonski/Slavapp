using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.SimpleInjector
{
    public class SimpleInjectorContainerWrapper : IContainerWrapper
    {
        private readonly Container _container;

        public SimpleInjectorContainerWrapper(Container container)
        {
            _container = container;
        }

        public T Resolve<T>() where T : class
        {
            return _container.GetInstance<T>();
        }

        public void RegisterSingle<T>() where T : class
        {
            _container.RegisterSingleton<T>();
        }

        public void Register<T, TImpl>()
            where T : class
            where TImpl : class, T
        {
            _container.Register<T, TImpl>();
        }
        public void RegisterSingle<T, TImpl>()
            where T : class
            where TImpl : class, T
        {
            _container.RegisterSingleton<T, TImpl>();
        }

        public void Register<T>() where T : class
        {
            _container.Register<T>();
        }

        public void Register<T>(T instance) where T : class
        {
            _container.RegisterSingleton(instance);
        }

        public void Register<T>(Func<T> factory)
            where T : class
        {
            _container.Register(factory);
        }
    }
}
