using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Wallet.GetTransactions;

public record GetTransactionsQuery(Guid UserId, int Page = 1, int Size = 20)
    : IRequest<Result<GetTransactionsResponse>>;

public record GetTransactionsResponse(
    IReadOnlyList<TransactionDto> Items,
    int Page,
    int Size,
    int TotalCount);

public record TransactionDto(
    Guid Id,
    decimal Amount,
    string Type,
    string Reason,
    DateTime CreatedAt);
