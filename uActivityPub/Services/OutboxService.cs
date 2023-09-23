using Microsoft.Extensions.Options;
using uActivityPub.Helpers;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace uActivityPub.Services;

public class OutboxService : IOutboxService
{
    private readonly IContentService _contentService;
    private readonly IOptions<WebRoutingSettings> _webRoutingSettings;
    private readonly IActivityHelper _activityHelper;

    public OutboxService(IContentService contentService, IOptions<WebRoutingSettings> webRoutingSettings, IActivityHelper activityHelper)
    {
        _contentService = contentService;
        _activityHelper = activityHelper;
        _webRoutingSettings = webRoutingSettings;
    }
    
    public OrderedCollection<Activity>? GetPublicOutbox(IUser user)
    {
        var rootContent = _contentService.GetRootContent().FirstOrDefault();
        
        if (rootContent == null)
            return null;

        var blogRoot = _contentService.GetPagedChildren(rootContent.Id, 0, 10, out _)
            .FirstOrDefault(c => c.ContentType.Alias == "articleList");
        
        if (blogRoot == null)
            return null;

        var blogItems = _contentService.GetPagedChildren(blogRoot.Id, 0, 10, out _);
        blogItems = blogItems.Where(b => b.Published);

        var outbox = new OrderedCollection<Activity>();

        var actor = $"{_webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{user.ActivityPubUserName()}";
        
        
        foreach (var blogItem in blogItems)
        {
            outbox.Items.Add(_activityHelper.GetActivityFromContent(blogItem, actor));
        }
    
        return outbox;
    }
}