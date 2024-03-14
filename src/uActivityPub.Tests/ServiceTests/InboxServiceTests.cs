using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using uActivityPub.Data;
using uActivityPub.Models;
using uActivityPub.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Tests.ServiceTests;

public class InboxServiceTests
{
    private readonly InboxService _unitUnderTest;

    //Mocks
    const string BaseApplicationUrl = "https://localhost.test/";
    private readonly Mock<IUmbracoDatabase> _dataBaseMock;
    private readonly Mock<ISignatureService> _signatureServiceMock;
    private readonly Fixture _fixture;
    private Mock<ISingedRequestHandler> _singedRequestHandlerMock;

    public InboxServiceTests()
    {
        _fixture = new Fixture();

        var loggingMock = new Mock<ILogger<InboxService>>();
        var dataBaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var umbracoContextAccessorMock = new Mock<IUmbracoContextAccessor>();
        var eventAggregatorMock = new Mock<IEventAggregator>();
        
        _dataBaseMock = new Mock<IUmbracoDatabase>();
        
        dataBaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(_dataBaseMock.Object);
        Mock<IOptions<WebRoutingSettings>> webRoutingSettingsMock = new();
        _signatureServiceMock = new Mock<ISignatureService>();
        _singedRequestHandlerMock = new();
        
        
        webRoutingSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = BaseApplicationUrl
        });


        _unitUnderTest = new InboxService(loggingMock.Object, dataBaseFactoryMock.Object, umbracoContextAccessorMock.Object, eventAggregatorMock.Object, webRoutingSettingsMock.Object,
            _signatureServiceMock.Object, _singedRequestHandlerMock.Object);
    }

    [Fact]
    public async Task HandleUndo_Returns_Null_After_Delete()
    {
        // Arrange
        var followActivity = JObject.FromObject(new Activity()
        {
            Type = "Follow",
            Actor = "UnitTest"
        });
        
        var activity = new Activity
        {
            Object = followActivity
        };
        
        var receivedActivitiesSchema = new ReceivedActivitiesSchema();

        _dataBaseMock.Setup(x =>
                x.FirstOrDefaultAsync<ReceivedActivitiesSchema>(It.IsAny<string>(), "Follow", "UnitTest"))
            .ReturnsAsync(receivedActivitiesSchema);

        
        // Act
        var returnedActivity = await _unitUnderTest.HandleUndo(activity, string.Empty);
        
        
        // Assert
        _dataBaseMock.Verify(x => x.DeleteAsync(receivedActivitiesSchema), Times.Once);
        returnedActivity.Should().BeNull();
    }


    [Fact]
    public async Task HandleFollow_Returns_ResponseActivity()
    {
        // Arrange
        const string userName = "UnitTest";
        const int userId = -1;
        
        var activity = new Activity
        {
            Type = "Follow",
            Actor = userName
        };


        var actor = _fixture.Build<Actor>()
            .With(a => a.PreferredUsername, userName)
            .With(a => a.Inbox, $"{BaseApplicationUrl}inbox")
            .Create();

        _signatureServiceMock.Setup(x => x.GetActor(activity.Actor))
            .ReturnsAsync(actor);

        _fixture.Customizations.Add(
            new TypeRelay(
                typeof(HttpContent),
                typeof(StringContent)));
        
        var responseMessage = _fixture.Build<HttpResponseMessage>()
            .With(m => m.Content, new StringContent("unit test"))
            .Create();
        
        _singedRequestHandlerMock.Setup(x => x.SendSingedPost(It.IsAny<Uri>(), It.IsAny<RSA>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(responseMessage);
        
        // Act
        var returnedActivity = await _unitUnderTest.HandleFollow(activity, string.Empty, userName, userId);

        // Assert
        returnedActivity.Should().NotBeNull();
        returnedActivity!.Type.Should().Be("Accept");
        
        _dataBaseMock.Verify(x => x.Insert("receivedActivityPubActivities", "Id", true, It.IsAny<ReceivedActivitiesSchema>()), Times.Once);
        
    }
}