using Microsoft.AspNetCore.Routing;
using uActivityPub.Controllers.PluginControllers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace uActivityPub.Notifications;

public class uActivityPubServerVariablesHandler  : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;

    /// <inheritdoc cref="INotificationHandler{TNotification}" />
    public uActivityPubServerVariablesHandler(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;    
    }
    
    public void Handle(ServerVariablesParsingNotification notification)
    {
        notification.ServerVariables.Add("uActivityPub", new Dictionary<string, object>
        {
            { "uActivityPubService", _linkGenerator.GetUmbracoApiServiceBaseUrl<UActivityPubDashboardApiController>(controller => controller.GetApi()) }
        });
    }
}