using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AccountAtAGlance.Repository;
using AccountAtAGlance.Repository.Seeding;
using AccountAtAGlance.Repository.Helpers;
using AccountAtAGlance.Repository.Interfaces;

namespace AccountAtAGlance
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Entity Framework services to the services container.
            services.AddDbContext<AccountAtAGlanceContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("AccountAtAGlance"));
                });

            // Add MVC services to the services container.
            services.AddMvc();

            //Add data repository mappings
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<IMarketsAndNewsRepository, MarketsAndNewsRepository>();
            services.AddTransient<ISecurityRepository, SecurityRepository>();
            services.AddTransient<IStockEngine, StockEngine>();
            services.AddTransient<DatabaseInitializer>();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            DatabaseInitializer dbInitializer)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Configure the HTTP request pipeline.

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvcWithDefaultRoute();

            // Seed the database here
            dbInitializer.SeedAsync().Wait();
        }
    }
}
