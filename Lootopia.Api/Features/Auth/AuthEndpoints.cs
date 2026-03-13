using Lootopia.Api.Features.Auth.GuestLogin;
using Lootopia.Api.Features.Auth.Login;
using Lootopia.Api.Features.Auth.MagicLink;
using Lootopia.Api.Features.Auth.RefreshToken;
using Lootopia.Api.Features.Auth.Register;
using Lootopia.Api.Features.Auth.UpgradeGuest;
using Microsoft.AspNetCore.Routing;

namespace Lootopia.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        LoginEndpoint.Map(app);
        RefreshTokenEndpoint.Map(app);
        GuestLoginEndpoint.Map(app);
        UpgradeGuestEndpoint.Map(app);
        MagicLinkEndpoints.Map(app);
        RegisterEndpoint.Map(app);
    }
}
