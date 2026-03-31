using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Admin.ListUsers;

public sealed class ListUsersHandler(LootopiaDbContext db)
    : IRequestHandler<ListUsersQuery, Result<ListUsersResponse>>
{
    public async Task<Result<ListUsersResponse>> Handle(
        ListUsersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                u.DisplayName.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(u => new UserSummary(
                u.Id,
                u.Email ?? "",
                u.DisplayName,
                u.Role.ToString(),
                !u.IsActive,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new ListUsersResponse(users, total));
    }
}
