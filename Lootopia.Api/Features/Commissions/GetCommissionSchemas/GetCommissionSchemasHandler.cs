using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Commissions.GetCommissionSchemas;

public sealed class GetCommissionSchemasHandler(LootopiaDbContext db)
    : IRequestHandler<GetCommissionSchemasQuery, Result<GetCommissionSchemasResponse>>
{
    public async Task<Result<GetCommissionSchemasResponse>> Handle(GetCommissionSchemasQuery request, CancellationToken cancellationToken)
    {
        var schemas = await db.CommissionSchemas
            .OrderBy(s => s.CreatedAt)
            .Select(s => new CommissionSchemaDto(
                s.Id,
                s.Type,
                s.Value,
                s.PlatformShare,
                s.OrganiserShare,
                s.IsDefault,
                s.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetCommissionSchemasResponse(schemas));
    }
}
