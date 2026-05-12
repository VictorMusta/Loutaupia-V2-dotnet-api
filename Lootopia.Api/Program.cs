using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Lootopia.Api.Features.Admin;
using Lootopia.Api.Features.Auth;
using Lootopia.Api.Features.Auctions;
using Lootopia.Api.Features.Campaigns;
using Lootopia.Api.Features.Commissions;
using Lootopia.Api.Features.Inventory;
using Lootopia.Api.Features.Partners;
using Lootopia.Api.Features.Hunts;
using Lootopia.Api.Features.Leaderboards;
using Lootopia.Api.Features.Achievements;
using Lootopia.Api.Features.Marketplace;
using Lootopia.Api.Features.Trading;
using Lootopia.Api.Features.Wallet;
using Lootopia.Api.Infrastructure.Middleware;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Behaviors;
using Lootopia.Api.SharedKernel.Geo;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
builder.Services.AddDbContext<LootopiaDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()));

// --- MediatR + Pipeline ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// --- Services ---
builder.Services.AddSingleton<IGeoValidator, GeoValidator>();
builder.Services.AddSingleton<GlobalExceptionHandler>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IFraudDetector, FraudDetector>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ICommissionService, CommissionService>();

// --- JWT Authentication ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("critical", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
    });
    options.AddFixedWindowLimiter("standard", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Middleware Pipeline ---
app.UseMiddleware<GlobalExceptionHandler>();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// --- Static Files (React Frontend) ---
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Database Migration & Seed ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LootopiaDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
    await DataSeeder.SeedAchievementsAsync(db);
    await DataSeeder.SeedTestDataAsync(db);
}

// --- Health Check Endpoint ---
app.MapGet("/health", () => Microsoft.AspNetCore.Http.Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithTags("System");

// --- Auth Endpoints ---
app.MapAuthEndpoints();

// --- Wallet Endpoints ---
app.MapWalletEndpoints();

// --- Hunt Endpoints ---
app.MapHuntEndpoints();

// --- Inventory Endpoints ---
app.MapInventoryEndpoints();

// --- Campaign Endpoints ---
app.MapCampaignEndpoints();

// --- Auction Endpoints ---
app.MapAuctionEndpoints();

// --- Commission Endpoints ---
app.MapCommissionEndpoints();

// --- Partner Endpoints ---
app.MapPartnerEndpoints();

// --- Admin Endpoints ---
app.MapAdminEndpoints();

// --- Marketplace Endpoints ---
app.MapMarketplaceEndpoints();

// --- Trading Endpoints ---
app.MapTradingEndpoints();

// --- SPA Fallback Routing ---
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
