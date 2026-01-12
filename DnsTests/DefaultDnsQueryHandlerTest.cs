using Xunit;
using Moq;
using DualstackDnsServer.Utils;
using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;
using DualstackDnsServer;
using ARSoft.Tools.Net;

namespace SimpleDnsTests
{
    public class DefaultDnsQueryHandlerTest
    {
        [Fact]
        public async Task HandleQueryAsync_ReturnsNxDomain_ForUnknownDomain()
        {
            var recordMgr = new Mock<IDnsRecordManger>();
            recordMgr.Setup(m => m.Resolve(It.IsAny<string>())).Returns((string?)null);
            var logger = new Mock<ILogger>();
            var handler = new DefaultDnsQueryHandler(recordMgr.Object, logger.Object);
            var query = new DnsMessage();
            query.Questions.Add(new DnsQuestion(DomainName.Parse("unknown.com"), RecordType.A, RecordClass.INet));
            var response = await handler.HandleQueryAsync(query);
            Assert.NotNull(response);
            Assert.Equal(ReturnCode.NxDomain, ((DnsMessage)response!).ReturnCode);
        }
    }
}
