using System;
using Microsoft.Extensions.Options;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using uActivityPub.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace uActivityPub.Tests;

public class ActivityHelperTests
{
    private readonly Mock<IOptions<WebRoutingSettings>> _webRouterSettingsMock;
    private readonly Mock<IUmbracoContextAccessor> _umbracoContextAccessorMock;
    private readonly Mock<IUmbracoContext> _umbracoContextMock;
    private readonly Mock<IPublishedUrlProvider> _publishedUrlProvider;
    private readonly Mock<IPublishedContentCache> _publishedContentCacheMock;
    
    public ActivityHelperTests()
    {
        _webRouterSettingsMock = new Mock<IOptions<WebRoutingSettings>>();
        _umbracoContextAccessorMock = new Mock<IUmbracoContextAccessor>();
        _umbracoContextMock = new Mock<IUmbracoContext>();
        var umbracoContextMockObject = _umbracoContextMock.Object;
        
        _publishedContentCacheMock = new Mock<IPublishedContentCache>();
        _publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        
        _umbracoContextAccessorMock
            .Setup(x => x.TryGetUmbracoContext(out umbracoContextMockObject))
            .Returns(true);
        _umbracoContextMock.Setup(x => x.Content).Returns(_publishedContentCacheMock.Object);
        
    }
    
    
    [Fact]
    public void GetActivityFromContentReturnsCreateActivityWithNote()
    {
        //Arrange
        const string actorString = "https://localhost.test/actor/testactor";
        const string baseApplicationUrl = "https://localhost.test/";
        const string path = "path-to/item";
        const string metaName = "this is the meta name";
        const string metaDesc = "this is the meta desc...";
        var publishDate = DateTime.Now;
        
        var contentMock = new Mock<IContent>();
        var publishedContentMock = new Mock<IPublishedContent>();

        _publishedContentCacheMock.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns(publishedContentMock.Object);
        _publishedUrlProvider.Setup(x => x.GetUrl(publishedContentMock.Object, UrlMode.Default, null, null))
            .Returns("/" + path);
        _webRouterSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = baseApplicationUrl
        });

        contentMock.Setup(x => x.GetValue<string>("metaName", null, null, false))
            .Returns(metaName);
        
        contentMock.Setup(x => x.GetValue<string>("metaDescription", null, null, false))
            .Returns(metaDesc);
        
        contentMock.Setup(x => x.PublishDate)
            .Returns(publishDate);   
        contentMock.Setup(x => x.Id)
            .Returns(1);

        var unitUnderTest = new ActivityHelper(_webRouterSettingsMock.Object, _umbracoContextAccessorMock.Object, _publishedUrlProvider.Object);

        //Act

        var activity = unitUnderTest.GetActivityFromContent(contentMock.Object, actorString);

        //Assert
        var expectedUrl = $"{baseApplicationUrl}{path}";
        Assert.NotNull(activity);
        Assert.Equal(actorString, activity.Actor);
        Assert.Equal("Create", activity.Type);
        Assert.Equal($"{baseApplicationUrl}activitypub/post/1", activity.Id);
        var note = activity.Object as Note;
        Assert.NotNull(note);
        Assert.Equal(expectedUrl, note?.Id);
        Assert.Equal(expectedUrl, note?.Url);
        Assert.Equal($"{publishDate:s}", note?.Published);
        Assert.Equal("https://www.w3.org/ns/activitystreams#Public", note?.To);
        Assert.Equal(actorString, note?.AttributedTo);
        Assert.Equal($"{metaName}<br/>{metaDesc}<br/><a href=\"{expectedUrl}\">{expectedUrl}</a>", note?.Content);
    }
    
    [Fact]
    public void GetActivityFromContentWithNonPublishedContentThrowsInvalidOperationException()
    {
        //Arrange
        const string actorString = "https://localhost.test/actor/testactor";
        
        var contentMock = new Mock<IContent>();

        _publishedContentCacheMock
            .Setup(x => x.GetById(It.IsAny<int>()))
            .Returns<IPublishedContent?>(null!);
       

        var unitUnderTest = new ActivityHelper(_webRouterSettingsMock.Object, _umbracoContextAccessorMock.Object, _publishedUrlProvider.Object);

        //Act
        try
        {
            _ = unitUnderTest.GetActivityFromContent(contentMock.Object, actorString);
        }
        catch (Exception e)
        {
            //Assert
            Assert.IsType<InvalidOperationException>(e);
        }
    }


}