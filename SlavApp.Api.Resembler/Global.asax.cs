using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SlavApp.Api.Resembler.Providers;
using SlavApp.Api.Resembler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SlavApp.Api.Resembler
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();

            container.Register<IVersionProvider, VersionFileProvider>();
            container.Register<ICloudTableProvider, CloudTableProvider>();
            container.Register<IResemblerUsageService, ResemblerUsageService>();
            container.Register<IUsageHashDecryptService, UsageHashDecryptService>();

            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }
    }
}
