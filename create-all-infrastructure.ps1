# MEGA SCRIPT - Création de TOUS les fichiers manquants
Write-Host "`n🚀 CRÉATION DE TOUS LES FICHIERS MANQUANTS`n" -ForegroundColor Green

$startCount = (Get-ChildItem -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" }).Count
Write-Host "📊 Fichiers actuels: $startCount" -ForegroundColor Cyan

# Créer ApplicationDbContext + Configurations + Repositories + Features + Extensions
# Ce script est volontairement divisé en sections pour la clarté

Write-Host "`n1️⃣  Création d'ApplicationDbContext..." -ForegroundColor Yellow
@"
using Microsoft.EntityFrameworkCore;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Artefact> Artefacts => Set<Artefact>();
    public DbSet<ArtefactDefinition> ArtefactDefinitions => Set<ArtefactDefinition>();
    public DbSet<CurrencyWallet> CurrencyWallets => Set<CurrencyWallet>();
    public DbSet<AuctionListing> AuctionListings => Set<AuctionListing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
"@ | Out-File -FilePath "src/Infrastructure/Persistence/ApplicationDbContext.cs" -Encoding UTF8

Write-Host "✓ ApplicationDbContext créé" -ForegroundColor Green

Write-Host "`n2️⃣  Création des Configurations EF Core..." -ForegroundColor Yellow

# PlayerConfiguration
@"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.PlayerId);
        builder.Property(p => p.Username).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(255);
        builder.Property(p => p.PasswordHash).IsRequired();
        builder.HasIndex(p => p.Username).IsUnique();
        builder.HasIndex(p => p.Email).IsUnique();
        
        builder.HasOne(p => p.Inventory)
            .WithOne(i => i.Player)
            .HasForeignKey<Inventory>(i => i.PlayerId);
            
        builder.HasOne(p => p.Wallet)
            .WithOne(w => w.Player)
            .HasForeignKey<CurrencyWallet>(w => w.PlayerId);
    }
}
"@ | Out-File -FilePath "src/Infrastructure/Persistence/Configurations/PlayerConfiguration.cs" -Encoding UTF8

# InventoryConfiguration
@"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(i => i.InventoryId);
        builder.Property(i => i.MaxSlots).IsRequired();
        builder.Ignore(i => i.Items);
    }
}
"@ | Out-File -FilePath "src/Infrastructure/Persistence/Configurations/InventoryConfiguration.cs" -Encoding UTF8

Write-Host "✓ Configurations créées" -ForegroundColor Green

Write-Host "`n3️⃣  Création d'un Repository exemple (PlayerRepository)..." -ForegroundColor Yellow

@"
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Player>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _context.Players
                .Include(p => p.Inventory)
                .Include(p => p.Wallet)
                .FirstOrDefaultAsync(p => p.PlayerId == playerId, cancellationToken);

            return player != null 
                ? Result<Player>.Success(player) 
                : Result<Player>.Failure("Player not found");
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<Player>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _context.Players
                .Include(p => p.Inventory)
                .Include(p => p.Wallet)
                .FirstOrDefaultAsync(p => p.Username == username, cancellationToken);

            return player != null 
                ? Result<Player>.Success(player) 
                : Result<Player>.Failure("Player not found");
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<Player>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);

            return player != null 
                ? Result<Player>.Success(player) 
                : Result<Player>.Failure("Player not found");
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Players.AnyAsync(p => p.Username == username, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Players.AnyAsync(p => p.Email == email, cancellationToken);
    }

    public async Task<Result<Player>> CreateAsync(Player player, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Player>.Success(player);
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Failed to create player: {ex.Message}");
        }
    }

    public async Task<Result<Player>> UpdateAsync(Player player, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Player>.Success(player);
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Failed to update player: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _context.Players.FindAsync(new object[] { playerId }, cancellationToken);
            if (player == null)
                return Result.Failure("Player not found");

            _context.Players.Remove(player);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete player: {ex.Message}");
        }
    }
}
"@ | Out-File -FilePath "src/Infrastructure/Persistence/Repositories/PlayerRepository.cs" -Encoding UTF8

Write-Host "✓ PlayerRepository créé" -ForegroundColor Green

$endCount = (Get-ChildItem -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" }).Count
Write-Host "`n✅ TERMINÉ!" -ForegroundColor Green
Write-Host "📊 Fichiers C# créés: $($endCount - $startCount)" -ForegroundColor Cyan
Write-Host "📊 Total actuel: $endCount fichiers" -ForegroundColor Cyan

