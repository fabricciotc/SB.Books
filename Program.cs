using Supabase;
using SupabaseNET.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace SupabaseNET;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var supabaseUrl = builder.Configuration["Supabase:Url"]
            ?? Environment.GetEnvironmentVariable("SUPABASE_URL");
        var supabaseKey = builder.Configuration["Supabase:Key"]
            ?? Environment.GetEnvironmentVariable("SUPABASE_KEY");

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
        {
            throw new InvalidOperationException(
                "Las credenciales de Supabase no están configuradas. " +
                "Configura SUPABASE_URL y SUPABASE_KEY como variables de entorno o en appsettings.json");
        }

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped(provider =>
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };
            return new Supabase.Client(supabaseUrl, supabaseKey, options);
        });

        builder.Services.AddScoped<ISupabaseService, SupabaseService>();

        builder.Services.AddControllersWithViews();

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(8);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/Login";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        var useHttpsRedirection = Environment.GetEnvironmentVariable("USE_HTTPS_REDIRECTION") != "false";
        if (useHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        var port = Environment.GetEnvironmentVariable("PORT");
        if (!string.IsNullOrEmpty(port))
        {
            app.Urls.Clear();
            app.Urls.Add($"http://+:{port}");
        }

        app.Run();
    }
}
