using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Models;
using uActivityPub.Notifications.Handlers;
using uActivityPub.Services;
using uActivityPub.Services.ContentServices;
using uActivityPub.Services.HelperServices;
using uActivityPub.Tests.TestHelpers;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Tests;

public class ContentPublishPostHandlerTests
{
    private readonly Mock<IOptions<WebRoutingSettings>> _webRouterSettingsMock;

    public ContentPublishPostHandlerTests()
    {
        const string baseApplicationUrl = "https://localhost.test/";
        _webRouterSettingsMock = new Mock<IOptions<WebRoutingSettings>>();
        _webRouterSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = baseApplicationUrl
        });
    }

    [Fact]
    public void HandlePostsToFollowers()
    {
        //Arrange
        var blogPostMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();

        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);

        iUActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserMode))
                    .Returns(new uActivitySettings
                    {
                        Id = 4,
                        Key = uActivitySettingKeys.SingleUserMode,
                        Value = "false"
                    });

        contentTypeMock.Setup(x => x.Alias).Returns("article");
        blogPostMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        blogPostMock.Setup(x => x.GetValue<int>("authorName", null, null, false))
            .Returns(1);


        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(databaseMock.Object);

        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Post", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>());
        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Follow", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>
            {
                new()
                {
                    Actor = "test-actor",
                    Object = "",
                    Type = "Follow",
                    Id = 1
                }
            });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/inbox").Respond(HttpStatusCode.Accepted);
        var client = mockHttp.ToHttpClient();

        var userServiceMock = new Mock<IUserService>();
        var userMock = new Mock<IUser>();
        userServiceMock.Setup(x => x.GetUserById(It.IsAny<int>()))
            .Returns(userMock.Object);

        var signatureServiceMock = new Mock<ISignatureService>();
        signatureServiceMock.Setup(x => x.GetActor(It.IsAny<string>()))
            .ReturnsAsync(new Actor()
            {
                Id = "actor",
                Inbox = "http://localhost/inbox"
            });

        signatureServiceMock.Setup(x => x.GetPrimaryKeyForUser("uActivityPub", 1))
            .ReturnsAsync(("key", RSA.Create(2048)));
        var singedRequestHandlerMock = new Mock<ISingedRequestHandler>();
        singedRequestHandlerMock.Setup(x =>
                x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

        var activityHelperMock = new Mock<IActivityHelper>();

        var activity = new Activity
        {
            Id = "test activity"
        };
        activityHelperMock.Setup(x => x.GetActivityFromContent(blogPostMock.Object, It.IsAny<string>()))
            .Returns(activity);

        httpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(client);


        var notification = new ContentPublishedNotification(blogPostMock.Object, null!);

        var unitUnderTest = new ContentPublishPostHandler(databaseFactoryMock.Object, _webRouterSettingsMock.Object,
            userServiceMock.Object, signatureServiceMock.Object, singedRequestHandlerMock.Object,
            activityHelperMock.Object, iUActivitySettingsServiceMock.Object);

        //Act

        unitUnderTest.Handle(notification);

        //Assert
        databaseMock.Verify(
            x => x.Insert("receivedActivityPubActivities", "Id", true, It.IsAny<ReceivedActivitiesSchema>()),
            Times.Once);
        singedRequestHandlerMock.Verify(
            x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void HandlePostsToMultipleFollowers()
    {
        //Arrange
        var blogPostMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();

        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);

        iUActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserMode))
            .Returns(new uActivitySettings
            {
                Id = 4,
                Key = uActivitySettingKeys.SingleUserMode,
                Value = "false"
            });

        contentTypeMock.Setup(x => x.Alias).Returns("article");
        blogPostMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        blogPostMock.Setup(x => x.GetValue<int>("authorName", null, null, false))
            .Returns(1);


        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(databaseMock.Object);

        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Post", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>());
        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Follow", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>
            {
                new()
                {
                    Actor = "test-actor",
                    Object = "",
                    Type = "Follow",
                    Id = 1
                },
                new()
                {
                    Actor = "test-actor2",
                    Object = "",
                    Type = "Follow",
                    Id = 2
                }
            });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/inbox").Respond(HttpStatusCode.Accepted);
        var client = mockHttp.ToHttpClient();

        var userServiceMock = new Mock<IUserService>();
        var userMock = new Mock<IUser>();
        userServiceMock.Setup(x => x.GetUserById(It.IsAny<int>()))
            .Returns(userMock.Object);

        var signatureServiceMock = new Mock<ISignatureService>();
        signatureServiceMock.Setup(x => x.GetActor(It.IsAny<string>()))
            .ReturnsAsync(new Actor()
            {
                Id = "actor",
                Inbox = "http://localhost/inbox"
            });

        signatureServiceMock.Setup(x => x.GetPrimaryKeyForUser("uActivityPub", 1))
            .ReturnsAsync(("key", RSA.Create(2048)));
        var singedRequestHandlerMock = new Mock<ISingedRequestHandler>();
        singedRequestHandlerMock.Setup(x =>
                x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

        var activityHelperMock = new Mock<IActivityHelper>();

        var activity = new Activity
        {
            Id = "test activity"
        };
        activityHelperMock.Setup(x => x.GetActivityFromContent(blogPostMock.Object, It.IsAny<string>()))
            .Returns(activity);

        httpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(client);


        var notification = new ContentPublishedNotification(blogPostMock.Object, null!);

        var unitUnderTest = new ContentPublishPostHandler(databaseFactoryMock.Object, _webRouterSettingsMock.Object,
            userServiceMock.Object, signatureServiceMock.Object, singedRequestHandlerMock.Object,
            activityHelperMock.Object, iUActivitySettingsServiceMock.Object);

        //Act

        unitUnderTest.Handle(notification);

        //Assert
        databaseMock.Verify(
            x => x.Insert("receivedActivityPubActivities", "Id", true, It.IsAny<ReceivedActivitiesSchema>()),
            Times.Once);
        singedRequestHandlerMock.Verify(
            x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public void HandlePostsToFollowersForNonArticle()
    {
        //Arrange

        var blogPostMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();

        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);


        contentTypeMock.Setup(x => x.Alias).Returns("nonArticle");
        blogPostMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);

        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();

        var userServiceMock = new Mock<IUserService>();
        var signatureServiceMock = new Mock<ISignatureService>();

        var singedRequestHandlerMock = new Mock<ISingedRequestHandler>();

        var activityHelperMock = new Mock<IActivityHelper>();


        var notification = new ContentPublishedNotification(blogPostMock.Object, null!);

        var unitUnderTest = new ContentPublishPostHandler(databaseFactoryMock.Object, _webRouterSettingsMock.Object,
            userServiceMock.Object, signatureServiceMock.Object, singedRequestHandlerMock.Object,
            activityHelperMock.Object, iUActivitySettingsServiceMock.Object);

        //Act

        unitUnderTest.Handle(notification);

        //Assert
        databaseFactoryMock.Verify(x => x.CreateDatabase(), Times.Never);
        singedRequestHandlerMock.Verify(
            x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void HandlePostsToFollowersWithAlreadyPostedArticle()
    {
        //Arrange
        var blogPostMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();

        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);

        contentTypeMock.Setup(x => x.Alias).Returns("article");
        blogPostMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        blogPostMock.Setup(x => x.GetValue<int>("authorName", null, null, false))
            .Returns(1);


        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(databaseMock.Object);

        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Post", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>
            {
                new()
                {
                    Actor = "test-actor",
                    Object = "",
                    Type = "Post",
                    Id = 1
                }
            });

        var userServiceMock = new Mock<IUserService>();
        var signatureServiceMock = new Mock<ISignatureService>();
        var singedRequestHandlerMock = new Mock<ISingedRequestHandler>();
        var activityHelperMock = new Mock<IActivityHelper>();
        var notification = new ContentPublishedNotification(blogPostMock.Object, null!);

        var unitUnderTest = new ContentPublishPostHandler(databaseFactoryMock.Object, _webRouterSettingsMock.Object,
            userServiceMock.Object, signatureServiceMock.Object, singedRequestHandlerMock.Object,
            activityHelperMock.Object, iUActivitySettingsServiceMock.Object);

        //Act

        unitUnderTest.Handle(notification);

        //Assert
        databaseMock.Verify(
            x => x.Insert("receivedActivityPubActivities", "Id", true, It.IsAny<ReceivedActivitiesSchema>()),
            Times.Never);
        singedRequestHandlerMock.Verify(
            x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void HandlePostsToFollowersForNotFoundContentAliasSettingThrowsInvalidOperation()
    {
        //Arrange
        var blogPostMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();

        contentTypeMock.Setup(x => x.Alias).Returns("article");


        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        var userServiceMock = new Mock<IUserService>();
        var signatureServiceMock = new Mock<ISignatureService>();
        var singedRequestHandlerMock = new Mock<ISingedRequestHandler>();
        var activityHelperMock = new Mock<IActivityHelper>();
        var notification = new ContentPublishedNotification(blogPostMock.Object, null!);

        var unitUnderTest = new ContentPublishPostHandler(databaseFactoryMock.Object, _webRouterSettingsMock.Object,
            userServiceMock.Object, signatureServiceMock.Object, singedRequestHandlerMock.Object,
            activityHelperMock.Object, iUActivitySettingsServiceMock.Object);

        try
        {
            //Act
            unitUnderTest.Handle(notification);
        }
        catch (Exception e)
        {
            //Assert
            Assert.IsType<InvalidOperationException>(e);
            databaseMock.Verify(
                x => x.Insert("receivedActivityPubActivities", "Id", true, It.IsAny<ReceivedActivitiesSchema>()),
                Times.Never);
            singedRequestHandlerMock.Verify(
                x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }

    [Fact]
    public void HandlePostsToFollowersForNotFoundUserThrowsInvalidOperation()
    {
        //Arrange

        var blogPostMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();
        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(new List<uActivitySettings>
            {
                new()
                {
                    Id = 1,
                    Key = uActivitySettingKeys.ContentTypeAlias,
                    Value = "unit"
                }
            });

        contentTypeMock.Setup(x => x.Alias).Returns("article");
        blogPostMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);


        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        var userServiceMock = new Mock<IUserService>();
        var signatureServiceMock = new Mock<ISignatureService>();
        var singedRequestHandlerMock = new Mock<ISingedRequestHandler>();
        var activityHelperMock = new Mock<IActivityHelper>();
        var notification = new ContentPublishedNotification(blogPostMock.Object, null!);

        var unitUnderTest = new ContentPublishPostHandler(databaseFactoryMock.Object, _webRouterSettingsMock.Object,
            userServiceMock.Object, signatureServiceMock.Object, singedRequestHandlerMock.Object,
            activityHelperMock.Object, iUActivitySettingsServiceMock.Object);

        try
        {
            //Act
            unitUnderTest.Handle(notification);
        }
        catch (Exception e)
        {
            //Assert
            Assert.IsType<InvalidOperationException>(e);
            databaseMock.Verify(
                x => x.Insert("receivedActivityPubActivities", "Id", true, It.IsAny<ReceivedActivitiesSchema>()),
                Times.Never);
            singedRequestHandlerMock.Verify(
                x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }

    [Fact]
    public void HandlePostsToFollowersDoesNotCrashIfActorNoLongerExists()
    {
        //Arrange

        var blogPostMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();

        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);

        iUActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserMode))
            .Returns(new uActivitySettings
            {
                Id = 4,
                Key = uActivitySettingKeys.SingleUserMode,
                Value = "false"
            });
        
        contentTypeMock.Setup(x => x.Alias).Returns("article");
        blogPostMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        blogPostMock.Setup(x => x.GetValue<int>("authorName", null, null, false))
            .Returns(1);


        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(databaseMock.Object);

        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Post", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>());
        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Follow", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>
            {
                new()
                {
                    Actor = "test-actor",
                    Object = "",
                    Type = "Follow",
                    Id = 1
                }
            });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/inbox").Respond(HttpStatusCode.Accepted);
        var client = mockHttp.ToHttpClient();

        var userServiceMock = new Mock<IUserService>();
        var userMock = new Mock<IUser>();
        userServiceMock.Setup(x => x.GetUserById(It.IsAny<int>()))
            .Returns(userMock.Object);

        var signatureServiceMock = new Mock<ISignatureService>();
        signatureServiceMock.Setup(x => x.GetActor(It.IsAny<string>()))
            .ReturnsAsync((Actor)null!);

        signatureServiceMock.Setup(x => x.GetPrimaryKeyForUser("uActivityPub", 1))
            .ReturnsAsync(("key", RSA.Create(2048)));
        var singedRequestHandlerMock = new Mock<ISingedRequestHandler>();

        var activityHelperMock = new Mock<IActivityHelper>();

        var activity = new Activity
        {
            Id = "test activity"
        };
        activityHelperMock.Setup(x => x.GetActivityFromContent(blogPostMock.Object, It.IsAny<string>()))
            .Returns(activity);

        httpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(client);


        var notification = new ContentPublishedNotification(blogPostMock.Object, null!);

        var unitUnderTest = new ContentPublishPostHandler(databaseFactoryMock.Object, _webRouterSettingsMock.Object,
            userServiceMock.Object, signatureServiceMock.Object, singedRequestHandlerMock.Object,
            activityHelperMock.Object, iUActivitySettingsServiceMock.Object);

        //Act

        unitUnderTest.Handle(notification);

        //Assert
        databaseMock.Verify(
            x => x.Insert("receivedActivityPubActivities", "Id", true, It.IsAny<ReceivedActivitiesSchema>()),
            Times.Once);
        singedRequestHandlerMock.Verify(
            x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void HandlePostsToFollowersDoesNotCrashIfRequestFails()
    {
        //Arrange
        var blogPostMock = new Mock<IContent>();
        var contentTypeMock = new Mock<ISimpleContentType>();
        var iUActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();

        iUActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(uActivitySettingsHelper.GetSettings);
        
        iUActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserMode))
            .Returns(new uActivitySettings
            {
                Id = 4,
                Key = uActivitySettingKeys.SingleUserMode,
                Value = "false"
            });

        contentTypeMock.Setup(x => x.Alias).Returns("article");
        blogPostMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        blogPostMock.Setup(x => x.GetValue<int>("authorName", null, null, false))
            .Returns(1);


        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(databaseMock.Object);

        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Post", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>());
        databaseMock.Setup(x => x.Query<ReceivedActivitiesSchema>(It.IsAny<string>(), "Follow", It.IsAny<string>()))
            .Returns(new List<ReceivedActivitiesSchema>
            {
                new()
                {
                    Actor = "test-actor",
                    Object = "",
                    Type = "Follow",
                    Id = 1
                }
            });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/inbox").Respond(HttpStatusCode.Accepted);
        var client = mockHttp.ToHttpClient();

        var userServiceMock = new Mock<IUserService>();
        var userMock = new Mock<IUser>();
        userServiceMock.Setup(x => x.GetUserById(It.IsAny<int>()))
            .Returns(userMock.Object);

        var signatureServiceMock = new Mock<ISignatureService>();
        signatureServiceMock.Setup(x => x.GetActor(It.IsAny<string>()))
            .ReturnsAsync(new Actor()
            {
                Id = "actor",
                Inbox = "http://localhost/inbox"
            });

        signatureServiceMock.Setup(x => x.GetPrimaryKeyForUser("uActivityPub", 1))
            .ReturnsAsync(("key", RSA.Create(2048)));
        var singedRequestHandlerMock = new Mock<ISingedRequestHandler>();
        singedRequestHandlerMock.Setup(x =>
                x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        var activityHelperMock = new Mock<IActivityHelper>();

        var activity = new Activity
        {
            Id = "test activity"
        };
        activityHelperMock.Setup(x => x.GetActivityFromContent(blogPostMock.Object, It.IsAny<string>()))
            .Returns(activity);

        httpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(client);


        var notification = new ContentPublishedNotification(blogPostMock.Object, null!);

        var unitUnderTest = new ContentPublishPostHandler(databaseFactoryMock.Object, _webRouterSettingsMock.Object,
            userServiceMock.Object, signatureServiceMock.Object, singedRequestHandlerMock.Object,
            activityHelperMock.Object, iUActivitySettingsServiceMock.Object);

        //Act

        unitUnderTest.Handle(notification);

        //Assert
        databaseMock.Verify(
            x => x.Insert("receivedActivityPubActivities", "Id", true, It.IsAny<ReceivedActivitiesSchema>()),
            Times.Once);
        singedRequestHandlerMock.Verify(
            x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }
}