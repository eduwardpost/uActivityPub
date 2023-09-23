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

[Route("activitypub")]
public class ActivityPubController : UmbracoApiController
{
    private readonly IUserService _userService;
    private readonly IInboxService _inboxService;
    private readonly IOutboxService _outboxService;
    private readonly IOptions<WebRoutingSettings> _webRoutingSettings;
    private readonly IScopeProvider _scopeProvider;

    public ActivityPubController(
        IUserService userService,
        IInboxService inboxService,
        IOutboxService outboxService,
        IOptions<WebRoutingSettings> webRoutingSettings,
        IScopeProvider scopeProvider)
    {
        _userService = userService;
        _inboxService = inboxService;
        _outboxService = outboxService;
        _webRoutingSettings = webRoutingSettings;
        _scopeProvider = scopeProvider;
    }

    [HttpGet("actor/{userName}")]
    public ActionResult<Actor> GetActor(string userName)
    {
        var user = _userService.GetUserByActivityPubName(userName);
        if (user == null)
            return NotFound();

        return Ok(new Actor(user, _webRoutingSettings, _scopeProvider));
    }

    [HttpGet("actor/{userName}/followers")]
    public async Task<ActionResult<Collection>> GetFollowersCollection(string userName)
    {
        using var scope = _scopeProvider.CreateScope();
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

        var user = _userService.GetUserByActivityPubName(userName);
        if (user == null)
            return NotFound();

        var signature = Request.Headers["Signature"];
        
        try
        {
            switch (activity.Type)
            {
                case "Follow":
                    return Ok(await _inboxService.HandleFollow(activity, signature, user));
                case "Undo":
                    return Ok(await _inboxService.HandleUndo(activity, signature));
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
        var user = _userService.GetUserByActivityPubName(userName);
        if (user == null)
            return NotFound();
        
        try
        {
            return Ok(_outboxService.GetPublicOutbox(user));
        }
        catch
        {
            return StatusCode(500);
        }
    }
}