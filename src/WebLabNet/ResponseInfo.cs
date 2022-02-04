using System.Net.Http;

namespace WebLabNet;

/// <summary>
/// Represents the result of a query.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="Request">The request.</param>
/// <param name="Success">Indicates whether or not the request was succesful.</param>
/// <param name="HttpResponse">The raw http response.</param>
/// <param name="Data">The found data object.</param>
public record ResponseInfo<TRequest, TResponse>(TRequest Request, bool Success, HttpResponseMessage? HttpResponse, TResponse? Data)
{
    /// <inheritdoc/>
    public override string ToString()
        => $"ResponseInfo<{typeof(TRequest).Name}, {typeof(TResponse).Name}>(...)";
}
