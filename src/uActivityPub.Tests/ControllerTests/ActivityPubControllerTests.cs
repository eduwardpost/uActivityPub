using System.Data;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using uActivityPub.Controllers;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Models;
using uActivityPub.Services;
using uActivityPub.Services.ActivityPubServices;
using uActivityPub.Services.ContentServices;
using uActivityPub.Tests.TestHelpers;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace uActivityPub.Tests.ControllerTests;

public class ActivityPubControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new Mock<IUserService>();
    private readonly Mock<IInboxService> _inboxServiceMock = new Mock<IInboxService>();
    private readonly Mock<IOutboxService> _outboxServiceMock = new Mock<IOutboxService>();
    private readonly Mock<IUmbracoDatabase> _umbracoDatabaseMock = new Mock<IUmbracoDatabase>();
    private readonly Mock<IActorService> _actorServiceMock = new Mock<IActorService>();


    private readonly Mock<IOptions<WebRoutingSettings>> _webRoutingSettingsMock =
        new Mock<IOptions<WebRoutingSettings>>();

    private readonly Mock<IScopeProvider> _scopeProviderMock = new Mock<IScopeProvider>();

    private readonly Mock<IUActivitySettingsService> _uActivitySettingsServiceMock =
        new Mock<IUActivitySettingsService>();


    private readonly ActivityPubController _unitUnderTest;

    public ActivityPubControllerTests()
    {
        const string baseApplicationUrl = "https://localhost.test/";
        _webRoutingSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = baseApplicationUrl
        });


        var scopeMock = new Mock<IScope>();
        _scopeProviderMock.Setup(x => x.CreateScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), null,
                null, null, false, false))
            .Returns(scopeMock.Object);
        scopeMock.Setup(x => x.Database)
            .Returns(_umbracoDatabaseMock.Object);

        var settings = UActivitySettingsHelper.GetSettings();
        settings.First(s => s.Key == uActivitySettingKeys.SingleUserMode).Value = "true";

        _uActivitySettingsServiceMock.Setup(x => x.GetAllSettings())
            .Returns(settings);
        _uActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserMode))
            .Returns(new uActivitySettings()
            {
                Value = "true"
            });
        _uActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserModeUserName))
            .Returns(new uActivitySettings()
            {
                Value = "uActivityPub"
            });


        _unitUnderTest = new ActivityPubController(_userServiceMock.Object, _inboxServiceMock.Object,
            _outboxServiceMock.Object, _scopeProviderMock.Object,
            _uActivitySettingsServiceMock.Object, _actorServiceMock.Object);
    }


    [Fact]
    public void GetActor_Should_Return_Actor()
    {
        // Arrange
        const string actorName = "uactivitypub";

        var fixture = new Fixture();

        var actorFixture = fixture.Build<Actor>().With(x => x.PreferredUsername, actorName).Create();
        _actorServiceMock.Setup(x => x.GetActor(It.IsAny<string>(), null)).Returns(actorFixture);

        // Act
        var actor = _unitUnderTest.GetActor(actorName);

        // Assert
        actor.Should().NotBeNull();
        actor.Should().BeAssignableTo<ActionResult<Actor>>();
        actor.Result.Should().NotBeNull();
        actor.Result.Should().BeAssignableTo<OkObjectResult>();
        actor.Result.As<OkObjectResult>().Value.Should().BeAssignableTo<Actor>();
        actor.Result.As<OkObjectResult>().Value.As<Actor>().Should().NotBeNull();
        actor.Result.As<OkObjectResult>().Value.As<Actor>().PreferredUsername.Should().Be(actorName);
    }
}