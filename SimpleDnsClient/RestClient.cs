using System.Net;
using System.Text;

namespace SimpleDnsClient
{
    public class RestClient
    {
        public Guid SessionId { get; } = Guid.NewGuid();        
        private readonly string serverUrl;

        public RestClient(string ip, int apiPort, bool useIPv6 = false)
        {
            serverUrl = BuildUrl(ip, apiPort, useIPv6);
            // Ensure /dns is always present as the base path
            if (!serverUrl.EndsWith(Constants.DNS_ROOT, StringComparison.OrdinalIgnoreCase))
                serverUrl += Constants.DNS_ROOT;
        }

        public void Register(string domain, string ip, bool registerWithSessionContext = true)
        {
            if (registerWithSessionContext)
            {
                Register(domain, ip, SessionId.ToString());
            }
            else
            {
                Register(domain, ip);
            }
        }

        private void Register(string domain, string ip, string sessionId)
        {
            WebRequest request = WebRequest.Create($"{serverUrl}/register/session?domain={domain}&ip={ip}&sessionId={sessionId}");
            request.Method = "POST";
            request.ContentLength = 0;
            WebResponse response = request.GetResponse();
            HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

            if (statusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Error while registering domain. HttpStatusCode={statusCode}");                
            }
            response.Close();
        }

        private void Register(string domain, string ip)
        {
            WebRequest request = WebRequest.Create($"{serverUrl}/register?domain={domain}&ip={ip}");
            request.Method = "POST";
            request.ContentLength = 0;
            WebResponse response = request.GetResponse();
            HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

            if (statusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Error while registering domain. HttpStatusCode={statusCode}");
            }
            response.Close();
        }

        public string Resolve(string domain)
        {
            WebRequest request = WebRequest.Create($"{serverUrl}/resolve?domain={domain}");
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

            if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NoContent)
            {     
                Console.WriteLine($"Error while resolving domain. HttpStatusCode={statusCode}");
            }

            string result = "";
            if (statusCode == HttpStatusCode.OK) result = GetResultFromResponse(response);

            response.Close();
            return result;
        }

        public void Unregister(string domain)
        {
            WebRequest request = WebRequest.Create($"{serverUrl}/unregister?domain={domain}");
            request.Method = "POST";
            request.ContentLength = 0;
            WebResponse response = request.GetResponse();
            HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

            if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NoContent)
            {
                Console.WriteLine($"Error while unregistering domain. HttpStatusCode={statusCode}");
            }
            response.Close();
        }

        public void UregisterSession()
        {
            WebRequest request = WebRequest.Create($"{serverUrl}/unregister/session?sessionId={SessionId}");
            request.Method = "POST";
            request.ContentLength = 0;
            WebResponse response = request.GetResponse();
            HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

            if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NoContent)
            {
                Console.WriteLine($"Error while unregistering domain. HttpStatusCode={statusCode}");                
            }
            response.Close();
        }

        public int SessionRecordsCount()
        {
            WebRequest request = WebRequest.Create($"{serverUrl}/count/session?sessionId={SessionId}");
            request.Method = "GET";
            request.ContentLength = 0;
            WebResponse response = request.GetResponse();
            HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

            if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NoContent)
            {
                Console.WriteLine($"Error while get SessionRecordsCount. HttpStatusCode={statusCode}");
            }

            string result = "";
            if (statusCode == HttpStatusCode.OK) result = GetResultFromResponse(response);

            response.Close();
            return int.Parse(result);
        }

        public int RecordsCount()
        {
            WebRequest request = WebRequest.Create($"{serverUrl}/count");
            request.Method = "GET";
            request.ContentLength = 0;
            WebResponse response = request.GetResponse();
            HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

            if (statusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Error while get RecordsCount. HttpStatusCode={statusCode}");                
            }
            string result = "";
            if (statusCode == HttpStatusCode.OK) result = GetResultFromResponse(response);

            response.Close();
            return int.Parse(result);
        }

        private string BuildUrl(string ip, int apiPort, bool useIPv6)
        {
            if (IPAddress.TryParse(ip, out var addr) && addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return $"http://[{ip}]:{apiPort}/{Constants.DncControllerName}";
            return $"http://{ip}:{apiPort}/{Constants.DncControllerName}";
        }

        private string GetResultFromResponse(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }        
    }
}
