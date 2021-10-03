using Microsoft.AspNetCore.Authorization;
using TwitchIrcHub.Authentication.Policies.Requirements;

namespace TwitchIrcHub.Authentication.Policies.Handler;

public class IsRegisterdAppHandler : AuthorizationHandler<IsRegisteredAppRequirements>
{
    protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context,
        IsRegisteredAppRequirements requirement)
    {
        if (context.User.HasClaim(claim => claim.Type == nameof(AuthClaims.AppKey)))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}