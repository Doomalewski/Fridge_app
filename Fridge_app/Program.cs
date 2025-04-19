using Fridge_app.Data;
using Fridge_app.Models;
using Fridge_app.Repositories;
using Fridge_app.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FridgeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
});
// Rejestracja serwisu
builder.Services.AddScoped<ProductService>();
// Rejestracja generycznego repozytorium
builder.Services.AddScoped<IRepository<Product>, EfCoreRepository<Product>>();
builder.Services.AddScoped<IRepository<StoredProduct>, EfCoreRepository<StoredProduct>>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<StoredProductService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<>));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IMealService, MealService>();
builder.Services.AddScoped<IRepository<Meal>, EfCoreRepository<Meal>>();
builder.Services.AddScoped<IRepository<Recipe>, EfCoreRepository<Recipe>>();
builder.Services.AddScoped<IRepository<ProductWithAmount>, EfCoreRepository<ProductWithAmount>>();
builder.Services.AddScoped<GeminiService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
        });
var app = builder.Build();


var supportedCultures = new[] { new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
