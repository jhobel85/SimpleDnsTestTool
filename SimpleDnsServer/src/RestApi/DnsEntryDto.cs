namespace SimpleDnsServer.RestApi
{
    public class DnsEntryDto
    {
        public string Domain { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
    }
}