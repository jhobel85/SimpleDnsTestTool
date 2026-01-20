
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

#nullable enable

namespace DualstackDnsServer.RestApi;

[ApiController]
[Route("dns")]
public class DnsApiController : ControllerBase
{
    //private const string LogPrefix = "[DnsApiController]";
    private readonly IDnsRecordManger recordManger;    
    private readonly ILogger<DnsApiController> _logger;

    public DnsApiController(IDnsRecordManger recordManger, ILogger<DnsApiController> logger)
    {
        this.recordManger = recordManger;
        this._logger = logger;
    }


    [HttpPost("register")]
    public IActionResult Register(string domain, string ip)
    {
        //logs will be printed in recordManger to avoid duplicate loggings
        //_logger.LogInformation("Register domain: {Domain} ip: {Ip}",  domain, ip);
        recordManger.Register(domain, ip);
        return Ok();
    }

    [HttpPost("register/bulk")]
    public IActionResult RegisterBulk([FromBody] IEnumerable<DnsEntryDto> entries, string? sessionId = null)
    {
        if (entries == null)
            return BadRequest("Entries are required.");
        recordManger.RegisterMany(entries, sessionId);
        return Ok(new { Registered = entries.Count() });
    }


    [HttpPost("register/session")]
    public IActionResult RegisterSession(string domain, string ip, string sessionId)
    {
        //_logger.LogInformation("Register domain in session context: {Domain} ip: {Ip}",  domain, ip);
        recordManger.Register(domain, ip, sessionId);
        return Ok();
    }


    [HttpPost("unregister")]
    public IActionResult Unregister(string domain)
    {
        //_logger.LogInformation("Unregister domain: {Domain}",  domain);
        recordManger.Unregister(domain);
        return Ok();
    }


    [HttpPost("unregister/session")]
    public IActionResult UnregisterSession(string sessionId)
    {
        //_logger.LogInformation("Unregister session: {SessionId}",  sessionId);
        recordManger.UnregisterSession(sessionId);
        return Ok();
    }


    [HttpDelete("unregister/all")]
    public IActionResult UnregisterAll()
    {
        //_logger.LogInformation("Unregister all domains");
        recordManger.UnregisterAll();
        return Ok();
    }


    [HttpGet("resolve")]
    public IActionResult Resolve(string domain)
    {
        //_logger.LogDebug("Resolve domain: {Domain}",  domain);
        string? str = recordManger.Resolve(domain);
        //_logger.LogDebug("Ip is: {Ip}",  str);
        return Ok(str ?? "");
    }



    [HttpGet("entries")]
    public ActionResult<IEnumerable<DnsEntryDto>> GetAllEntries()
    {
        //_logger.LogInformation("Get all DNS entries");
        var entries = recordManger.GetAllEntries();
        return Ok(entries);
    }


    [HttpGet("count")]
    public IActionResult RecordsCount()
    {
        //_logger.LogInformation("Get records count");
        int count = recordManger.GetCount();
        //_logger.LogInformation("All records count is: {Count}",  count);
        return Ok(count);
    }

    [HttpGet("count/session")]
    public IActionResult RecordsSessionCount(string sessionId)
    {
        //_logger.LogInformation("Get records count of session: {SessionId}",  sessionId);
        int sessionCount = recordManger.GetSessionCount(sessionId);
        //_logger.LogInformation("Records count of session is: {SessionCount}",  sessionCount);
        return Ok(sessionCount);
    }
}
