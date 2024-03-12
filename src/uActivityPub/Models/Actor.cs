using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Scoping;

namespace uActivityPub.Models;

public class Actor : ActivityPubBase
{
    // ReSharper disable once MemberCanBePrivate.Global
    public string PreferredUsername { get; set; } = default!;

    // ReSharper disable once UnusedMember.Global
    public string Name => PreferredUsername;

    public string Inbox { get; set; } = default!;
    public string? Outbox { get; set; }
    public string? Followers { get; set; }
    public Icon? Icon { get; set; }
    public DateTime? Published { get; set; } = new DateTime(2023, 07, 17, 12, 00, 00);
    public DateTime? Updated { get; set; } = DateTime.Now;
    public bool ManuallyApprovesFollowers { get; set; } = false;

    public PublicKey? PublicKey { get; set; }

    /// <summary>
    /// Create an empty actor
    /// </summary>
    public Actor()
    {
        Type = "Person";
        Context = new[]
        {
            "https://www.w3.org/ns/activitystreams",
            "https://w3id.org/security/v1"
        };
    }

    /// <summary>
    /// Create an actor for uActivityPub
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="webRoutingSettings"></param>
    /// <param name="settingsService"></param>
    /// <param name="scopeProvider"></param>
    /// <param name="user"></param>
    public Actor(string userName, IOptions<WebRoutingSettings> webRoutingSettings,
        IUActivitySettingsService settingsService, IScopeProvider scopeProvider, IUser? user = null) : this()
    {
        using var scope = scopeProvider.CreateScope();
        var settings = settingsService.GetAllSettings();
        var singleUserMode = settings!.First(s => s.Key == uActivitySettingKeys.SingleUserMode).Value == "true";

        var activityPubUserName = user?.ActivityPubUserName() ?? user?.Id.ToString() ?? userName.ToLowerInvariant();
        var userId = singleUserMode ? uActivitySettingKeys.SingleUserModeUserId : user!.Id;

        PreferredUsername = activityPubUserName;
        Id =
            $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{activityPubUserName}";
        Inbox =
            $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/inbox/{activityPubUserName}";
        Outbox =
            $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/outbox/{activityPubUserName}";
        Followers =
            $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{activityPubUserName}/followers";
        Icon = new Icon
        {
            Url = user?.GetGravatarUrl() ?? userName.GetGravatarUrl() //todo get url to avatar if custom uploaded
        };


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


        PublicKey = new PublicKey
        {
            Id = $"{Id}#main-key",
            Owner = Id,
            PublicKeyPem = userKey.PublicKey
        };

        scope.Complete();
    }
}