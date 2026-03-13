using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Admin.FreezeUser;

public sealed class FreezeUserHandler(LootopiaDbContext db) : IRequestHandler<FreezeUserCommand, Result>
{
    public async Task<Result> Handle(FreezeUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([request.UserId], cancellationToken);
        if (user is null)
            return Result.Failure(Error.Custom("User.NotFound", "User not found."));

        user.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
