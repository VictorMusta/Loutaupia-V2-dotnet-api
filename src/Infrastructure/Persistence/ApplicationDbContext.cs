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
