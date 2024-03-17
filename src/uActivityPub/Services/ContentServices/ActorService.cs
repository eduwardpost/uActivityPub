using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Scoping;

namespace uActivityPub.Services.ContentServices;

public class ActorService(
    IOptions<WebRoutingSettings> webRoutingSettings,
    IUActivitySettingsService settingsService,
    IScopeProvider scopeProvider) : IActorService
{
    public Actor GetActor(string userName, IUser? user = null)
    {
        using var scope = scopeProvider.CreateScope();
        var settings = settingsService.GetAllSettings();
        
        var singleUserMode = settings!.First(s => s.Key == uActivitySettingKeys.SingleUserMode).Value == "true";
        var gravatarEmail = settings!.First(s => s.Key == uActivitySettingKeys.GravatarEmail).Value;

        var activityPubUserName = user?.ActivityPubUserName() ?? user?.Id.ToString() ?? userName.ToLowerInvariant();
        var userId = singleUserMode ? uActivitySettingKeys.SingleUserModeUserId : user!.Id;

        var userKey = scope.Database.FirstOrDefault<UserKeysSchema>("SELECT * FROM userKeys WHERE UserId = @0",
            userId);

        if (userKey == null)
        {
            //user does not have pub private key yet, lets make a pair
            var rsa = RSA.Create(2048);

            userKey = new UserKeysSchema
            {
                Id = userId,
                PublicKey = rsa.ExportRSAPublicKeyPem(),
                PrivateKey = rsa.ExportRSAPrivateKeyPem()
            };

            scope.Database.Insert(userKey);
        }

        var actorId = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{activityPubUserName}";
        var actor = new Actor()
        {
            PreferredUsername = activityPubUserName,
            Id = actorId,
            Url = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{activityPubUserName}",
            Inbox = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/inbox/{activityPubUserName}",
            Outbox = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/outbox/{activityPubUserName}",
            Followers =
                $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{activityPubUserName}/followers",
            Icon = new Icon
            {
                Url = user?.GetGravatarUrl() ?? gravatarEmail.GetGravatarUrl()
            },
            PublicKey = new PublicKey
            {
                Id = $"{actorId}#main-key",
                Owner = actorId,
                PublicKeyPem = userKey.PublicKey
            }
        };
        scope.Complete();

        return actor;
    }
}