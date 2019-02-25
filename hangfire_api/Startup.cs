using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Owin;
using Microsoft.Owin.Security;
using System.Web;
using Hangfire.Annotations;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using hangfire_api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Owin;
using Microsoft.AspNetCore.DataProtection;
using System.IO;


namespace hangfire_api
{
    public class Startup
    {
        //private readonly gitRepo_Controller _Controller;  wrong use Hahahaha !!
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //_Controller = controller;

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connStr = Configuration.GetSection("ConnectionStrings");
            var MySqlStr = connStr.GetSection("MySql");
            services.AddDbContext<gitRepo_Context>(options => options.UseMySQL(MySqlStr.GetSection("gitRepo").Get<string>()));
            // i used sqlserver (mssql-server) in ubuntu to access hangfire !
            services.AddHangfire(config => config.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));
            //services.AddHangfire(config => config.UseStorage(new MySqlStorage(MySqlStr.GetSection("Hangfire").Get<string>())));
            services.AddHttpClient("github", c => {
                c.BaseAddress = new Uri(connStr.GetSection("Github").GetSection("url").Get<string>());
                c.DefaultRequestHeaders.Add("Accept", "Application/Json");
                c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            });

            services.AddMvc();
            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHangfireServer();
            app.UseAuthentication();

            app.UseHangfireDashboard("/api/hangfire", new DashboardOptions {
                Authorization = new [] { new MyAuthorizationFilter() }
            });
            
            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private DirectoryInfo GetKeyRingDirInfo()
        {
            var startupAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var applicationBasePath = System.AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                directoryInfo = directoryInfo.Parent;

                var keyRingDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, "KeyRing"));
                if (keyRingDirectoryInfo.Exists)
                {
                    return keyRingDirectoryInfo;
                }
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"KeyRing folder could not be located using the application root {applicationBasePath}.");
        }
    }


    // What this class does is that it uses a basic authentication real for accessing hangfire dashboard !
    // the the credential are correct you are authenticated and can access the dashboard, otherwise you're not authenticated !
    public class HFDashboardAuthFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            Console.WriteLine("It's Working !");
            var httpContext = context.GetHttpContext();

            var header = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(header))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var authValues = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(header);

            if (!"Basic".Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var parameter = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
            var parts = parameter.Split(':');

            if (parts.Length < 2)
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var username = parts[0];
            var password = parts[1];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            if (username == "Wicked" && password == "Luna")
            {
                return true;
            }

            SetChallengeResponse(httpContext);
            return false;
        }

        private void SetChallengeResponse(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard Authentication\"");
            httpContext.Response.WriteAsync("Authentication is required.");
        }

    }

    // To use this Authorization filter you have to be authenticated to access hangfire dashboard !
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity.IsAuthenticated;
        }
    }
}
