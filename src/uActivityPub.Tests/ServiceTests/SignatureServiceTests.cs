using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using uActivityPub.Data;
using uActivityPub.Models;
using uActivityPub.Services;
using uActivityPub.Services.HelperServices;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Tests.ServiceTests;

public class SignatureServiceTests
{
    private readonly Mock<IOptions<WebRoutingSettings>> _webRouterSettingsMock;

    #region testRSA
    //randomly generated keypair that is not used elsewhere
    private const string TestPrivateKeyPem = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpQIBAAKCAQEA1CUZkCH8QIXroxDE2NapFlf3QGwrTEqmQSJGTqfBawGSDhBW
mwd1OSv7DxWour9lxUsLOLBc299H6aKzYGduJON+V255i6wIvzrK23q36DduNnF5
aY48MgFKgU33/La9N9fDU4/d3yUbImyOG8doJws6GwCXIYYOLprAmNf0cmHffE9S
FMJ30LpWYR2g6n2za4DokgCQ1vyeMT2/gBAHCjfAvVPMXWaqdZbfT8VzrJepozea
a2Xp+ZmrJUdkbir/QJ2YbttvqBr4QXf17Wgl5ZmAMBP7+a4offHP6y7RHBTM2i3R
LFIS4Pgab2RKk85eVZZkDpLsl3pBHUatlL7J5wIDAQABAoIBAQDPV1dNtLmrgnhU
js4ltF/cc6s3bsE+tnMZk9stgu4anJVYY4WRzc83SpH9I0kfBHP+SSM1i8GmL+tE
IP26SjyvlxzjbRkkdsaxyE9+Bcn7DzoLwgpNLXQN148vI7otZ5k2HA5O/Hx4XPbR
RVH7OoDEMEfhHkmXvULdL+jvOZozs0cuu89FF7WusM5SE+juF27Co4e/srPTkl5z
4Xs1Aenq+Nutfp5UFRDRxVce1uVB8/EhvfBzVHG5q4stFQenTX+WaaSTfcZTbHz9
VxJTdVtPSdsyqDKiZ0EvGVUa2OPXfhRaXPsUJzVSXGBzLcfhpa1hsXhbn2KYkpIk
VYtLVf3BAoGBAO2tCSIR9W/lVnYnyEeJjqZNKYiHcb+uE/XqVAVm3nkrLfk9yeXV
I5j4NGlOc3TWVQ9JFGf4ZP3q5PAm3upKk71d2Bvrj06s37IKf8hGvDrcVyTa7JiA
8dRpWeLxfYxz85jZJKv9jbTb6FS6dugSajdhI2me/usd0kjREcErKrYHAoGBAOSA
KekztFkomwGUr1CJt/QWQ0lHeDnDmpsaWJ5nRLf2/88XKvziCb6C2v5bA5Jvccun
DKzJbrT7i3YTi7iR6zt1ogURN+wbvcndy4halHBAUy9UvZlYOBJVXqgDAj4Md4hC
sCIYwZJZpAnLz7iau2RNdslys1nWlR9m1HcAi1UhAoGBAO0Lfidnx+VA0znmtX57
uDUukjTj+VPWN3w1qHT2wv4QTbreoEXEjMtdCVDZ+JbXyWAEYADOIVOJ+al26y67
EJx2QzqddoFeM4CrrgQ+YC9IYPWWDxCO0iIqrIaGmdQCNTsFnuiWasWt6grPKhaN
fzgafqox9EbkqD0Nn2qMNf3zAoGBAKLPTw2CcbY1YHy33FKeThv04OyNp/RAkyCA
nTQAkM4jiBGeiFq3FApSYodpghoMniBlXnqe6+q4cJ4pPxy7m8g6AL9rjVUGTyxO
hH8bSzjQHHVVA+MHIKyvd0pvGmHrsB++pJEm4oXMqTzxo4f9eBf3ufO/bltMseQ1
+JzyHlUBAoGAGi09Uc7rDvWc9ba3+KP7bVp2rP5Z+rZK8NcdB2rszaiou3LgB2C4
lcf8PH1Ab97GcvLig++zlGVo0PbSiZ4r1X0v7dKnDgbEQdhzd13ooDdv97TfzdsI
aGw+fVn4z0e0N//l7HJ8s3U1sSjWa4dhnCfgqIz+cw7E7E1WTWRKqSY=
-----END RSA PRIVATE KEY-----";

    private const string TestPublicKeyPem = @"-----BEGIN RSA PUBLIC KEY-----
MIIBCgKCAQEA1CUZkCH8QIXroxDE2NapFlf3QGwrTEqmQSJGTqfBawGSDhBWmwd1
OSv7DxWour9lxUsLOLBc299H6aKzYGduJON+V255i6wIvzrK23q36DduNnF5aY48
MgFKgU33/La9N9fDU4/d3yUbImyOG8doJws6GwCXIYYOLprAmNf0cmHffE9SFMJ3
0LpWYR2g6n2za4DokgCQ1vyeMT2/gBAHCjfAvVPMXWaqdZbfT8VzrJepozeaa2Xp
+ZmrJUdkbir/QJ2YbttvqBr4QXf17Wgl5ZmAMBP7+a4offHP6y7RHBTM2i3RLFIS
4Pgab2RKk85eVZZkDpLsl3pBHUatlL7J5wIDAQAB
-----END RSA PUBLIC KEY-----";
    #endregion

