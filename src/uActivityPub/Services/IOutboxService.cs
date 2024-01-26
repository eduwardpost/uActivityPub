using uActivityPub.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace uActivityPub.Services;

public interface IOutboxService
{
    OrderedCollection<Activity>? GetPublicOutbox(IUser user);
}