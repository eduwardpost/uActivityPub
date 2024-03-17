using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using uActivityPub.Data;
using uActivityPub.Helpers;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Tests.DatabaseTests;

public class SettingsSeedHelperTests
{
    private readonly SettingSeedHelper _unitUnderTest;
    private readonly Mock<IUmbracoDatabase> _dataBaseMock;
    
    public SettingsSeedHelperTests()
    {
        var dataBaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        
        _dataBaseMock = new Mock<IUmbracoDatabase>();
        dataBaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(_dataBaseMock.Object);
        
        _unitUnderTest = new SettingSeedHelper(dataBaseFactoryMock.Object);
    }
    
    [Fact]
    public void Handle_UmbracoApplicationStartedNotification_Should_InsertSettings_If_Not_Exist()
    {
        // Arrange
        var notification = new UmbracoApplicationStartedNotification(false);
        const string primaryKeyName = "Id";
        
        // Act
        _unitUnderTest.Handle(notification);
        
        // Assert
        _dataBaseMock.Verify(x => x.Insert(uActivitySettingKeys.TableName, primaryKeyName, true, It.IsAny<uActivitySettings>()), Times.Exactly(6));
    }
}