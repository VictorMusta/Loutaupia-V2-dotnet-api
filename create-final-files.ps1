# Script final - Extensions et Repositories manquants
Write-Host "`n🚀 CRÉATION DES EXTENSIONS ET REPOSITORIES MANQUANTS`n" -ForegroundColor Green

Write-Host "1️⃣  ServiceCollectionExtensions..." -ForegroundColor Yellow

@"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Contracts.Services;
using Loutaupia_V2_dotnet_api.Infrastructure.Authentication;
using Loutaupia_V2_dotnet_api.Infrastructure.Persistence;
using Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Repositories;
using Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;
using Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

namespace Loutaupia_V2_dotnet_api.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<ICurrencyWalletRepository, CurrencyWalletRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<CreatePlayerUseCase>();
        services.AddScoped<AuthenticatePlayerUseCase>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Loutaupia V2 API",
                Version = "v1",
                Description = "API for Loutaupia V2 treasure hunt platform"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
            {
                builder.WithOrigins("http://localhost:3000", "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
"@ | Out-File -FilePath "src/Api/Extensions/ServiceCollectionExtensions.cs" -Encoding UTF8

Write-Host "✓ ServiceCollectionExtensions créé" -ForegroundColor Green

Write-Host "`n2️⃣  WebApplicationExtensions..." -ForegroundColor Yellow

@"
using Microsoft.AspNetCore.Builder;
using Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;
using Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

namespace Loutaupia_V2_dotnet_api.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapCreatePlayerEndpoint();
        app.MapAuthenticatePlayerEndpoint();
        return app;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loutaupia V2 API v1");
            });
        }
        return app;
    }
}
"@ | Out-File -FilePath "src/Api/Extensions/WebApplicationExtensions.cs" -Encoding UTF8

Write-Host "✓ WebApplicationExtensions créé" -ForegroundColor Green

Write-Host "`n3️⃣  Repositories manquants..." -ForegroundColor Yellow

# InventoryRepository
@"
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Inventory>> GetByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId, cancellationToken);

            return inventory != null
                ? Result<Inventory>.Success(inventory)
                : Result<Inventory>.Failure("Inventory not found");
        }
        catch (Exception ex)
        {
            return Result<Inventory>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<Inventory>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.PlayerId == playerId, cancellationToken);

            return inventory != null
                ? Result<Inventory>.Success(inventory)
                : Result<Inventory>.Failure("Inventory not found");
        }
        catch (Exception ex)
        {
            return Result<Inventory>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<Inventory>> CreateAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Inventory>.Success(inventory);
        }
        catch (Exception ex)
        {
            return Result<Inventory>.Failure($"Failed to create inventory: {ex.Message}");
        }
    }

    public async Task<Result<Inventory>> UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Inventory>.Success(inventory);
        }
        catch (Exception ex)
        {
            return Result<Inventory>.Failure($"Failed to update inventory: {ex.Message}");
        }
    }
}
"@ | Out-File -FilePath "src/Infrastructure/Persistence/Repositories/InventoryRepository.cs" -Encoding UTF8

# CurrencyWalletRepository
@"
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Repositories;

public class CurrencyWalletRepository : ICurrencyWalletRepository
{
    private readonly ApplicationDbContext _context;

    public CurrencyWalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CurrencyWallet>> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        try
        {
            var wallet = await _context.CurrencyWallets
                .FirstOrDefaultAsync(w => w.WalletId == walletId, cancellationToken);

            return wallet != null
                ? Result<CurrencyWallet>.Success(wallet)
                : Result<CurrencyWallet>.Failure("Wallet not found");
        }
        catch (Exception ex)
        {
            return Result<CurrencyWallet>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyWallet>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var wallet = await _context.CurrencyWallets
                .FirstOrDefaultAsync(w => w.PlayerId == playerId, cancellationToken);

            return wallet != null
                ? Result<CurrencyWallet>.Success(wallet)
                : Result<CurrencyWallet>.Failure("Wallet not found");
        }
        catch (Exception ex)
        {
            return Result<CurrencyWallet>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyWallet>> CreateAsync(CurrencyWallet wallet, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.CurrencyWallets.Add(wallet);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<CurrencyWallet>.Success(wallet);
        }
        catch (Exception ex)
        {
            return Result<CurrencyWallet>.Failure($"Failed to create wallet: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyWallet>> UpdateAsync(CurrencyWallet wallet, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.CurrencyWallets.Update(wallet);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<CurrencyWallet>.Success(wallet);
        }
        catch (Exception ex)
        {
            return Result<CurrencyWallet>.Failure($"Failed to update wallet: {ex.Message}");
        }
    }
}
"@ | Out-File -FilePath "src/Infrastructure/Persistence/Repositories/CurrencyWalletRepository.cs" -Encoding UTF8

Write-Host "✓ Repositories créés" -ForegroundColor Green

Write-Host "`n✅ TOUT EST CRÉÉ!" -ForegroundColor Green

$count = (Get-ChildItem -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" }).Count
Write-Host "📊 Total de fichiers C#: $count" -ForegroundColor Cyan

