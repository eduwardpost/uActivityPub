using System.Data;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Models;
using uActivityPub.Services;
using uActivityPub.Tests.TestHelpers;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace uActivityPub.Tests.ModelTests;

public class ActorTests
{
    private readonly Mock<IOptions<WebRoutingSettings>> _webRouterSettingsMock;
    private readonly Mock<IUActivitySettingsService> _uActivitySettingsServiceMock;
    private readonly Mock<IScopeProvider> _scopeProviderMock;
    private readonly Mock<IUmbracoDatabase> _umbracoDatabaseMock;

    public ActorTests()
    {
        const string baseApplicationUrl = "https://localhost.test/";
        _webRouterSettingsMock = new Mock<IOptions<WebRoutingSettings>>();
        _webRouterSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = baseApplicationUrl
        });

        _uActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();
        _scopeProviderMock = new Mock<IScopeProvider>();
        _umbracoDatabaseMock = new Mock<IUmbracoDatabase>();
        var scopeMock = new Mock<IScope>();
        _scopeProviderMock.Setup(x => x.CreateScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), null,
                null, null, false, false))
            .Returns(scopeMock.Object);
        scopeMock.Setup(x => x.Database)
            .Returns(_umbracoDatabaseMock.Object);
    }

    [Fact]
    public void Calling_Parameterless_Constructor_Creates_Simple_Actor()
    {
        // Arrange & Act
        var actor = new Actor();
        
        // Assert
        actor.Type.Should().Be("Person");
        actor.Context.Should().Contain("https://www.w3.org/ns/activitystreams");
        actor.Context.Should().Contain("https://w3id.org/security/v1");
    }

    [Fact]
    public void Calling_Parameter_Constructor_In_SingleUserMode_Creates_Actor()
    {
        // Arrange
        const string actorUserName = "uactivitypub";

        var settings = uActivitySettingsHelper.GetSettings();
        settings.First(s => s.Key == uActivitySettingKeys.SingleUserMode).Value = "true";

        _uActivitySettingsServiceMock.Setup(x => x.GetAllSettings()).Returns(settings);
        
        // Act
        var actor = new Actor(actorUserName, _webRouterSettingsMock.Object, _uActivitySettingsServiceMock.Object,
            _scopeProviderMock.Object);
        
        // Assert
        actor.Should().NotBeNull();
        actor.PreferredUsername.Should().Be(actorUserName);
        actor.Id.Should().Contain(actorUserName);
        actor.Inbox.Should().Contain(actorUserName);
        actor.Outbox.Should().Contain(actorUserName);
        actor.Followers.Should().Contain(actorUserName);
        actor.Icon.Should().NotBeNull();
        actor.Icon!.Url.Should().NotBeNull();
        actor.PublicKey.Should().NotBeNull();
    }
    
    [Fact]
    public void Calling_Parameter_Constructor_In_MultiUserMode_Creates_Actor()
    {
        // Arrange
        const string actorUserName = "uactivitypub";
        var user = new Mock<IUser>();
        user.Setup(x => x.Name)
            .Returns(actorUserName);
        user.Setup(x => x.Id)
            .Returns(1);
        user.Setup(x => x.Email)
            .Returns($"{actorUserName}@unit.test");
        
        var settings = uActivitySettingsHelper.GetSettings();

        _uActivitySettingsServiceMock.Setup(x => x.GetAllSettings()).Returns(settings);
        
        // Act
        var actor = new Actor(actorUserName, _webRouterSettingsMock.Object, _uActivitySettingsServiceMock.Object,
            _scopeProviderMock.Object, user.Object);
        
        // Assert
        actor.Should().NotBeNull();
        actor.PreferredUsername.Should().Be(actorUserName);
        actor.Id.Should().Contain(actorUserName);
        actor.Inbox.Should().Contain(actorUserName);
        actor.Outbox.Should().Contain(actorUserName);
        actor.Followers.Should().Contain(actorUserName);
        actor.Icon.Should().NotBeNull();
        actor.Icon!.Url.Should().NotBeNull();
        actor.PublicKey.Should().NotBeNull();
    }
}