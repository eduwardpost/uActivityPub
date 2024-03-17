using uActivityPub.Models;

namespace uActivityPub.Services.ActivityPubServices;

public interface IOutboxService
{
    OrderedCollection<Activity>? GetPublicOutbox(string userName);
}