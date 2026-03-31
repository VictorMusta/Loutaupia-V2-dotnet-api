using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Admin.ListUsers;

public record ListUsersQuery(int Page, int Size, string? Search) : IRequest<Result<ListUsersResponse>>;

public record ListUsersResponse(IReadOnlyList<UserSummary> Items, int Total);

public record UserSummary(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    bool IsFrozen,
    DateTime CreatedAt);
