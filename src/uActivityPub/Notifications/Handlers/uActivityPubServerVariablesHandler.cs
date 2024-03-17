using Microsoft.AspNetCore.Routing;
using uActivityPub.Controllers.PluginControllers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace uActivityPub.Notifications.Handlers;

public class UActivityPubServerVariablesHandler  : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;

    /// <inheritdoc cref="INotificationHandler{TNotification}" />
    public UActivityPubServerVariablesHandler(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));    
    }
    
    public void Handle(ServerVariablesParsingNotification notification)
    {
        notification.ServerVariables.Add("uActivityPub", new Dictionary<string, object?>
        {
            { "uActivityPubService", _linkGenerator.GetUmbracoApiServiceBaseUrl<UActivityPubDashboardApiController>(controller => controller.GetApi()) }
        });
    }
}