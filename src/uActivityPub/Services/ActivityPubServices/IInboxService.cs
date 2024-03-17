using Microsoft.AspNetCore.Mvc;
using uActivityPub.Models;

namespace uActivityPub.Services.ActivityPubServices;

public interface IInboxService
{
    Task<Activity?> HandleFollow(Activity activity, string signature, string userName, int userId);
    Task<Activity?> HandleUndo(Activity activity, string signature);
    Task<ActionResult> HandleCreate(Activity activity, string signature);
}