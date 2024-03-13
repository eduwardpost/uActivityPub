using Microsoft.Extensions.Options;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace uActivityPub.Services;

public class ActivityHelper : IActivityHelper
{
    private readonly IOptions<WebRoutingSettings> _webRoutingSettings;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public ActivityHelper(
        IOptions<WebRoutingSettings> webRoutingSettings, IUmbracoContextAccessor umbracoContextAccessor, IPublishedUrlProvider publishedUrlProvider)
    {
        _webRoutingSettings = webRoutingSettings;
        _umbracoContextAccessor = umbracoContextAccessor;
        _publishedUrlProvider = publishedUrlProvider;
    }
    
    public Activity GetActivityFromContent(IContent content, string actor)
    {
        var context = _umbracoContextAccessor.GetRequiredUmbracoContext();
        var published = context.Content?.GetById(content.Id);
        
        if (published == null)
        {
            throw new InvalidOperationException("Could not produce url for content");
        }
        
        var path = _publishedUrlProvider.GetUrl(published).TrimStart('/');
        var blogUrl = $"{_webRoutingSettings.Value.UmbracoApplicationUrl}{path}";
        
        return new Activity
        {
            Type = "Create",
            Id = $"{_webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/post/{content.Id}",
            Actor = actor,
            Object = new Note
            {
                Id = blogUrl,
                Content = $"{content.GetValue<string>("metaName")}<br/>{content.GetValue<string>("metaDescription")}<br/><a href=\"{blogUrl}\">{blogUrl}</a>",
                Url = blogUrl,
                Published = $"{content.PublishDate:s}",
                To = new List<string>() { "https://www.w3.org/ns/activitystreams#Public" },
                AttributedTo = actor
            }
        };
    }
}