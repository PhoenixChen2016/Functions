using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace Functions
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var targetAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(@"C:\Users\Phoenix\documents\visual studio 2017\Projects\Functions\ClassLibrary1\bin\Debug\netcoreapp1.1\ClassLibrary1.dll");

            var inst = targetAssembly.CreateInstance("ClassLibrary1.Class1");
            var targetType = inst.GetType();
            var mi = targetType.GetMethod("Execute");

            var parameters = mi.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();

            var call = Expression.Call(Expression.Constant(inst), mi, parameters);

            var lambda = Expression.Lambda(call, parameters);
            var func = lambda.Compile();

            app.UseMvc(route =>
            {
                route.Routes.Add(new Route(
                    new RouteHandler(async context =>
                    {
                        var arguments = new List<object>();
                        foreach (var pi in parameters)
                        {
                            arguments.Add(context.GetRouteValue(pi.Name));
                        }
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(func.DynamicInvoke(arguments.ToArray())));
                    }),
                    "api/Hello/{arg}",
                    route.ServiceProvider.GetRequiredService<IInlineConstraintResolver>()));
            });
        }
    }
}
