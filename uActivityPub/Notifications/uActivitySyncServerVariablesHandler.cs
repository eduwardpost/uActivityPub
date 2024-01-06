using Microsoft.AspNetCore.Routing;
using uActivityPub.Controllers.PluginControllers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace uActivityPub.Notifications;

public class uActivitySyncServerVariablesHandler  : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;

    /// <inheritdoc cref="INotificationHandler{TNotification}" />
    public uActivitySyncServerVariablesHandler(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;    
    }
    
    public void Handle(ServerVariablesParsingNotification notification)
    {
        notification.ServerVariables.Add("uActivitySync", new Dictionary<string, object>
        {
            { "uActivitySyncService", _linkGenerator.GetUmbracoApiServiceBaseUrl<UActivitySyncDashboardApiController>(controller => controller.GetApi()) }
        });
    }
}