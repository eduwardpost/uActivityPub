using System.Collections.Generic;
using Microsoft.Extensions.Options;
using uActivityPub.Models;
using uActivityPub.Services;
using uActivityPub.Tests.TestHelpers;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace uActivityPub.Tests.ServiceTests;

public class OutboxServiceTests
{
    private readonly Mock<IOptions<WebRoutingSettings>> _webRouterSettingsMock;

    public OutboxServiceTests()
    {
        const string baseApplicationUrl = "https://localhost.test/";
        _webRouterSettingsMock = new Mock<IOptions<WebRoutingSettings>>();
        _webRouterSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = baseApplicationUrl
        });
    }

    [Fact]
    public void GetPublicOutboxForExistingUserReturnsOutbox()
    {
        //Arrange
        const string actorString = "https://localhost.test/actor/testactor";
        var contentServiceMock = new Mock<IContentService>();
        var activityHelperMock = new Mock<IActivityHelper>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();

        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);
        
        var rootContentMock = new Mock<IContent>();
        rootContentMock.Setup(x => x.Id)
            .Returns(1);

        contentServiceMock.Setup(x => x.GetRootContent())
            .Returns(new List<IContent>
            {
                rootContentMock.Object
            });
        
        long total = 1;
        var blogRootMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        blogRootMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        blogRootMock.Setup(x => x.Id).Returns(99);
        contentTypeMock.Setup(x => x.Alias).Returns("articleList");
        
        contentServiceMock
            .Setup(x => x.GetPagedChildren(1, It.IsAny<long>(), It.IsAny<int>(), out total, null, null))
            .Returns(new List<IContent>
            {
                blogRootMock.Object
            });

        var publishedBlogItemMock = new Mock<IContent>();
        publishedBlogItemMock.Setup(x => x.Published).Returns(true);

        contentServiceMock
            .Setup(x => x.GetPagedChildren(99, It.IsAny<long>(), It.IsAny<int>(), out total, null, null))
            .Returns(new List<IContent>()
            {
                publishedBlogItemMock.Object
            });
        
        activityHelperMock
            .Setup(x => x.GetActivityFromContent(publishedBlogItemMock.Object, actorString))
            .Returns(new Activity
            {
                Actor = actorString,
                Object = new Note
                {
                    AttributedTo = actorString
                }
            });
        
        var unitUnderTest = new OutboxService(
            contentServiceMock.Object,
            _webRouterSettingsMock.Object,
            activityHelperMock.Object,
            iUActivitySettingsServiceMock.Object);

        //Act
        var outbox = unitUnderTest.GetPublicOutbox("uActivityPub");

        //Assert
        Assert.NotNull(outbox);
        Assert.Single(outbox.Items);
        Assert.Contains("https://www.w3.org/ns/activitystreams", outbox.Context);
        activityHelperMock.Verify(x => x.GetActivityFromContent(It.IsAny<IContent>(), It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public void GetPublicOutboxForWithoutRootContentReturnsNull()
    {
        //Arrange
        var contentServiceMock = new Mock<IContentService>();
        var activityHelperMock = new Mock<IActivityHelper>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();
        
        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);
        
        contentServiceMock.Setup(x => x.GetRootContent())
            .Returns(new List<IContent>());

        var unitUnderTest = new OutboxService(
            contentServiceMock.Object,
            _webRouterSettingsMock.Object,
            activityHelperMock.Object,
            iUActivitySettingsServiceMock.Object);

        //Act
        var outbox = unitUnderTest.GetPublicOutbox("uActivityPub");

        //Assert
        Assert.Null(outbox);
    }
    
    [Fact]
    public void GetPublicOutboxForNonExistingBlogRootReturnsNull()
    {
        //Arrange
        var contentServiceMock = new Mock<IContentService>();
        var activityHelperMock = new Mock<IActivityHelper>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();
        
        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);

        var rootContentMock = new Mock<IContent>();
        rootContentMock.Setup(x => x.Id)
            .Returns(1);

        contentServiceMock.Setup(x => x.GetRootContent())
            .Returns(new List<IContent>
            {
                rootContentMock.Object
            });
        
        long total;
        contentServiceMock
            .Setup(x => x.GetPagedChildren(1, It.IsAny<int>(), It.IsAny<int>(), out total, null, null))
            .Returns(new List<IContent>());

        var unitUnderTest = new OutboxService(
            contentServiceMock.Object,
            _webRouterSettingsMock.Object,
            activityHelperMock.Object,
            iUActivitySettingsServiceMock.Object);

        //Act
        var outbox = unitUnderTest.GetPublicOutbox("uActivityPub");

        //Assert
        Assert.Null(outbox);
    }
    
}