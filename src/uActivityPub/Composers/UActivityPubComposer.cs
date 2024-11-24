using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using uActivityPub.Authorization;
using uActivityPub.Data.Migrations;
using uActivityPub.Helpers;
using uActivityPub.Notifications.Handlers;
using uActivityPub.Services;
using uActivityPub.Services.ActivityPubServices;
using uActivityPub.Services.ContentServices;
using uActivityPub.Services.HelperServices;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Manifest;

namespace uActivityPub.Composers;

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class UActivityPubComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IPackageManifestReader, USyncStaticAssetsExtensions.UActivityPubAssetManifestReader>();
        
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunUserKeysMigration>();
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunReceivedActivitiesMigration>();
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunUActivitySettingsMigration>();
        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, SettingSeedHelper>();
        builder.AddNotificationHandler<ContentPublishedNotification , ContentPublishPostHandler>();
        builder.AddNotificationHandler<ServerVariablesParsingNotification, UActivityPubServerVariablesHandler>();
        builder.Services.AddTransient<IInboxService, InboxService>();
        builder.Services.AddTransient<IOutboxService, OutboxService>();
        builder.Services.AddTransient<ISignatureService, SignatureService>();
        builder.Services.AddTransient<ISingedRequestHandler, SingedRequestHandler>();
        builder.Services.AddTransient<IActivityHelper, ActivityHelper>();
        builder.Services.AddTransient<IActorService, ActorService>();
        builder.Services.AddTransient<IUActivitySettingsService, UActivitySettingsService>();
        builder.Services.AddAuthorization(o => CreatePolicies(o));
    }
    
    private static void CreatePolicies(AuthorizationOptions options, string backofficeAuthenticationScheme = Constants.Security.BackOfficeAuthenticationType)
    {
        options.AddPolicy(SyncAuthorizationPolicies.TreeAccessUActivityPub, policy =>
        {
            policy.AuthenticationSchemes.Add(backofficeAuthenticationScheme);
            policy.Requirements.Add(new BackOfficeRequirement());
        });
    }
}