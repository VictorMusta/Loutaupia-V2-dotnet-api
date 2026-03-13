using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Commissions.RequestPayout;

public sealed class RequestPayoutHandler(LootopiaDbContext db)
    : IRequestHandler<RequestPayoutCommand, Result<RequestPayoutResponse>>
{
    public async Task<Result<RequestPayoutResponse>> Handle(RequestPayoutCommand request, CancellationToken cancellationToken)
    {
        var partner = await db.Partners.FirstOrDefaultAsync(p => p.UserId == request.OrganiserId, cancellationToken);
        if (partner is null)
            return Result.Failure<RequestPayoutResponse>(Error.Custom("Payout.PartnerRequired", "Only partners (organisers) can request payouts."));

        var payout = new Payout
        {
            Id = Guid.NewGuid(),
            OrganiserId = request.OrganiserId,
            Amount = request.Amount,
            Status = "Requested",
            RequestedAt = DateTime.UtcNow
        };

        db.Payouts.Add(payout);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new RequestPayoutResponse(payout.Id, payout.Status));
    }
}
