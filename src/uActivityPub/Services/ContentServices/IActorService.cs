using uActivityPub.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace uActivityPub.Services.ContentServices;

public interface IActorService
{
    Actor GetActor(string userName, IUser? user = null);
}