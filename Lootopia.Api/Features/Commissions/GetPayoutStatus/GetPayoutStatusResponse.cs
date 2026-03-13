namespace Lootopia.Api.Features.Commissions.GetPayoutStatus;

public record GetPayoutStatusResponse(IReadOnlyList<PayoutDto> Payouts, int TotalCount);

public record PayoutDto(
    Guid Id,
    decimal Amount,
    string Status,
    DateTime RequestedAt,
    DateTime? SettledAt);
