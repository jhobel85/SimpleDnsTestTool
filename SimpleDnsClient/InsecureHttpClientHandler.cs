using System.Net.Http;

namespace SimpleDnsClient
{
    /// <summary>
    /// HttpClientHandler that disables all server certificate validation (for dev/test only!)
    /// </summary>
    public class InsecureHttpClientHandler : HttpClientHandler
    {
        public InsecureHttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
    }
}
