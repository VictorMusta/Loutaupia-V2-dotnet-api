using FluentValidation;

namespace Lootopia.Api.Features.Auctions.GetAuction;

public sealed class GetAuctionValidator : AbstractValidator<GetAuctionQuery>
{
    public GetAuctionValidator()
    {
        RuleFor(x => x.AuctionId).NotEmpty();
    }
}
