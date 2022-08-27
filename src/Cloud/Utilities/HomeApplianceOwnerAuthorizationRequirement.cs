using System.Security.Claims;
using Humanizer;
using IntelliHome.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace IntelliHome.Cloud;

public sealed class HomeApplianceOwnerAuthorizationRequirement : AuthorizationHandler<HomeApplianceOwnerAuthorizationRequirement>, IAuthorizationRequirement
{
    private readonly IDatabase _database;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly MemoryCache _homeApplianceIdToOwnerMapping;

    public HomeApplianceOwnerAuthorizationRequirement(IDatabase database, IHttpContextAccessor httpContextAccessor)
    {
        _database = database;
        _httpContextAccessor = httpContextAccessor;
        _homeApplianceIdToOwnerMapping = new MemoryCache(new MemoryCacheOptions());
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HomeApplianceOwnerAuthorizationRequirement requirement)
    {
        if (!Guid.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            context.Fail(new AuthorizationFailureReason(requirement, "unable to identify user"));
            return;
        }

        if (!Guid.TryParse(requirement._httpContextAccessor.HttpContext?.Request.Host.Host.Split(".")[0], out var homeApplianceId))
        {
            context.Fail(new AuthorizationFailureReason(requirement, "unable to identify home appliance id"));
            return;
        }

        var homeApplianceOwnerId =
            await _homeApplianceIdToOwnerMapping.GetOrCreateAsync(
                homeApplianceId,
                async entry =>
                {
                    entry.SlidingExpiration = 1.Hours();
                    return (await (await requirement.
                            _database.
                            HomeAppliances.
                            FindAsync(homeAppliance => homeAppliance.Id == homeApplianceId)).
                        SingleOrDefaultAsync())?.OwnerId;
                });

        if (homeApplianceOwnerId == null)
        {
            context.Fail(new AuthorizationFailureReason(requirement, "unable to find home appliance"));
            _homeApplianceIdToOwnerMapping.Remove(homeApplianceId);
            return;
        }

        if (homeApplianceOwnerId != userId)
        {
            context.Fail(new AuthorizationFailureReason(requirement, "Incorrect home appliance owner"));
            return;
        }

        context.Succeed(requirement);
    }
}