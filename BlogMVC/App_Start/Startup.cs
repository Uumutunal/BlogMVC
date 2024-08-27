using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlogMVC.Controllers;
using System.Security.Claims;

namespace BlogMVC
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
            })
            .AddTwitter(options =>
            {
                options.ConsumerKey = "JtI4AoLrHOnst8taYDLnptI0V";     // Replace with your Client ID
                options.ConsumerSecret = "tXfq3wH5VqiQ8HG09qk6I66kDAZNiAvrEK33c2757daxljGeLV"; // Replace with your Client Secret
                options.CallbackPath = "/signin-twitter"; // Ensure this matches the Callback URL registered with Twitter
                options.RetrieveUserDetails = true;
                options.Events.OnCreatingTicket = context =>
                {
                    // This is where you can access the user information from Twitter
                    var identity = context.Principal.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        // Access additional claims or user information here
                    }
                    return Task.CompletedTask;
                };
            });

            services.AddControllersWithViews();
            services.AddHttpClient();

            // Add session services
            services.AddDistributedMemoryCache();
			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});
		}

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
			app.UseSession();
			app.UseAuthentication(); // Ensure this is called to set up authentication middleware
            app.UseAuthorization(); // Ensure this is called to set up authorization middleware

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
