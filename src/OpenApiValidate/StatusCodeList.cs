namespace OpenApiValidate;

public class StatusCodeList
{
    private readonly List<int> _statusCodes = [];

    public bool ShouldValidateAll => _statusCodes.Count == 0;

    public StatusCodeList() { }

    public StatusCodeList(IEnumerable<int> statusCodes)
    {
        _statusCodes = new List<int>(statusCodes);
    }

    public bool ContainsStatusCode(int statusCode)
    {
        return ShouldValidateAll || _statusCodes.Contains(statusCode);
    }

    public void Add(int statusCode)
    {
        _statusCodes.Add(statusCode);
    }

    public void AddRange(params int[] statusCodes)
    {
        _statusCodes.AddRange(statusCodes);
    }

    public bool Remove(int statusCode)
    {
        return _statusCodes.Remove(statusCode);
    }

    public void Clear()
    {
        _statusCodes.Clear();
    }

    public IEnumerator<int> GetEnumerator()
    {
        return _statusCodes.GetEnumerator();
    }

    public static StatusCodeList All => new();

    public static StatusCodeList SuccessOnly
    {
        get
        {
            var successStatusCodes = Enumerable.Range(200, 99).ToArray();
            var list = new StatusCodeList();
            list.AddRange(successStatusCodes);
            return list;
        }
    }
}
