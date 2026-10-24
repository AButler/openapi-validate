using System.Net;

namespace OpenApiValidate;

public static class StatusCodeListExtensions
{
    extension(StatusCodeList statusCodeList)
    {
        public bool ContainsStatusCode(HttpStatusCode statusCode)
        {
            return statusCodeList.ContainsStatusCode((int)statusCode);
        }

        public void Add(HttpStatusCode statusCode)
        {
            statusCodeList.Add((int)statusCode);
        }

        public void AddRange(params HttpStatusCode[] statusCodes)
        {
            statusCodeList.AddRange(statusCodes.Cast<int>().ToArray());
        }

        public bool Remove(HttpStatusCode statusCode)
        {
            return statusCodeList.Remove((int)statusCode);
        }
    }
}
