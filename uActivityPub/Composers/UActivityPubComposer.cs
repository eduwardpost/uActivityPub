using Microsoft.Extensions.DependencyInjection;
using uActivityPub.Data;
using uActivityPub.Data.Migrations;
using uActivityPub.Helpers;
using uActivityPub.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace uActivityPub.Composers;

// ReSharper disable once UnusedType.Global
public class UActivityPubComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunUserKeysMigration>();
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunReceivedActivitiesMigration>();
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunUActivitySettingsMigration>();
        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, SettingSeedHelper>();
        builder.AddNotificationHandler<ContentPublishedNotification , ContentPublishPostHandler>();
        builder.Services.AddTransient<IInboxService, InboxService>();
        builder.Services.AddTransient<IOutboxService, OutboxService>();
        builder.Services.AddTransient<ISignatureService, SignatureService>();
        builder.Services.AddTransient<ISingedRequestHandler, SingedRequestHandler>();
        builder.Services.AddTransient<IActivityHelper, ActivityHelper>();
        builder.Services.AddTransient<IUActivitySettingsService, UActivitySettingsService>();
    }
}