using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Spotify.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de JWT
var jwtConfigSection = builder.Configuration.GetSection("JwtConfig");
var jwtConfig = jwtConfigSection.Get<JwtConfig>();
builder.Services.Configure<JwtConfig>(jwtConfigSection);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Spotify",
            ValidAudience = "Spotify",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
        };
    });

// Otros servicios
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SpotifyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

var app = builder.Build();

// Configuraci�n de middleware y enrutamiento
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
