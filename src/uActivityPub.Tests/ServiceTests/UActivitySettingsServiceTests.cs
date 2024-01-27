using System.Linq;
using FluentAssertions;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Services;
using uActivityPub.Tests.TestHelpers;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Tests.ServiceTests;

public class UActivitySettingsServiceTests
{

    private readonly IUActivitySettingsService _unitUnderTest;
    
    // Mock
    private readonly Mock<IUmbracoDatabase> _dataBaseMock;
    
    public UActivitySettingsServiceTests()
    {
        var dataBaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        _dataBaseMock = new Mock<IUmbracoDatabase>();
        
        dataBaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(_dataBaseMock.Object);

        _unitUnderTest = new UActivitySettingsService(dataBaseFactoryMock.Object);
    }

    [Fact]
    public void GetAllSettings_Returns_All_Settings_In_Database()
    {
        // Arrange
        _dataBaseMock.Setup(x => x.Fetch<uActivitySettings>(It.IsAny<string>()))
            .Returns(uActivitySettingsHelper.GetSettings());
        
        // Act
        var settings = _unitUnderTest.GetAllSettings()?.ToList();

        // Assert
        settings.Should().NotBeNull();
        settings!.Count.Should().Be(uActivitySettingsHelper.GetSettings().Count);
    }
    
    [Fact]
    public void GetSetting_Returns_Specific_Settings_In_Database()
    {
        // Arrange
        _dataBaseMock.Setup(x => x.Fetch<uActivitySettings>(It.IsAny<string>()))
            .Returns(uActivitySettingsHelper.GetSettings());
        
        // Act
        var setting = _unitUnderTest.GetSettings(uActivitySettingKeys.SingleUserMode);

        // Assert
        setting.Should().NotBeNull();
        setting!.Key.Should().Be(uActivitySettingKeys.SingleUserMode);
    }
}