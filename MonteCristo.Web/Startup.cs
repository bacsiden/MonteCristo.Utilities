using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MonteCristo.Application.Models.Constants;
using MonteCristo.Application.Models.Framework;
using MonteCristo.Application.Repositories;
using MonteCristo.Application.Services;
using MonteCristo.Database.Repositories;
using MonteCristo.FileService;
using MonteCristo.MongoDB.Framework.Models;
using MonteCristo.Web.Services;

namespace MonteCristo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole>(identityOptions =>
            {
                identityOptions.Password.RequiredLength = 1;
                identityOptions.Password.RequireLowercase = false;
                identityOptions.Password.RequireUppercase = false;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequireDigit = false;
            }, mongoIdentityOptions =>
            {
                mongoIdentityOptions.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
            });

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration.GetSection("AppSettings:AuthenticationFacebookAppId").Value;
                facebookOptions.AppSecret = Configuration.GetSection("AppSettings:AuthenticationFacebookAppSecret").Value;
            }).AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration.GetSection("AppSettings:AuthenticationGoogleClientId").Value;
                googleOptions.ClientSecret = Configuration.GetSection("AppSettings:AuthenticationGoogleClientSecret").Value;
            });

            ConfigureIoC(services);
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void ConfigureIoC(IServiceCollection services)
        {
            string mongoUrl = Configuration.GetConnectionString("DefaultConnection");
            MongoUrl url = new MongoUrl(mongoUrl);
            IMongoDatabase database = new MongoClient(url).GetDatabase(url.DatabaseName);
            services.AddSingleton(database);

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IFileService, AmazoneS3>();

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserService, UserService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            InitRoles(serviceProvider).Wait();
        }

        private async Task InitRoles(IServiceProvider serviceProvider)
        {
            //adding custom roles
            RoleManager<ApplicationRole> RoleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (string roleName in Roles.Values)
            {
                //creating the roles and seeding them to the database
                bool roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await RoleManager.CreateAsync(new ApplicationRole(roleName));
                }
            }
        }
    }
}
