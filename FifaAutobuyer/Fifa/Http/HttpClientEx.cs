using FifaAutobuyer.Fifa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Http
{
    public class HttpClientEx
    {
        public const string UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36"
            ;
        private readonly HttpClient _httpClient;

        public HttpClientHandler ClientHandler { get; set; } // HttpClientHandler
        public HttpClientEx()
        {
            ClientHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip }; // HttpClientHandler
            ClientHandler.AllowAutoRedirect = true;
            ClientHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.MaxServicePoints = int.MaxValue;
            ServicePointManager.SetTcpKeepAlive(true, 6000000, 100000);
            _httpClient = new HttpClient(ClientHandler);
            SetExpectContinueHeaderToFalse();
        }

        public void SetCookieContainer(CookieContainer cookies)
        {
            ClientHandler.CookieContainer = cookies;
            ClientHandler.UseCookies = true;
        }

        public void AddCookie(string key, string value, string host)
        {
            if (ClientHandler.CookieContainer != null)
            {
                ClientHandler.CookieContainer.Add(new Cookie(key, value, "/", host));
            }
        }

        public string GetCookie(string key)
        {
            if (ClientHandler.CookieContainer != null)
            {
                var cookies = ClientHandler.CookieContainer.GetAllCookies();
                foreach (Cookie c in cookies)
                {
                    if (c.Name.ToLower() == key.ToLower())
                    {
                        return c.Value;
                    }
                }
            }
            return "";
        }

        private void SetExpectContinueHeaderToFalse()
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
        }

        public void ClearRequestHeaders()
        {
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public void AddConnectionKeepAliveHeader()
        {
            _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

        public void AddRequestHeader(string name, string value)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
            }
            catch
            {

            }

        }

        public void RemoveRequestHeader(string name)
        {
            _httpClient.DefaultRequestHeaders.Remove(name);
        }

        public void SetReferrerUri(string value)
        {
            _httpClient.DefaultRequestHeaders.Referrer = new Uri(value);
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent)
        {
            return await _httpClient.PostAsync(requestUri, httpContent).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return await _httpClient.DeleteAsync(requestUri);
        }

        public async Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent httpContent)
        {
            return await _httpClient.PostAsync(requestUri, httpContent).ConfigureAwait(false);
        }

        public async Task<byte[]> GetByteArrayAsync(string requestUri)
        {
            return await _httpClient.GetByteArrayAsync(requestUri).ConfigureAwait(false);
        }

        public List<Cookie> GetCookies()
        {
            var ret = new List<Cookie>();
            var cookies = ClientHandler.CookieContainer.GetAllCookies();
            foreach (Cookie c in cookies)
            {
                ret.Add(c);
            }
            return ret;
        }


        private bool _addedCommonHeaders = false;

        public void AddCommonHeaders()
        {
            if (!_addedCommonHeaders)
            {
                AddAcceptEncodingHeader();
                AddAcceptLanguageHeader();
                AddAcceptHeader("application/json");
                AddRequestHeader(HttpHeaders.ContentType, "application/json");
                AddRequestHeader("X-Requested-With", "com.ea.gp.fifaultimate");
                AddRequestHeader("X-UT-Embed-Error", "true");
                AddUserAgent();
                AddConnectionKeepAliveHeader();
                _addedCommonHeaders = true;
            }

        }

        public void AddUserAgent()
        {
            AddRequestHeader(HttpHeaders.UserAgent, UserAgent);
        }

        public void AddAcceptHeader(string value)
        {
            AddRequestHeader(HttpHeaders.Accept, value);
        }

        public void AddReferrerHeader(string value)
        {
            SetReferrerUri(value);
        }

        public void AddAcceptEncodingHeader()
        {
            AddRequestHeader(HttpHeaders.AcceptEncoding, "gzip, deflate");
        }

        public void AddAcceptLanguageHeader()
        {
            AddRequestHeader(HttpHeaders.AcceptLanguage, "en-US,en;q=0.8");
        }
    }
}
