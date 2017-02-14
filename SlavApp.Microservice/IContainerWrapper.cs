using System;

namespace SlavApp.Microservice
{
    public interface IContainerWrapper
    {
        T Resolve<T>() where T : class;
        void RegisterSingle<T>() where T : class;
        void Register<T>() where T : class;
        void Register<T>(T instance) where T : class;
        void Register<T>(Func<T> factory)
            where T : class;
        void Register<T, TImpl>()
            where T : class
            where TImpl : class, T;

        void RegisterSingle<T, TImpl>()
            where T : class
            where TImpl : class, T;
    }
}