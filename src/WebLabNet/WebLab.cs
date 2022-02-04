using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebLabNet.Internal;

namespace WebLabNet;

/// <summary>
/// Used for interacting with WebLab.
/// </summary>
public class WebLab : IDisposable
{
    private readonly HttpClient client;
    private readonly HttpClientHandler? handler;
    private readonly bool ownClient;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebLab"/> class.
    /// </summary>
    /// <param name="client">The HTTP client used for making requests.</param>
    public WebLab(HttpClient client)
        => this.client = client;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebLab"/> class.
    /// </summary>
    public WebLab()
    {
        handler = new HttpClientHandler { UseCookies = false };
        client = new HttpClient(handler);
        ownClient = true;
    }

    /// <summary>
    /// Gets or sets the base url.
    /// </summary>
    public string BaseUrl { get; set; } = "https://weblab.tudelft.nl";

    /// <summary>
    /// Sends the given request.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request info.</param>
    /// <returns>The response info.</returns>
    public async Task<ResponseInfo<TRequest, TResponse>> SendRequestAsync<TRequest, TResponse>(RequestInfo<TRequest, TResponse> request)
        where TRequest : RequestInfo<TRequest, TResponse>
        where TResponse : ResponseData<TRequest, TResponse>
    {
        string url = request.Url;
        string queryString = string.Join("&", request.QueryParts.Select(x => $"{x.Key}={x.Value}"));

        if (request.QueryParts.Any() || request.UseHmac)
        {
            url += "?";
            url += queryString;
        }

        if (request.UseHmac)
        {
            if (!string.IsNullOrEmpty(queryString))
            {
                url += "&";
            }

            string hash = HmacHelper.Compute(queryString, request.ApiSecret!);
            url += $"signature={hash}";
        }

        HttpResponseMessage? httpResponse;
        if (request.RequestType == RequestType.Post)
        {
            string json = JsonConvert.SerializeObject(request.Body);
            using HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            httpResponse = await client.PostAsync(url, content).ConfigureAwait(false);
        }
        else
        {
            httpResponse = await client.GetAsync(url).ConfigureAwait(false);
        }

        if (httpResponse is null || !httpResponse.IsSuccessStatusCode)
        {
            return new ResponseInfo<TRequest, TResponse>((TRequest)request, false, httpResponse, null);
        }

        string body = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        TResponse? data;
#pragma warning disable
        try
        {
            data = string.IsNullOrWhiteSpace(body) ? null : JsonConvert.DeserializeObject<TResponse>(body);
        }
        catch
        {
            data = null;
        }
#pragma warning restore

        if (data is null)
        {
            return new ResponseInfo<TRequest, TResponse>((TRequest)request, false, httpResponse, null);
        }

        ResponseInfo<TRequest, TResponse> result = new ResponseInfo<TRequest, TResponse>((TRequest)request, true, httpResponse, data);
        data.ResponseInfo = result;
        return result;
    }

    /// <summary>
    /// Pushes the grade asynchronous.
    /// </summary>
    /// <param name="netId">The net identifier.</param>
    /// <param name="grade">The grade.</param>
    /// <param name="comment">The comment.</param>
    /// <param name="saveDate">The save date of the submission.</param>
    /// <param name="keepLastNComments">The number of comments to keep, 0 keeps all comments.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The http response of pushing the grade.</returns>
    public Task<ResponseInfo<PushGradeNetidRequestInfo, PushGradeNetidResponseData>> PushGradeAsync(string netId, double grade, string comment, DateTime saveDate, int keepLastNComments, string apiKey, string? apiSecret = null)
        => SendRequestAsync(new PushGradeNetidRequestInfo(this, apiKey, apiSecret, grade, comment, saveDate, keepLastNComments, netId));

    /// <summary>
    /// Pushes the grade asynchronous.
    /// </summary>
    /// <param name="netId">The net identifier.</param>
    /// <param name="grade">The grade.</param>
    /// <param name="comment">The comment.</param>
    /// <param name="saveDate">The save date of the submission.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The http response of pushing the grade.</returns>
    public Task<ResponseInfo<PushGradeNetidRequestInfo, PushGradeNetidResponseData>> PushGradeAsync(string netId, double grade, string comment, DateTime saveDate, string apiKey, string? apiSecret = null)
        => PushGradeAsync(netId, grade, comment, saveDate, -1, apiKey, apiSecret);

    /// <summary>
    /// Pushes the grade asynchronous.
    /// </summary>
    /// <param name="student">The WebLab ID of the student.</param>
    /// <param name="grade">The grade.</param>
    /// <param name="comment">The comment.</param>
    /// <param name="saveDate">The save date of the submission.</param>
    /// <param name="keepLastNComments">The number of comments to keep, 0 keeps all comments.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The http response of pushing the grade.</returns>
    public Task<ResponseInfo<PushGradeWebLabIdRequestInfo, PushGradeWebLabIdResponseData>> PushGradeAsync(int student, double grade, string comment, DateTime saveDate, int keepLastNComments, string apiKey, string? apiSecret = null)
        => SendRequestAsync(new PushGradeWebLabIdRequestInfo(this, apiKey, apiSecret, grade, comment, saveDate, keepLastNComments, student));

    /// <summary>
    /// Pushes the grade asynchronous.
    /// </summary>
    /// <param name="student">The WebLab ID of the student.</param>
    /// <param name="grade">The grade.</param>
    /// <param name="comment">The comment.</param>
    /// <param name="saveDate">The save date of the submission.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The http response of pushing the grade.</returns>
    public Task<ResponseInfo<PushGradeWebLabIdRequestInfo, PushGradeWebLabIdResponseData>> PushGradeAsync(int student, double grade, string comment, DateTime saveDate, string apiKey, string? apiSecret = null)
        => PushGradeAsync(student, grade, comment, saveDate, -1, apiKey, apiSecret);

    /// <summary>
    /// Gets the submissions for the given assignment.
    /// </summary>
    /// <param name="assignmentId">The assignment ID to request the submissions for.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The response of requesting the submissions.</returns>
    public Task<ResponseInfo<SubmissionsRequestInfo, SubmissionsResponseData>> GetSubmissionsAsync(int assignmentId, string apiKey, string? apiSecret = null)
        => SendRequestAsync(new SubmissionsRequestInfo(this, apiKey, apiSecret, assignmentId));

    /// <summary>
    /// Gets the submissions for the given assignment.
    /// </summary>
    /// <param name="assignmentId">The assignment ID to request the submissions for.</param>
    /// <param name="student">The WebLab ID of the student.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The response of requesting the submissions.</returns>
    public Task<ResponseInfo<SubmissionRequestInfo, SubmissionResponseData>> GetSubmissionAsync(int assignmentId, int student, string apiKey, string? apiSecret = null)
        => SendRequestAsync(new SubmissionRequestInfo(this, apiKey, apiSecret, student, assignmentId));

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed && disposing && ownClient)
        {
            handler?.Dispose();
            client.Dispose();
            isDisposed = true;
        }
    }
}
