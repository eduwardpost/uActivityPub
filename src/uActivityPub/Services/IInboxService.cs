using uActivityPub.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace uActivityPub.Services;

public interface IInboxService
{
    Task<Activity?> HandleFollow(Activity activity, string signature, string userName, int userId);
    Task<Activity?> HandleUndo(Activity activity, string signature);
}