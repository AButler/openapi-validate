using System.Net;

namespace OpenApiValidate;

public static class StatusCodeListExtensions
{
    public static bool ContainsStatusCode(
        this StatusCodeList statusCodeList,
        HttpStatusCode statusCode
    )
    {
        return statusCodeList.ContainsStatusCode((int)statusCode);
    }

    public static void Add(this StatusCodeList statusCodeList, HttpStatusCode statusCode)
    {
        statusCodeList.Add((int)statusCode);
    }

    public static void AddRange(
        this StatusCodeList statusCodeList,
        params IEnumerable<HttpStatusCode> statusCodes
    )
    {
        statusCodeList.AddRange(statusCodes.Cast<int>());
    }

    public static bool Remove(this StatusCodeList statusCodeList, HttpStatusCode statusCode)
    {
        return statusCodeList.Remove((int)statusCode);
    }
}
