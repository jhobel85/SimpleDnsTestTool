using Microsoft.AspNetCore.Mvc;
using Moq;
using DualstackDnsServer.RestApi;
using DualstackDnsServer.Client;

namespace DualstackDnsServer;

public class DnsQueryControllerTest
{
    private const int PORT = 53;

    [Fact]
    public async Task Query_ReturnsOk_WhenDomainIsResolved()
    {        
    // Arrange
        var mockService = new Mock<IDnsUdpClient>();
        mockService.Setup(s => s.QueryDnsAsync("8.8.8.8", "example.com", PORT, QueryType.A, It.IsAny<CancellationToken>()))
            .ReturnsAsync("1.2.3.4");
        var controller = new DnsQueryController(mockService.Object);

        // Act
        var result = await controller.QueryWithServer("example.com", "8.8.8.8", PORT, "A");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("1.2.3.4", okResult.Value);
    }

    [Fact]
    public async Task Query_ReturnsNotFound_WhenNoIpFound()
    {
        // Arrange
        var mockService = new Mock<IDnsUdpClient>();
        mockService.Setup(s => s.QueryDnsAsync("8.8.8.8", "notfound.com", PORT, QueryType.A, It.IsAny<CancellationToken>()))
            .ReturnsAsync("");
        var controller = new DnsQueryController(mockService.Object);

        // Act
        var result = await controller.QueryWithServer("notfound.com", "8.8.8.8", PORT, "A");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Query_ReturnsBadRequest_WhenDomainMissing()
    {
        // Arrange
        var mockService = new Mock<IDnsUdpClient>();
        var controller = new DnsQueryController(mockService.Object);

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = await controller.QueryWithServer(null, "8.8.8.8", PORT);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Query_ReturnsBadRequest_WhenDnsServerMissing()
    {
        // Arrange
        var mockService = new Mock<IDnsUdpClient>();
        var controller = new DnsQueryController(mockService.Object);

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = await controller.Query("example.com");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Query_ReturnsServerError_OnException()
    {
        // Arrange
        var mockService = new Mock<IDnsUdpClient>();
        mockService.Setup(s => s.QueryDnsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<QueryType>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("fail"));

        var serverOptions = new ServerOptions { Ip = "8.8.8.8", IpV6 = "::1", UdpPort = PORT };
        var controller = new DnsQueryController(mockService.Object, serverOptions);

        // Act
        var result = await controller.Query("example.com");

        // Assert
        var serverError = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverError.StatusCode);
    }
}
