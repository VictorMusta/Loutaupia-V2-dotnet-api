using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Wallet.GetWallet;

public record GetWalletQuery(Guid UserId) : IRequest<Result<GetWalletResponse>>;

public record GetWalletResponse(decimal Balance, string Currency, DateTime UpdatedAt);
