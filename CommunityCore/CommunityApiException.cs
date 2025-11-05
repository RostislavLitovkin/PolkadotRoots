using System.Net;

namespace CommunityCore
{
    public sealed class CommunityApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ResponseBody { get; }

        public CommunityApiException(string message, HttpStatusCode statusCode, string? responseBody = null, Exception? inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public override string ToString() =>
            $"{base.ToString()} | HTTP {(int)StatusCode} {StatusCode} | Body: {ResponseBody}";

    }
}
