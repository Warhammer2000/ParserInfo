using Microsoft.EntityFrameworkCore;
using Parser.Db;
using Parser.Interfaces;
using Parser.Models;
using Parser.Repository;
using Parser.Services;

namespace Parser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ParserSettings>(builder.Configuration.GetSection("ParserSettings"));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
            builder.Services.AddScoped<PurchaseParsingService>();
            builder.Services.AddScoped<PurchaseManagementService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");


            app.Run();
        }
    }
}
