using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleDnsServer;
using SimpleDnsServer.RestApi;
using Xunit;

namespace SimpleDnsTests
{
    public class DnsApiControllerTest
    {
        private readonly Mock<IDnsRecordManger> _mockRecordManager;
        private readonly DnsApiController _controller;

        public DnsApiControllerTest()
        {
            _mockRecordManager = new Mock<IDnsRecordManger>();
            _controller = new DnsApiController(_mockRecordManager.Object);
        }

        [Fact]
        public void Register_ReturnsOk()
        {
            var result = _controller.Register("example.com", "1.2.3.4");
            _mockRecordManager.Verify(m => m.Register("example.com", "1.2.3.4"), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void RegisterSession_ReturnsOk()
        {
            var result = _controller.RegisterSession("example.com", "1.2.3.4", "session1");
            _mockRecordManager.Verify(m => m.Register("example.com", "1.2.3.4", "session1"), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Unregister_ReturnsOk()
        {
            var result = _controller.Unregister("example.com");
            _mockRecordManager.Verify(m => m.Unregister("example.com"), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void UnregisterSession_ReturnsOk()
        {
            var result = _controller.UnregisterSession("session1");
            _mockRecordManager.Verify(m => m.UnregisterSession("session1"), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void UnregisterAll_ReturnsOk()
        {
            var result = _controller.UnregisterAll();
            _mockRecordManager.Verify(m => m.UnregisterAll(), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Resolve_ReturnsOkWithIp()
        {
            _mockRecordManager.Setup(m => m.Resolve("example.com")).Returns("1.2.3.4");
            var result = _controller.Resolve("example.com") as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal("1.2.3.4", result.Value);
        }

        [Fact]
        public void RecordsCount_ReturnsOkWithCount()
        {
            _mockRecordManager.Setup(m => m.GetCount()).Returns(5);
            var result = _controller.RecordsCount() as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(5, result.Value);
        }

        [Fact]
        public void RecordsSessionCount_ReturnsOkWithSessionCount()
        {
            _mockRecordManager.Setup(m => m.GetSessionCount("session1")).Returns(2);
            var result = _controller.RecordsSessionCount("session1") as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(2, result.Value);
        }
    }
}
