using System.Data;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using uActivityPub.Helpers;
using uActivityPub.Models;
using uActivityPub.Services;
using uActivityPub.Services.ContentServices;
using uActivityPub.Tests.TestHelpers;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace uActivityPub.Tests.ServiceTests;

public class ActorServiceTests
{
    private readonly Mock<IOptions<WebRoutingSettings>> _webRouterSettingsMock;
    private readonly Mock<IUActivitySettingsService> _uActivitySettingsServiceMock;
    private readonly Mock<IScopeProvider> _scopeProviderMock;
    private readonly Mock<IUmbracoDatabase> _umbracoDatabaseMock;

    private readonly ActorService _unitUnderTest;
    
    public ActorServiceTests()
    {
        const string baseApplicationUrl = "https://localhost.test/";
        _webRouterSettingsMock = new Mock<IOptions<WebRoutingSettings>>();
        _webRouterSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = baseApplicationUrl
        });

        var settings = UActivitySettingsHelper.GetSettings();
        _uActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();
        _uActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(settings);
        
        _scopeProviderMock = new Mock<IScopeProvider>();
        _umbracoDatabaseMock = new Mock<IUmbracoDatabase>();
        var scopeMock = new Mock<IScope>();
        _scopeProviderMock.Setup(x => x.CreateScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), null,
                null, null, false, false))
            .Returns(scopeMock.Object);
        scopeMock.Setup(x => x.Database)
            .Returns(_umbracoDatabaseMock.Object);

        _unitUnderTest = new ActorService(_webRouterSettingsMock.Object, _uActivitySettingsServiceMock.Object,
            _scopeProviderMock.Object);
    }

    [Fact]
    public void GetActor_With_Only_UserName_Should_Return_Actor_BasedOn_String()
    {
        // Arrange
        const string userName = "uActivityPub";
        
        var settings = UActivitySettingsHelper.GetSettings();
        settings.First(s => s.Key == uActivitySettingKeys.SingleUserMode).Value = "true";
        
        _uActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(settings);
        
        //Act
        var actor = _unitUnderTest.GetActor(userName);
        
        // Assert
        actor.Type.Should().Be("Person");
        actor.Context.Should().Contain("https://www.w3.org/ns/activitystreams");
        actor.Context.Should().Contain("https://w3id.org/security/v1");
        actor.PreferredUsername.Should().Be(userName.ToLowerInvariant());
    }

    [Fact]
    public void GetActor_In_SingleUserMode_Creates_Actor()
    {
        // Arrange
        const string actorUserName = "uactivitypub";
    
        var settings = UActivitySettingsHelper.GetSettings();
        settings.First(s => s.Key == uActivitySettingKeys.SingleUserMode).Value = "true";
    
        _uActivitySettingsServiceMock.Setup(x => x.GetAllSettings()).Returns(settings);
        
        // Act
        var actor = _unitUnderTest.GetActor(actorUserName);
        
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
        const string notActorUserName = "notuactivitypub";
        var user = new Mock<IUser>();
        user.Setup(x => x.Name)
            .Returns(actorUserName);
        user.Setup(x => x.Id)
            .Returns(1);
        user.Setup(x => x.Email)
            .Returns($"{actorUserName}@unit.test");
        
        var settings = UActivitySettingsHelper.GetSettings();
        settings.First(s => s.Key == uActivitySettingKeys.SingleUserMode).Value = "false";
    
        _uActivitySettingsServiceMock.Setup(x => x.GetAllSettings()).Returns(settings);
        
        // Act
        var actor = _unitUnderTest.GetActor(notActorUserName, user.Object);
        
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