using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Items.CreateItem;

public sealed class CreateItemHandler(LootopiaDbContext db) : IRequestHandler<CreateItemCommand, Result<CreateItemResponse>>
{
    public async Task<Result<CreateItemResponse>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Rarity = request.Rarity,
            Type = request.Type,
            ImageUrl = request.ImageUrl,
            IsTradeable = request.IsTradeable,
            CreatedAt = DateTime.UtcNow
        };

        db.Items.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateItemResponse(item.Id));
    }
}
