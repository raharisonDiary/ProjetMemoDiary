using SocialGasy.Services;
using SocialGasy.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization; // Zava-dehibe
using MongoDB.Driver;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. MONGODB
var mongoUri = builder.Configuration.GetValue<string>("MONGODB_URI") 
    ?? "mongodb+srv://diary:diary1234@cluster0.q60ysss.mongodb.net/?retryWrites=true&w=majority";
var mongoClient = new MongoClient(mongoUri);
var database = mongoClient.GetDatabase("SocialGasyDb");

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddScoped<IMongoDatabase>(sp => database);
builder.Services.AddScoped(sp => database.GetCollection<User>("Users"));

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 2. CONTROLLERS - Mampiasa AuthorizeFilter eto fa tsy FallbackPolicy
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy)); // Manery login manerana ny app
})
.AddViewLocalization()
.AddDataAnnotationsLocalization()
.AddJsonOptions(options => {
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddSingleton<QRCodeService>();

// 3. AUTHENTICATION
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

var app = builder.Build();

// 4. MIDDLEWARE
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 5. ROUTES
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();