using FluentAssertions;
using J2N.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using uActivityPub.Notifications.Handlers;
using Umbraco.Cms.Core.Notifications;

namespace uActivityPub.Tests.NotificationHandlerTests;

public class UActivityPubServerVariablesHandlerTests
{
    [Fact]
    public void Handle_ServerVariablesParsingNotification_Should_Set_Service_Url()
    {
        // Arrange
        var notification = new ServerVariablesParsingNotification(new Dictionary<string, object>());
        var linkGeneratorMock = new Mock<LinkGenerator>();
        
        var unitUnderTest = new UActivityPubServerVariablesHandler(linkGeneratorMock.Object);        
        
        // Act
        unitUnderTest.Handle(notification);
        

        // Assert
        notification.ServerVariables.Should().ContainKey("uActivityPub");

    }
}