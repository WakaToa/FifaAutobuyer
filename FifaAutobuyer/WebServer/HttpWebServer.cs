using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Conventions;
using Nancy.Hosting.Self;
using Nancy.Responses;
using Nancy.TinyIoc;
using Nancy.ViewEngines;

namespace FifaAutobuyer.WebServer
{
    public class HttpWebServer
    {
        private static NancyHost _host;

        public static string WebPath = "Views\\";

        public static void Start(int port)
        {
            var configuration = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true },
            };

            _host = new NancyHost(configuration, new Uri($"http://localhost:{port}/"));
            _host.Start();
        }

        public static void Stop()
        {
            _host.Stop();
        }
    }

    public class PathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), HttpWebServer.WebPath));
            return path;
        }
    }

    public static class StaticResourceConventionBuilder
    {
        public static Func<NancyContext, string, Response> AddDirectory(string requestedPath, Assembly assembly, string namespacePrefix)
        {
            return (context, s) =>
            {
                var path = context.Request.Path;

                if (!path.StartsWith(requestedPath))
                {
                    return null;
                }

                string resourcePath;
                string name;

                var adjustedPath = path.Substring(requestedPath.Length + 1);

                if (adjustedPath.IndexOf('/') >= 0)
                {
                    name = Path.GetFileName(adjustedPath);
                    resourcePath = namespacePrefix + "." + adjustedPath
                                       .Substring(0, adjustedPath.Length - name.Length - 1)
                                       .Replace('/', '.');
                }
                else
                {
                    name = adjustedPath;
                    resourcePath = namespacePrefix;
                }

                return new EmbeddedFileResponse(assembly, resourcePath, name);
            };
        }
    }

    public class ApplicationBootstrapper : DefaultNancyBootstrapper
    {
        protected override IRootPathProvider RootPathProvider => new PathProvider();

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            nancyConventions.StaticContentsConventions.Clear();
            nancyConventions.StaticContentsConventions.Add(StaticResourceConventionBuilder.AddDirectory("/bootstrap", typeof(Program).Assembly, "FifaAutobuyer.WebServer.Views.bootstrap"));
            nancyConventions.StaticContentsConventions.Add(StaticResourceConventionBuilder.AddDirectory("/dist", typeof(Program).Assembly, "FifaAutobuyer.WebServer.Views.dist"));
            nancyConventions.StaticContentsConventions.Add(StaticResourceConventionBuilder.AddDirectory("/plugins", typeof(Program).Assembly, "FifaAutobuyer.WebServer.Views.plugins"));

            base.ConfigureConventions(nancyConventions);
        }

        public override void Configure(INancyEnvironment environment)
        {
            environment.Tracing(false, true);
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(
                container.Resolve<IUserValidator>(),
                "Fifa"));
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            //ensure you put the correct assembly in here!
            ResourceViewLocationProvider.RootNamespaces.Add(typeof(Program).Assembly, "FifaAutobuyer.WebServer.Views");
        }

        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(
                    cfg =>
                    {
                        cfg.ViewLocationProvider = typeof(ResourceViewLocationProvider);
                    });
            }
        }

    }
}
