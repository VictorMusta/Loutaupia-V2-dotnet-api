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
