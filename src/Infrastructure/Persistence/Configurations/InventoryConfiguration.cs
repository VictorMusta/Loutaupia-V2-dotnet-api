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
