using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace UniqueUrlSlugEditor
{
    public class UrlChecker
    {
        /// <summary>
        /// HTTP client timeout.
        /// </summary>
        public TimeSpan Timeout { get; protected set; }

        /// <summary>
        /// Name of the GUID response header.
        /// </summary>
        public string ResponseHeaderName { get; protected set; }

        /// <summary>
        /// Instantiates a new UrlChecker.
        /// </summary>
        /// <param name="timeoutSeconds">HTTP client timeout.</param>
        /// <param name="responseHeaderName">Name of the GUID response header.</param>
        public UrlChecker(int timeoutSeconds, string responseHeaderName)
        {
            Timeout = new TimeSpan(0, 0, timeoutSeconds);

            ResponseHeaderName = !string.IsNullOrEmpty(responseHeaderName)
                ? responseHeaderName 
                : throw new ArgumentNullException(nameof(responseHeaderName));
        }

        /// <summary>
        /// Pings a URL address and returns a value of the <see cref="ResponseHeaderName"/> header. 
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>Bool saying if the URL returns content, together with the value of the <see cref="ResponseHeaderName"/> header.</returns>
        public async Task<(bool IsSuccess, Guid LastHeaderValue)> GetSuccessAndResponseHeaderFromUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            return await MakeRequestAsync(new Uri(url));
        }

        /// <summary>
        /// Pings the <paramref name="uri"/>, recursively invokes itself to follow redirects, and retrieves the GUID response header.
        /// </summary>
        /// <param name="uri">URI to ping.</param>
        /// <param name="lastHeaderValue">GUID response header value from the previous iteration.</param>
        /// <returns></returns>
        protected async Task<(bool IsSuccess, Guid LastHeaderValue)> MakeRequestAsync(Uri uri, string lastHeaderValue = null)
        {
            // Get response w/ HttpClient.
            // You may want to use HttpClientFactory instead 
            // (https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests).
            using (var client = new HttpClient(new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    AllowAutoRedirect = false
                })
                    {
                        Timeout = Timeout
                    })
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = uri,
                    Method = HttpMethod.Get
                };

                var response = await client.SendAsync(request);

                // Get the GUID from the response header.
                var retrievedHeaderValue = response?.Headers?
                    .FirstOrDefault(header => header.Key.Equals(ResponseHeaderName, StringComparison.OrdinalIgnoreCase))
                    .Value?
                    .FirstOrDefault();

                var statusCode = (int)response.StatusCode;

                if (statusCode >= 300 && statusCode <= 399)
                {
                    var redirectUri = response.Headers.Location;

                    if (!redirectUri.IsAbsoluteUri)
                    {
                        redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                    }

                    // Recursively ping the redirect URI.
                    // You may also want to capture the first header value, depending on your use case.
                    return await MakeRequestAsync(redirectUri, retrievedHeaderValue ?? lastHeaderValue);
                }

                return (response.IsSuccessStatusCode, 
                    Guid.TryParse(retrievedHeaderValue ?? lastHeaderValue, out Guid output)
                        ? output
                        : Guid.Empty);
            }
        }
    }
}