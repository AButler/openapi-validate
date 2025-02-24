using System.Collections;
using System.Net;

namespace OpenApiValidate;

public class StatusCodeList : IEnumerable<int>
{
    private readonly List<int> _statusCodes = [];

    public bool ShouldValidateAll => _statusCodes.Count == 0;

    public bool ContainsStatusCode(int statusCode)
    {
        return ShouldValidateAll || _statusCodes.Contains(statusCode);
    }

    public bool ContainsStatusCode(HttpStatusCode statusCode) =>
        ContainsStatusCode((int)statusCode);

    public void Add(int statusCode)
    {
        _statusCodes.Add(statusCode);
    }

    public void Add(HttpStatusCode statusCode) => Add((int)statusCode);

    public void AddRange(params IEnumerable<int> statusCodes)
    {
        _statusCodes.AddRange(statusCodes);
    }

    public void AddRange(params IEnumerable<HttpStatusCode> statusCodes)
    {
        AddRange(statusCodes.Cast<int>());
    }

    public bool Remove(int statusCode)
    {
        return _statusCodes.Remove(statusCode);
    }

    public bool Remove(HttpStatusCode statusCode) => Remove((int)statusCode);

    public void Clear()
    {
        _statusCodes.Clear();
    }

    public IEnumerator<int> GetEnumerator()
    {
        return _statusCodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_statusCodes).GetEnumerator();
    }

    public static StatusCodeList All => new();
    public static StatusCodeList SuccessOnly
    {
        get
        {
            var successStatusCodes = Enumerable.Range(200, 99);
            var list = new StatusCodeList();
            list.AddRange(successStatusCodes);
            return list;
        }
    }
}
