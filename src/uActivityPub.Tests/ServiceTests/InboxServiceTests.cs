using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using uActivityPub.Data;
using uActivityPub.Models;
using uActivityPub.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Tests.ServiceTests;

public class InboxServiceTests
{
    private readonly InboxService _unitUnderTest;

    //Mocks
    private readonly Mock<IUmbracoDatabase> _dataBaseMock;
    private readonly Mock<IOptions<WebRoutingSettings>> _webRoutingSettingsMock;
    private readonly Mock<ISignatureService> _signatureServiceMock;
    private readonly Mock<ISingedRequestHandler> _singedRequestHandlerMock;

    public InboxServiceTests()
    {
        var dataBaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        _dataBaseMock = new Mock<IUmbracoDatabase>();
        
        dataBaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(_dataBaseMock.Object);
        _webRoutingSettingsMock = new Mock<IOptions<WebRoutingSettings>>();
        _signatureServiceMock = new Mock<ISignatureService>();
        _singedRequestHandlerMock = new Mock<ISingedRequestHandler>();


        _unitUnderTest = new InboxService(dataBaseFactoryMock.Object, _webRoutingSettingsMock.Object,
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
}