    public SignatureServiceTests()
    {
        const string baseApplicationUrl = "https://localhost.test/";
        _webRouterSettingsMock = new Mock<IOptions<WebRoutingSettings>>();
        _webRouterSettingsMock.Setup(x => x.Value).Returns(new WebRoutingSettings
        {
            UmbracoApplicationUrl = baseApplicationUrl
        });
    }

    [Fact]
    public async Task GetActorReturnsActor()
    {
        //Arrange
        const string actorString = "https://localhost.test/actor/testactor";

        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        
        var actor = new Actor
        {
            Id = actorString
        };
        
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(actorString)
            .Respond("application/json", JsonSerializer.Serialize(actor));
        var client = mockHttp.ToHttpClient();

        httpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(client);


        var unitUnderTest = new SignatureService(databaseFactoryMock.Object, httpClientFactoryMock.Object,
            _webRouterSettingsMock.Object);

        //Act
        var retrievedActor = await unitUnderTest.GetActor(actorString);


        //Assert
        Assert.NotNull(retrievedActor);
        Assert.Equal(actorString, retrievedActor.Id);
    }
    
    [Fact]
    public async Task GetActorThatDoesNotUserHttpsThrowsInvalidOperationException()
    {
        //Arrange
        const string actorString = "http://localhost.test/actor/testactor";

        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
      
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(actorString)
            .Respond(HttpStatusCode.NotFound);
        
        var client = mockHttp.ToHttpClient();

        httpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(client);


        var unitUnderTest = new SignatureService(databaseFactoryMock.Object, httpClientFactoryMock.Object,
            _webRouterSettingsMock.Object);

        //Act && Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await unitUnderTest.GetActor(actorString));
    }
    
    [Fact]
    public async Task GetActorThatDoesNotExistReturnsNull()
    {
        //Arrange
        const string actorString = "https://localhost.test/actor/testactor";

        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
      
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(actorString)
            .Respond(HttpStatusCode.NotFound);
        
        var client = mockHttp.ToHttpClient();

        httpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(client);


        var unitUnderTest = new SignatureService(databaseFactoryMock.Object, httpClientFactoryMock.Object,
            _webRouterSettingsMock.Object);

        //Act
        var retrievedActor = await unitUnderTest.GetActor(actorString);


        //Assert
        Assert.Null(retrievedActor);
    }

    [Fact]
    public async Task GetPrimaryKeyForUserReturnsKeyPair()
    {
        //Arrange
        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var userKeySchema = new UserKeysSchema
        {
            Id = 2,
            PrivateKey = TestPrivateKeyPem,
            PublicKey = TestPublicKeyPem
        };
        
        databaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(databaseMock.Object);
        databaseMock.Setup(x => x.FirstOrDefaultAsync<UserKeysSchema>(It.IsAny<string>(), 1))
            .ReturnsAsync(userKeySchema);
        

        var unitUnderTest = new SignatureService(
            databaseFactoryMock.Object, 
            httpClientFactoryMock.Object,
            _webRouterSettingsMock.Object);

        //Act
        var keyPairTuple = await unitUnderTest.GetPrimaryKeyForUser("test-user", 1);


        //Assert
        Assert.NotNull(keyPairTuple.KeyId);
        Assert.Equal(TestPrivateKeyPem.Replace("\r\n", "\n"), keyPairTuple.Rsa.ExportRSAPrivateKeyPem());
        Assert.Equal(TestPublicKeyPem.Replace("\r\n", "\n"), keyPairTuple.Rsa.ExportRSAPublicKeyPem());
        Assert.Contains("activitypub/actor/test-user", keyPairTuple.KeyId);
    }
    
    [Fact]
    public async Task GetPrimaryKeyForUserReturnsGeneratedKeyPair()
    {
        //Arrange
        var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
        var databaseMock = new Mock<IUmbracoDatabase>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        databaseFactoryMock.Setup(x => x.CreateDatabase())
            .Returns(databaseMock.Object);
        databaseMock.Setup(x => x.FirstOrDefaultAsync<UserKeysSchema>(It.IsAny<string>(), 1))
            .ReturnsAsync((UserKeysSchema) null!);

        var unitUnderTest = new SignatureService(
            databaseFactoryMock.Object, 
            httpClientFactoryMock.Object,
            _webRouterSettingsMock.Object);

        //Act
        var keyPairTuple = await unitUnderTest.GetPrimaryKeyForUser("test-user", 1);


        //Assert
        Assert.NotNull(keyPairTuple.KeyId);
        Assert.False(string.IsNullOrEmpty(keyPairTuple.Rsa.ExportRSAPrivateKeyPem()));
        Assert.False(string.IsNullOrEmpty(keyPairTuple.Rsa.ExportRSAPublicKeyPem()));
        Assert.Contains("activitypub/actor/test-user", keyPairTuple.KeyId);
        databaseMock.Verify(x => x.Insert(It.IsAny<UserKeysSchema>()), Times.Once);
    }
}