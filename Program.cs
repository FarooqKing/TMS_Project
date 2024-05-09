using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TMS.Models;
using TMS_Project.Models;

var builder = WebApplication.CreateBuilder(args);

// Inside the ConfigureServices method
builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<SoftDeleteInterceptor>();

builder.Services.AddDbContext<TmsContext>((sp, options) =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Tms-project")).AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/login/login";
        option.SlidingExpiration = true;
        option.ExpireTimeSpan = TimeSpan.FromHours(1);
    });
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Login}/{id?}");

app.Run();
