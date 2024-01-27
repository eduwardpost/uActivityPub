using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Infrastructure.Scoping;
using uActivityPub.Data;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using uActivityPub.Helpers;
using uActivityPub.Services;

namespace uActivityPub.Controllers;

[Route("/activitypub")]

public class ActivityPubController(
    IUserService userService,
    IInboxService inboxService,
    IOutboxService outboxService,
    IOptions<WebRoutingSettings> webRoutingSettings,
    IScopeProvider scopeProvider,
    IUActivitySettingsService uActivitySettingsService)
    : UmbracoApiController
{
    [HttpGet("actor/{userName}")]
    public ActionResult<Actor> GetActor(string userName)
    {
        Actor actor;
        if (uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserMode)!.Value == "false")
        {
            var user = userService.GetUserByActivityPubName(userName);
            if (user == null)
                return NotFound();


            actor = new Actor(user, webRoutingSettings, scopeProvider);
        }
        else
        {
            var user = uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserModeUserName)!.Value;
            actor = new Actor(user, webRoutingSettings, scopeProvider);
        }
        
        return Ok(actor);
    }

    [HttpGet("actor/{userName}/followers")]
    public async Task<ActionResult<Collection>> GetFollowersCollection(string userName)
    {
        using var scope = scopeProvider.CreateScope();
        var followers = await
            scope.Database.FetchAsync<ReceivedActivitiesSchema>(
                "SELECT * FROM receivedActivityPubActivities WHERE Type = @0", "Follow");

        if (!followers.Any())
            return Ok(new Collection());

        var collection = new Collection<string>();
    
        foreach (var follower in followers)
        {
            collection.Items.Add(follower.Actor);
        }

        collection.TotalItems = collection.Items.Count;

        return Ok(collection);
    }

    [HttpPost("inbox/{userName}")]
    public async Task<ActionResult<Activity>> PostInbox(string userName, [FromBody] Activity activity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string activityPubUserName;
        int userId;

        if (uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserMode)!.Value == "false")
        {
            var user = userService.GetUserByActivityPubName(userName);
            if (user == null)
                return NotFound();

            activityPubUserName = user.ActivityPubUserName()!;
            userId = user.Id;
        }
        else
        {
            userId = uActivitySettingKeys.SingleUserModeUserId;
            activityPubUserName = uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserModeUserName)!.Value;
        }

        var signature = Request.Headers["Signature"];
        
        try
        {
            switch (activity.Type)
            {
                case "Follow":
                    return Ok(await inboxService.HandleFollow(activity, signature, activityPubUserName, userId));
                case "Undo":
                    return Ok(await inboxService.HandleUndo(activity, signature));
                default:
                    return BadRequest($"{activity.Type} is not supported on this server");
            }
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpGet("outbox/{userName}")]
    public ActionResult<OrderedCollection> GetOutbox(string userName)
    {
        OrderedCollection<Activity>? outbox;
        if (uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserMode)!.Value == "false")
        {
            var user = userService.GetUserByActivityPubName(userName);
            if (user == null)
                return NotFound();

            outbox = outboxService.GetPublicOutbox(user.ActivityPubUserName()!);
        }
        else
        {
            var activityPubUserName = uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserModeUserName)!.Value;
            outbox = outboxService.GetPublicOutbox(activityPubUserName);
        }

        try
        {
            return Ok(outbox);
        }
        catch
        {
            return StatusCode(500);
        }
    }
}