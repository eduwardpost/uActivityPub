using System.Text.Json;
using Microsoft.Extensions.Options;
using Serilog;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Services;
using uActivityPub.Services.ContentServices;
using uActivityPub.Services.HelperServices;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Notifications.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ContentPublishPostHandler : INotificationHandler<ContentPublishedNotification>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    private readonly IUmbracoDatabaseFactory _databaseFactory;
    private readonly IOptions<WebRoutingSettings> _webRoutingSettings;
    private readonly IUserService _userService;
    private readonly ISignatureService _signatureService;
    private readonly ISingedRequestHandler _singedRequestHandler;
    private readonly IActivityHelper _activityHelper;
    private readonly IUActivitySettingsService _uActivitySettingsService;

    public ContentPublishPostHandler(IUmbracoDatabaseFactory databaseFactory,
        IOptions<WebRoutingSettings> webRoutingSettings, IUserService userService, ISignatureService signatureService,
        ISingedRequestHandler singedRequestHandler, IActivityHelper activityHelper, IUActivitySettingsService uActivitySettingsService)
    {
        _databaseFactory = databaseFactory;
        _webRoutingSettings = webRoutingSettings;
        _userService = userService;
        _signatureService = signatureService;
        _singedRequestHandler = singedRequestHandler;
        _activityHelper = activityHelper;
        _uActivitySettingsService = uActivitySettingsService;
    }

    public void Handle(ContentPublishedNotification notification)
    {
        var settings = _uActivitySettingsService.GetAllSettings()?.ToList();
        var contentAlias = settings?.Find(s => s.Key == uActivitySettingKeys.ContentTypeAlias);
        var userPropertyAlias = settings?.Find(s => s.Key == uActivitySettingKeys.UserNameContentAlias);

        if (contentAlias == null)
            throw new InvalidOperationException("Could not find configured key for the content type");
        if (userPropertyAlias == null)
            throw new InvalidOperationException("Could not find configured key for the user property type");

        
        
        var post = notification.PublishedEntities.FirstOrDefault();
        if (post?.ContentType.Alias != contentAlias.Value) 
            return;
        
        using var database = _databaseFactory.CreateDatabase();

        var posts =
            database.Query<ReceivedActivitiesSchema>(
                "SELECT * FROM receivedActivityPubActivities WHERE Type = @0 AND Object = @1",
                "Post", post.Id.ToString());

        if (posts.Any()) 
            return;

        string userName;
        int userId;

        if (_uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserMode)!.Value == "false")
        {
            userId = post.GetValue<int>(userPropertyAlias.Value);
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Could not find user or actor for article");
            }

            userName = user.ActivityPubUserName()!;
        }
        else
        {
            userName = _uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserModeUserName)!.Value;
            userId = uActivitySettingKeys.SingleUserModeUserId;
        }
            
        var actor = $"{_webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{userName}";
                
        var activity = _activityHelper.GetActivityFromContent(post, actor);

        var keyInfo = _signatureService.GetPrimaryKeyForUser(userName, userId).Result;
        var serializedActivity = JsonSerializer.Serialize(activity, JsonSerializerOptions);

        var followers =
            database.Query<ReceivedActivitiesSchema>(
                "SELECT * FROM receivedActivityPubActivities WHERE Type = @0 AND Object = @1",
                "Follow", actor);

        foreach (var follower in followers)
        {
            var followerActor = _signatureService.GetActor(follower.Actor).Result;
            if(followerActor == null)
                continue;
            try
            {
                var response = _singedRequestHandler.SendSingedPost(new Uri(followerActor.Inbox),
                    keyInfo.Rsa,
                    serializedActivity, keyInfo.KeyId).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("Could not post to {@Actor} response is {@Response} with content {Content}",
                        followerActor, response, response.Content.ReadAsStringAsync().Result);
                }
            }
            catch(Exception e)
            {
                Log.Information(e, "Could not post to follower");
            }
        }

        var receivedActivity = new ReceivedActivitiesSchema
        {
            Actor = actor,
            Object = post.Id.ToString(),
            Type = "Post"
        };
        database.Insert("receivedActivityPubActivities", "Id", true, receivedActivity);
    }
}