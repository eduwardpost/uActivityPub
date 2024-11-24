using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using uActivityPub.Controllers.PluginControllers;
using Umbraco.Cms.Core;
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
            { "uActivityPubService", GetUmbracoBaseUrl<UActivityPubDashboardApiController>(_linkGenerator, controller => controller.GetApi()) }
        });
    }
    
    private static string? GetUmbracoBaseUrl<T>(LinkGenerator linkGenerator, Expression<Func<T, object?>> methodSelector) where T : Controller
    {
        var method = ExpressionHelper.GetMethodInfo(methodSelector);
        if (method == null)
        {
            throw new MissingMethodException("Could not find the method " + methodSelector + " on type " + typeof(T) +
                                             " or the result ");
        }

        return linkGenerator.GetUmbracoControllerUrl(method.Name, typeof(UActivityPubDashboardApiController))?.TrimEnd(method.Name);
        
        
    }
}