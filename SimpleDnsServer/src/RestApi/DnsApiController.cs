using Microsoft.AspNetCore.Mvc;

#nullable enable
namespace SimpleDnsServer.RestApi;

[ApiController]
[Route("dns")]
public class DnsApiController(IDnsRecordManger recordManger) : ControllerBase
{
    private readonly IDnsRecordManger recordManger = recordManger;

    [HttpPost("register")]
    public IActionResult Register(string domain, string ip)
    {
        Console.WriteLine("I will try register domain: " + domain + " ip: " + ip);
        recordManger.Register(domain, ip);
        return (IActionResult)Ok();
    }

    [HttpPost("register/session")]
    public IActionResult RegisterSession(string domain, string ip, string sessionId)
    {
        Console.WriteLine("I will try register domain in session context: " + domain + " ip: " + ip);
        recordManger.Register(domain, ip, sessionId);
        return (IActionResult)Ok();
    }

    [HttpPost("unregister")]
    public IActionResult Unregister(string domain)
    {
        Console.WriteLine("I will try unregister domain:" + domain);
        recordManger.Unregister(domain);
        return (IActionResult)Ok();
    }

    [HttpPost("unregister/session")]
    public IActionResult UnregisterSession(string sessionId)
    {
        Console.WriteLine("I will try unregister session:" + sessionId);
        recordManger.UnregisterSession(sessionId);
        return (IActionResult)Ok();
    }

    [HttpDelete("unregister/all")]
    public IActionResult UnregisterAll()
    {
        recordManger.UnregisterAll();
        return (IActionResult)Ok();
    }

    [HttpGet("resolve")]
    public IActionResult Resolve(string domain)
    {
        Console.WriteLine("I will try resolve domain:" + domain);
        string? str = recordManger.Resolve(domain);
        Console.WriteLine("Ip is: " + str);
        return (IActionResult)Ok(str ?? "");
    }

    [HttpGet("count")]
    public IActionResult RecordsCount()
    {
        Console.WriteLine("I will try get records count");
        int count = recordManger.GetCount();
        Console.WriteLine("All records count is: " + count.ToString());
        return (IActionResult)Ok((object)count);
    }

    [HttpGet("count/session")]
    public IActionResult RecordsSessionCount(string sessionId)
    {
        Console.WriteLine("I will try get records count of session:" + sessionId);
        int sessionCount = recordManger.GetSessionCount(sessionId);
        Console.WriteLine("Records count of session is: " + sessionCount.ToString());
        return (IActionResult)Ok((object)sessionCount);
    }
}
