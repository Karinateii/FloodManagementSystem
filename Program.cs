using NewLagosFloodDetectionSystem.Data;
using NewLagosFloodDetectionSystem.Models;
using Microsoft.AspNetCore.Identity;
using NewLagosFloodDetectionSystem.AccountRepository.Implementation;
using NewLagosFloodDetectionSystem.AccountRepository.Abstract;
using Microsoft.ML;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FloodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<FloodDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(op => op.LoginPath = "/UserAuthentication/Login");

builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

builder.Services.AddScoped<MLContext>();
builder.Services.AddAuthorization();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
