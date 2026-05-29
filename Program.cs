using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using IPOWeb;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // Set max request body size (500 MB)
    options.Limits.MaxRequestBodySize = 524288000;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000;
});
// ---------------- MVC ----------------

// 1. Add services to the container
builder.Services.AddDistributedMemoryCache(); // Required for Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;                // Security: prevent JS access
    options.Cookie.IsEssential = true;             // Required for GDPR/compliance
    options.Cookie.Name = ".STA.Session";

});
builder.Services.AddControllersWithViews()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<UserManagementModelValidator>();
    });

builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // localhost
        options.AccessDeniedPath = "/Login/Login";
        //options.AccessDeniedPath = "/Login/Denied";
        // options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.ExpireTimeSpan = TimeSpan.FromHours(5);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
//builder.Services.AddScoped<UserProvider>();

var app = builder.Build();

// ---------------- PIPELINE ----------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<ApiTokenRefreshMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
