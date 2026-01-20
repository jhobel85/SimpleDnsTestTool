namespace DualstackDnsServer.Utils;

using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DualstackDnsServer.Services;
using System.Net;

public class DnsQueryHandler : IDnsQueryHandler
{
    private const string LogPrefix = "[DnsQueryHandler]";
    private readonly IDnsRecordManger recordManager;
    private readonly ILogger logger;

    public DnsQueryHandler(IDnsRecordManger recordManager, ILogger logger)
    {
        this.recordManager = recordManager;
        this.logger = logger;
    }

    public Task<DnsMessageBase?> HandleQueryAsync(DnsMessage query)
    {
        var question = query.Questions.FirstOrDefault();
        var recordType = question?.RecordType.ToString() ?? "<unknown>";
        logger.LogDebug("{Prefix} Received DNS query: {QueryName} type={RecordType}", LogPrefix, question?.Name, recordType);
        DnsMessage responseInstance = query.CreateResponseInstance();
        if (query.Questions.Count == 1)
        {
            string str = query.Questions[0].Name.ToString();
            string? ipString = recordManager.Resolve(str);
            if (ipString != null)
            {
                responseInstance.ReturnCode = ReturnCode.NoError;
                var ip = IPAddress.Parse(ipString);
                if (query.Questions[0].RecordType == RecordType.A)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        responseInstance.AnswerRecords.Add(new ARecord(DomainName.Parse(str), 3600, ip));
                    else
                    {
                        responseInstance.ReturnCode = ReturnCode.NxDomain;
                        responseInstance.AnswerRecords.Clear();
                        logger.LogDebug("{Prefix} Record exists but is not IPv4 for A query: {QueryName} -> {Ip}", LogPrefix, str, ipString);
                    }
                }
                else if (query.Questions[0].RecordType == RecordType.Aaaa)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        responseInstance.AnswerRecords.Add(new AaaaRecord(DomainName.Parse(str), 3600, ip));
                    else
                    {
                        responseInstance.ReturnCode = ReturnCode.NxDomain;
                        responseInstance.AnswerRecords.Clear();
                        logger.LogDebug("{Prefix} Record exists but is not IPv6 for AAAA query: {QueryName} -> {Ip}", LogPrefix, str, ipString);
                    }
                }
                else
                {
                    responseInstance.ReturnCode = ReturnCode.ServerFailure;
                }
            }
            else
            {
                responseInstance.ReturnCode = ReturnCode.NxDomain;
            }
        }
        else
        {
            responseInstance.ReturnCode = ReturnCode.ServerFailure;
        }
        logger.LogDebug("{Prefix} DNS response: {AnswerCount} answers for type={RecordType}", LogPrefix, responseInstance.AnswerRecords.Count, recordType);
        return Task.FromResult<DnsMessageBase?>(responseInstance);
    }
}
