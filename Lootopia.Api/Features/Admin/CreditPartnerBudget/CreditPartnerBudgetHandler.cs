using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Admin.CreditPartnerBudget;

public sealed class CreditPartnerBudgetHandler(LootopiaDbContext db) : IRequestHandler<CreditPartnerBudgetCommand, Result>
{
    public async Task<Result> Handle(CreditPartnerBudgetCommand request, CancellationToken cancellationToken)
    {
        var partner = await db.Partners.FindAsync([request.PartnerId], cancellationToken);
        if (partner is null)
            return Result.Failure(Error.Custom("Partner.NotFound", "Partner not found."));

        partner.TokenBudget += request.Amount;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
