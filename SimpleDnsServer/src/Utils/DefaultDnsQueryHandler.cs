namespace SimpleDnsServer.Utils;

using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using System.Net;

public class DefaultDnsQueryHandler : IDnsQueryHandler
{
    private readonly IDnsRecordManger recordManager;
    private readonly ILogger logger;

    public DefaultDnsQueryHandler(IDnsRecordManger recordManager, ILogger logger)
    {
        this.recordManager = recordManager;
        this.logger = logger;
    }

    public Task<DnsMessageBase?> HandleQueryAsync(DnsMessage query)
    {
        logger.LogDebug("Received DNS query: {QueryName}", query.Questions.FirstOrDefault()?.Name);
        DnsMessage responseInstance = query.CreateResponseInstance();
        if (query.Questions.Count == 1)
        {
            string str = query.Questions[0].Name.ToString();
            string? ipString = recordManager.Resolve(str);
            if (ipString != null)
            {
                responseInstance.ReturnCode = ReturnCode.NoError;
                if (query.Questions[0].RecordType == RecordType.A)
                {
                    responseInstance.AnswerRecords.Add(new ARecord(DomainName.Parse(str), 3600, IPAddress.Parse(ipString)));
                }
                else if (query.Questions[0].RecordType == RecordType.Aaaa)
                {
                    responseInstance.AnswerRecords.Add(new AaaaRecord(DomainName.Parse(str), 3600, IPAddress.Parse(ipString)));
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
        logger.LogDebug("DNS response: {AnswerCount} answers", responseInstance.AnswerRecords.Count);
        return Task.FromResult<DnsMessageBase?>(responseInstance);
    }
}
