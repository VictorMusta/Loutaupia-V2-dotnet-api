using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Admin.UnfreezeUser;

public sealed class UnfreezeUserHandler(LootopiaDbContext db) : IRequestHandler<UnfreezeUserCommand, Result>
{
    public async Task<Result> Handle(UnfreezeUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([request.UserId], cancellationToken);
        if (user is null)
            return Result.Failure(Error.Custom("User.NotFound", "User not found."));

        user.IsActive = true;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
