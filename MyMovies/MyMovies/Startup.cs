using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMovies.Common.Options;
using MyMovies.Common.Services;
using MyMovies.Custom;
using MyMovies.Repositories;
using MyMovies.Repositories.Interfaces;
using MyMovies.Services;
using MyMovies.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMovies
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
            //configure in startup
            //see configuration below
            services.AddDbContext<MyMoviesDbContext>(
                x => x.UseSqlServer(Configuration.GetConnectionString("MyMovies"))
                );

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
                options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(int.Parse(Configuration["CookieExpirationPeriod"]));
                    options.LoginPath = "/Auth/SignIn";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                }
                );

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsAdmin", policy =>
                {
                    policy.RequireClaim("IsAdmin", "True");
                });
            });

            //configure options
            services.Configure<SidebarConfig>(Configuration.GetSection("SidebarConfig"));

            //register services
            object p = services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddTransient<IMoviesService, MoviesService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IUsersService,UsersService>();
            services.AddTransient<ICommentsService, CommentsService>();
            services.AddTransient<ISidebarService, SidebarService>();
            services.AddTransient<ILogService, LogServices>();
            services.AddTransient<IMovieTypeService, MovieTypeService>();
            services.AddTransient<IMovieLikeService, MovieLikeService>();

            //register repositories
            services.AddTransient<IMoviesRepository, MoviesRepository>();
            services.AddTransient<IUserRepository, UsersRepository>();
            services.AddTransient<ICommentsRepository, CommentsRepository>();
            services.AddTransient<IMovieTypeRepository, MovieTypeRepository>();
            services.AddTransient<IMovieLikeRepository, MovieLikeRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Info/InternalError");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            
            app.UseMiddleware<ExceptionLoggingMiddleware>();

            app.UseMiddleware<RequestResponseLogMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Movies}/{action=Overview}/{id?}");
            });
        }
    }
}
