using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace JumpenoWebassembly.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            services.AddDbContext<DataContext>(context => context.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCompression(opts => {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie();
                // TODO: uncomment after registreing app in fb and google dev accounts and adding keys
                //.AddFacebook(fbOptions => {
                //    fbOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                //    fbOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                //})
                //.AddGoogle(goOptions => {
                //    goOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                //    goOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                //});

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<GameService>();
            services.AddSingleton<AnonymUsersService>();
            services.AddSingleton<MonitorService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<Hubs.AdminPanelHub>(AdminPanelHubC.Url);
                endpoints.MapHub<Hubs.GlobalChatHub>(GlobalChatHubC.Url);
                endpoints.MapHub<Hubs.GameHub>(GameHubC.Url);
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
