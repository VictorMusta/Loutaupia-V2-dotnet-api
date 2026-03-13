using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Commissions.CreateCommissionSchema;

public sealed class CreateCommissionSchemaHandler(LootopiaDbContext db)
    : IRequestHandler<CreateCommissionSchemaCommand, Result<CreateCommissionSchemaResponse>>
{
    public async Task<Result<CreateCommissionSchemaResponse>> Handle(CreateCommissionSchemaCommand request, CancellationToken cancellationToken)
    {
        if (request.IsDefault)
        {
            var existingDefaults = await db.CommissionSchemas.Where(s => s.IsDefault).ToListAsync(cancellationToken);
            foreach (var s in existingDefaults)
                s.IsDefault = false;
        }

        var schema = new CommissionSchema
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Value = request.Value,
            PlatformShare = request.PlatformShare,
            OrganiserShare = request.OrganiserShare,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        db.CommissionSchemas.Add(schema);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCommissionSchemaResponse(schema.Id));
    }
}
