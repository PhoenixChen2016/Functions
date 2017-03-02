using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpReverseProxy;
using System.Text.RegularExpressions;

namespace ReverseProxy
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseProxy(
                new List<ProxyRule> {
                    new ProxyRule {
                        Matcher = uri => uri.AbsoluteUri.Contains("/api/"),
                        Modifier = (request, principal) => {
                             var match = Regex.Match(request.RequestUri.AbsolutePath, "/api/(.+)");
                             request.RequestUri = new Uri("http://localhost:53713/api/" + match.Groups[1].Value);
                        },
                        RequiresAuthentication = true
                    }
                },
                r =>
                {

                });
        }
    }
}
