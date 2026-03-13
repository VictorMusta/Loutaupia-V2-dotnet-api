using FluentValidation;

namespace Lootopia.Api.Features.Auctions.CloseAuction;

public sealed class CloseAuctionValidator : AbstractValidator<CloseAuctionCommand>
{
    public CloseAuctionValidator()
    {
        RuleFor(x => x.AuctionId).NotEmpty();
    }
}
