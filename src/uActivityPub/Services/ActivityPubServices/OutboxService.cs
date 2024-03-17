using Microsoft.Extensions.Options;
using uActivityPub.Helpers;
using uActivityPub.Models;
using uActivityPub.Services.ContentServices;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;

namespace uActivityPub.Services.ActivityPubServices;

public class OutboxService(
    IContentService contentService,
    IOptions<WebRoutingSettings> webRoutingSettings,
    IActivityHelper activityHelper,
    IUActivitySettingsService uActivitySettingsService)
    : IOutboxService
{
    public OrderedCollection<Activity>? GetPublicOutbox(string userName)
    {
        var settings = uActivitySettingsService.GetAllSettings();
        var contentListAlias = settings?.FirstOrDefault(s => s.Key == UActivitySettingKeys.ListContentTypeAlias);
        
        if (contentListAlias == null)
            throw new InvalidOperationException("Could not find configured key for the content list type");
        
        var rootContent = contentService.GetRootContent().FirstOrDefault();

        if (rootContent == null)
            return null;

        var blogRoot = contentService.GetPagedChildren(rootContent.Id, 0, 10, out _)
            .FirstOrDefault(c => c.ContentType.Alias == contentListAlias!.Value);

        if (blogRoot == null)
            return null;

        //todo get more pages(?)
        var blogItems = contentService.GetPagedChildren(blogRoot.Id, 0, 20, out _);
        blogItems = blogItems.Where(b => b.Published);

        var outbox = new OrderedCollection<Activity>();

        var actor = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{userName}";


        foreach (var blogItem in blogItems)
        {
            outbox.Items.Add(activityHelper.GetActivityFromContent(blogItem, actor));
        }

        return outbox;
    }
}