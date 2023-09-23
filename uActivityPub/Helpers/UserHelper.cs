using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace uActivityPub.Helpers;

public static class UserHelper
{
    public static string? ActivityPubUserName(this IUser user)
    {
        return user.Name?.Replace(" ", "").ToLowerInvariant();
    }

    public static IUser? GetUserByActivityPubName(this IUserService userService, string userName)
    {
        userName = userName.ToLowerInvariant();
        
        return userService.GetAll(0, 100, out _)
            .FirstOrDefault(u => u.ActivityPubUserName() == userName);
        //todo do something with the paging
    }
}