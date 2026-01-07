using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using System.Net;

#nullable enable
namespace SimpleDnsServer;

public class DnsUdpListener : BackgroundService
{
    private readonly DnsServer udpServer;
    private readonly DnsRecordManger recordManager;

    public DnsUdpListener(DnsRecordManger recordManager, IConfiguration config)
    {            
        string ipString = Constants.ResolveDnsIp(config);
        int port = int.Parse(Constants.ResolveUdpPort(config));
        this.recordManager = recordManager;

        // Best effort dual-stack: bind both IPv4 and IPv6 endpoints
        var transportV4 = new UdpServerTransport(new IPEndPoint(IPAddress.Any, port));
        var transportV6 = new UdpServerTransport(new IPEndPoint(IPAddress.IPv6Any, port));
        udpServer = new DnsServer([transportV4, transportV6]);
        udpServer.QueryReceived += new AsyncEventHandler<QueryReceivedEventArgs>(OnQueryReceived);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
            udpServer.Start();
        else
            udpServer.Stop();            
    }

    private async Task OnQueryReceived(object sender, QueryReceivedEventArgs e)
    {
        if (e.Query is not DnsMessage query)
            return;
        logDnsMessageQuestions(query);
        DnsMessage responseInstance = query.CreateResponseInstance();
        if (query.Questions.Count == 1 && query.Questions[0].RecordType == RecordType.A)
        {
            string str = query.Questions[0].Name.ToString();
            string ipString = recordManager.Resolve(str);
            if (ipString != null)
            {
                responseInstance.ReturnCode = ReturnCode.NoError;
                responseInstance.AnswerRecords.Add((DnsRecordBase)new ARecord(DomainName.Parse(str), 3600, IPAddress.Parse(ipString)));
            }
            else
                responseInstance.ReturnCode = ReturnCode.NxDomain;
        }
        else
            responseInstance.ReturnCode = ReturnCode.ServerFailure;
        logDnsMessageAnswers(responseInstance);
        e.Response = (DnsMessageBase)responseInstance;
    }

    public virtual void Dispose()
    {
        base.Dispose();
        udpServer?.Stop();
    }

    private void LogMessageHeader(DnsMessage message)
    {
        Console.WriteLine(string.Format("ID: {0}", (object)message.TransactionID));
        Console.WriteLine(string.Format("Operation Code: {0}", (object)message.OperationCode));
        Console.WriteLine(string.Format("Is Query: {0}", (object)message.IsQuery));
        Console.WriteLine(string.Format("Is Recursion Desired: {0}", (object)message.IsRecursionDesired));
        Console.WriteLine(string.Format("Is Checking Disabled: {0}", (object)message.IsCheckingDisabled));
        Console.WriteLine(string.Format("Is Authentic Data: {0}", (object)message.IsAuthenticData));
    }

    public void logDnsMessageQuestions(DnsMessage message)
    {
        Console.WriteLine("Questions:");
        LogMessageHeader(message);
        foreach (DnsQuestion question in message.Questions)
        {
            Console.WriteLine(string.Format("Questions Name: {0}", (object)question.Name));
            Console.WriteLine(string.Format("Questions Record Type: {0}", (object)question.RecordType));
            Console.WriteLine(string.Format("Questions Record Class: {0}", (object)question.RecordClass));
        }
    }

    public void logDnsMessageAnswers(DnsMessage message)
    {
        Console.WriteLine("Answers:");
        LogMessageHeader(message);
        foreach (DnsRecordBase answerRecord in message.AnswerRecords)
        {
            Console.WriteLine(string.Format("Answer Name: {0}", (object)answerRecord.Name));
            Console.WriteLine(string.Format("Answer Record Type: {0}", (object)answerRecord.RecordType));
            if (answerRecord.RecordType == RecordType.A)
                Console.WriteLine(string.Format("Answer Ip adrress: {0}", (object)((AddressRecordBase)answerRecord).Address));
            Console.WriteLine(string.Format("Answer Time to Live: {0}", (object)answerRecord.TimeToLive));
        }
    }
}
