using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using uActivityPub.Controllers;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Models;
using uActivityPub.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace uActivityPub.Tests.ControllerTests;

public class WebfingerControllerTests
{
    private readonly WebfingerController _unitUnderTest;
    
    // Mocks
    private readonly Mock<IUserService> _userServiceMock = new Mock<IUserService>();
    private readonly Mock<IOptions<WebRoutingSettings>> _webRoutingSettingsMock = new Mock<IOptions<WebRoutingSettings>>();

    private readonly Mock<IUActivitySettingsService> _uActivitySettingsServiceMock = new Mock<IUActivitySettingsService>();
    
    public WebfingerControllerTests()
    {
        const string baseApplicationUrl = "https://localhost.test/";
        _webRoutingSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = baseApplicationUrl
        });
        
        _unitUnderTest = new WebfingerController(_userServiceMock.Object, _webRoutingSettingsMock.Object, _uActivitySettingsServiceMock.Object);
    }

    [Fact]
    public void HandleRequest_Returns_Bad_Request_When_No_RequestQueryString_Is_Provided()
    {
        // Arrange
        _unitUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _unitUnderTest.ControllerContext.HttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>());
        
        // Act
        var response = _unitUnderTest.HandleRequest();

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public void HandleRequest_Returns_Bad_Request_When_Not_Usable_RequestQueryString_Is_Provided()
    {
        // Arrange
        _unitUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var collectionStore = new Dictionary<string, StringValues> { { "resource", "" } };
        _unitUnderTest.ControllerContext.HttpContext.Request.Query = new QueryCollection(collectionStore);
        
        // Act
        var response = _unitUnderTest.HandleRequest();

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public void HandleRequest_Returns_Bad_Request_When_Not_Usable_Resource_Is_Requested()
    {
        // Arrange
        _unitUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var collectionStore = new Dictionary<string, StringValues> { { "resource", "unit:test" } };
        _unitUnderTest.ControllerContext.HttpContext.Request.Query = new QueryCollection(collectionStore);
        
        // Act
        var response = _unitUnderTest.HandleRequest();

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public void HandleRequest_Returns_Webfinger_Response_When_Account_Is_Requested_In_Single_User_Mode()
    {
        // Arrange
        _unitUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var collectionStore = new Dictionary<string, StringValues> { { "resource", "acct:uActivityPub@umbracoSite.domain" } };
        _unitUnderTest.ControllerContext.HttpContext.Request.Query = new QueryCollection(collectionStore);

        _uActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserMode))
            .Returns(new uActivitySettings
            {
                Id = 1,
                Key = uActivitySettingKeys.SingleUserMode,
                Value = "true"
            });
        
        _uActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserModeUserName))
            .Returns(new uActivitySettings
            {
                Id = 2,
                Key = uActivitySettingKeys.SingleUserModeUserName,
                Value = "uActivityPub"
            });
        
        // Act
        var response = _unitUnderTest.HandleRequest();

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ActionResult<WebFingerResponse>>();
        response.Result.Should().BeOfType<OkObjectResult>();

        var objectResponse = response.Result as OkObjectResult;
        objectResponse.Should().NotBeNull();
        objectResponse!.Value.Should().NotBeNull();
        var webFingerResponse = objectResponse.Value as WebFingerResponse;
        webFingerResponse.Should().NotBeNull();
        webFingerResponse!.Subject.Should().Be("acct:uActivityPub@umbracoSite.domain");
        webFingerResponse.Links.Length.Should().Be(1);
        webFingerResponse.Links[0].Rel.Should().Be("self");
        webFingerResponse.Links[0].Type.Should().Be("application/activity+json");
        webFingerResponse.Links[0].Href.Should().Be("https://localhost.test/activitypub/actor/uactivitypub");
    }
    
    [Fact]
    public void HandleRequest_Returns_Webfinger_Response_When_Account_Is_Requested_Not_In_Single_User_Mode()
    {
        // Arrange
        _unitUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var collectionStore = new Dictionary<string, StringValues> { { "resource", "acct:uActivityPub@umbracoSite.domain" } };
        _unitUnderTest.ControllerContext.HttpContext.Request.Query = new QueryCollection(collectionStore);

        _uActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserMode))
            .Returns(new uActivitySettings
            {
                Id = 1,
                Key = uActivitySettingKeys.SingleUserMode,
                Value = "false"
            });

        long outTotal;
        _userServiceMock.Setup(x => x.GetAll(0, 100, out outTotal))
            .Returns(new List<IUser>
            {
                new User(new GlobalSettings())
                {
                    Name = "uActivityPub"
                }
            });
        
        // Act
        var response = _unitUnderTest.HandleRequest();

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ActionResult<WebFingerResponse>>();
        response.Result.Should().BeOfType<OkObjectResult>();

        var objectResponse = response.Result as OkObjectResult;
        objectResponse.Should().NotBeNull();
        objectResponse!.Value.Should().NotBeNull();
        var webFingerResponse = objectResponse.Value as WebFingerResponse;
        webFingerResponse.Should().NotBeNull();
        webFingerResponse!.Subject.Should().Be("acct:uActivityPub@umbracoSite.domain");
        webFingerResponse.Links.Length.Should().Be(1);
        webFingerResponse.Links[0].Rel.Should().Be("self");
        webFingerResponse.Links[0].Type.Should().Be("application/activity+json");
        webFingerResponse.Links[0].Href.Should().Be("https://localhost.test/activitypub/actor/uactivitypub");
    }
    
    [Fact]
    public void HandleRequest_Returns_NotFound_Response_When_Non_Existing_Account_Is_Requested_Not_In_Single_User_Mode()
    {
        // Arrange
        _unitUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var collectionStore = new Dictionary<string, StringValues> { { "resource", "acct:uActivityPub@umbracoSite.domain" } };
        _unitUnderTest.ControllerContext.HttpContext.Request.Query = new QueryCollection(collectionStore);

        _uActivitySettingsServiceMock.Setup(x => x.GetSettings(uActivitySettingKeys.SingleUserMode))
            .Returns(new uActivitySettings
            {
                Id = 1,
                Key = uActivitySettingKeys.SingleUserMode,
                Value = "false"
            });

        long outTotal;
        _userServiceMock.Setup(x => x.GetAll(0, 100, out outTotal))
            .Returns(new List<IUser>
            {
                Capacity = 0
            });
        
        // Act
        var response = _unitUnderTest.HandleRequest();

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ActionResult<WebFingerResponse>>();
        response.Result.Should().BeOfType<NotFoundResult>();
    }
}