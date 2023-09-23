using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using uActivityPub.Data;
using uActivityPub.Helpers;
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
    
    public Actor()
    {
        Type = "Person";
        Context = new[]
        {
            "https://www.w3.org/ns/activitystreams",
            "https://w3id.org/security/v1"
        };
    }

    public Actor(IUser user, IOptions<WebRoutingSettings> webRoutingSettings, IScopeProvider scopeProvider) : this()
    {
        PreferredUsername = user.ActivityPubUserName() ?? user.Id.ToString();
        Id = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{user.ActivityPubUserName()}";
        Inbox = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/inbox/{user.ActivityPubUserName()}";
        Outbox = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/outbox/{user.ActivityPubUserName()}";
        Followers = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{user.ActivityPubUserName()}/followers";
        Icon = new Icon
        {
            Url = user.GetGravatarUrl() //todo get url to avatar if custom uploaded
        };

        using var scope = scopeProvider.CreateScope();
        
        var userKey = scope.Database.FirstOrDefault<UserKeysSchema>("SELECT * FROM userKeys WHERE UserId = @0", user.Id);
        if (userKey == null)
        {
            //user does not have pub private key yet, lets make a pair
            var rsa = RSA.Create(2048);

            userKey = new UserKeysSchema
            {
                Id = user.Id,
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