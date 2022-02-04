using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using WebLabNet.Internal;

namespace WebLabNet;

/// <summary>
/// Base for all request information.
/// </summary>
/// <param name="Api">The API used for the request.</param>
/// <param name="ApiKey">The API key used.</param>
/// <param name="ApiSecret">The API secret used.</param>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public abstract record RequestInfo<TRequest, TResponse>(WebLab Api, string ApiKey, string? ApiSecret)
    where TRequest : RequestInfo<TRequest, TResponse>
    where TResponse : ResponseData<TRequest, TResponse>
{
    /// <summary>
    /// Empty dictionary.
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, string> EmptyDict = new Dictionary<string, string>();

    /// <summary>
    /// Gets the url for the request.
    /// </summary>
    public abstract string Url { get; }

    /// <summary>
    /// Gets a value indicating whether the request body is used or not.
    /// </summary>
    public abstract bool UseBody { get; }

    /// <summary>
    /// Gets a value indicating whether hmac should be used.
    /// </summary>
    public bool UseHmac => !string.IsNullOrWhiteSpace(ApiSecret);

    /// <summary>
    /// Gets the request body.
    /// </summary>
    public abstract IReadOnlyDictionary<string, string> Body { get; }

    /// <summary>
    /// Gets the query parts.
    /// </summary>
    public abstract IReadOnlyDictionary<string, string> QueryParts { get; }

    /// <summary>
    /// Gets the request type.
    /// </summary>
    public abstract RequestType RequestType { get; }

    /// <summary>
    /// Sends this request asynchronously.
    /// </summary>
    /// <returns>The response of the request.</returns>
    public Task<ResponseInfo<TRequest, TResponse>> SendAsync()
        => Api.SendRequestAsync(this);
}

/// <summary>
/// Base for all push grade request information.
/// </summary>
/// <param name="Api">The API used for the request.</param>
/// <param name="ApiKey">The API key used.</param>
/// <param name="ApiSecret">The API secret used.</param>
/// <param name="Grade">The grade to push.</param>
/// <param name="Comment">The feedback to give.</param>
/// <param name="SaveDate">The date when the submission was saved.</param>
/// <param name="KeepLastNComments">The number of feedback entries to maintain.</param>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public abstract record PushGradeCommonRequestInfo<TRequest, TResponse>(WebLab Api, string ApiKey, string? ApiSecret, double Grade, string Comment, DateTime SaveDate, int KeepLastNComments)
    : RequestInfo<TRequest, TResponse>(Api, ApiKey, ApiSecret)
        where TRequest : RequestInfo<TRequest, TResponse>
        where TResponse : ResponseData<TRequest, TResponse>
{
    /// <inheritdoc/>
    public override string Url => $"{Api.BaseUrl}/pushGrade/{ApiKey}";

    /// <inheritdoc/>
    public override bool UseBody => true;

    /// <inheritdoc/>
    public override RequestType RequestType => RequestType.Post;

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, string> QueryParts => EmptyDict;

    /// <summary>
    /// Builds a dictionary.
    /// </summary>
    /// <param name="identifyKey">The name of the field used for student identification.</param>
    /// <param name="identifyValue">The value of the student indentification field.</param>
    /// <returns>The created body.</returns>
    protected IReadOnlyDictionary<string, string> BuildBody(string identifyKey, string identifyValue)
        => new Dictionary<string, string>()
        {
            { identifyKey, identifyValue },
            { "grade", Grade.ToString(CultureInfo.InvariantCulture) },
            { "comment", Comment },
            { "saveDate", SaveDate.ToEpochMilliseconds().ToString(CultureInfo.InvariantCulture) + "l" },
            { "keepLastNComments", KeepLastNComments.ToString(CultureInfo.InvariantCulture) },
        };
}

/// <summary>
/// Request information for pushing grades to netids.
/// </summary>
/// <param name="Api">The API used for the request.</param>
/// <param name="ApiKey">The API key used.</param>
/// <param name="ApiSecret">The API secret used.</param>
/// <param name="Grade">The grade to push.</param>
/// <param name="Comment">The feedback to give.</param>
/// <param name="SaveDate">The date when the submission was saved.</param>
/// <param name="KeepLastNComments">The number of feedback entries to maintain.</param>
/// <param name="Netid">The netid/username/student number to push a grade to.</param>
public record PushGradeNetidRequestInfo(WebLab Api, string ApiKey, string? ApiSecret, double Grade, string Comment, DateTime SaveDate, int KeepLastNComments, string Netid)
    : PushGradeCommonRequestInfo<PushGradeNetidRequestInfo, PushGradeNetidResponseData>(Api, ApiKey, ApiSecret, Grade, Comment, SaveDate, KeepLastNComments)
{
    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, string> Body => BuildBody("netid", Netid);
}

/// <summary>
/// Request information for pushing grades to netids.
/// </summary>
/// <param name="Api">The API used for the request.</param>
/// <param name="ApiKey">The API key used.</param>
/// <param name="ApiSecret">The API secret used.</param>
/// <param name="Grade">The grade to push.</param>
/// <param name="Comment">The feedback to give.</param>
/// <param name="SaveDate">The date when the submission was saved.</param>
/// <param name="KeepLastNComments">The number of feedback entries to maintain.</param>
/// <param name="WebLabId">The weblab id to push a grade to.</param>
public record PushGradeWebLabIdRequestInfo(WebLab Api, string ApiKey, string? ApiSecret, double Grade, string Comment, DateTime SaveDate, int KeepLastNComments, int WebLabId)
    : PushGradeCommonRequestInfo<PushGradeWebLabIdRequestInfo, PushGradeWebLabIdResponseData>(Api, ApiKey, ApiSecret, Grade, Comment, SaveDate, KeepLastNComments)
{
    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, string> Body => BuildBody("student", WebLabId.ToString(CultureInfo.InvariantCulture));
}

/// <summary>
/// Base for all data request information.
/// </summary>
/// <param name="Api">The API used for the request.</param>
/// <param name="ApiKey">The API key used.</param>
/// <param name="ApiSecret">The API secret used.</param>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public abstract record DataRequestInfo<TRequest, TResponse>(WebLab Api, string ApiKey, string? ApiSecret)
    : RequestInfo<TRequest, TResponse>(Api, ApiKey, ApiSecret)
        where TRequest : RequestInfo<TRequest, TResponse>
        where TResponse : ResponseData<TRequest, TResponse>
{
    /// <inheritdoc/>
    public override bool UseBody => false;

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, string> Body => EmptyDict;

    /// <inheritdoc/>
    public override RequestType RequestType => RequestType.Get;

    /// <summary>
    /// Gets the extended base url for requests.
    /// </summary>
    public string BaseUrl => $"{Api.BaseUrl}/api/V0/GET";

    /// <inheritdoc/>
    public override string Url => $"{BaseUrl}/{DataType}";

    /// <summary>
    /// Gets the data type part of the url.
    /// </summary>
    public abstract string DataType { get; }

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, string> QueryParts
    {
        get
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var entry in AdditionalQueryParts)
            {
                result[entry.Key] = entry.Value;
            }

            result["apikey"] = ApiKey;
            result["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

            return result;
        }
    }

    /// <summary>
    /// Gets the additional query parts (excluding api key).
    /// </summary>
    public abstract IReadOnlyDictionary<string, string> AdditionalQueryParts { get; }
}

/// <summary>
/// Used for requesting all submissions.
/// </summary>
/// <param name="Api">The API used for the request.</param>
/// <param name="ApiKey">The API key used.</param>
/// <param name="ApiSecret">The API secret used.</param>
/// <param name="Assignment">The assignment ID.</param>
public record SubmissionsRequestInfo(WebLab Api, string ApiKey, string? ApiSecret, int Assignment)
    : DataRequestInfo<SubmissionsRequestInfo, SubmissionsResponseData>(Api, ApiKey, ApiSecret)
{
    /// <inheritdoc/>
    public override string DataType => "SUBMISSIONS";

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, string> AdditionalQueryParts { get; } = new Dictionary<string, string>()
    {
        { "assignment", Assignment.ToString(CultureInfo.InvariantCulture) },
    };
}

/// <summary>
/// Used for requesting all submissions.
/// </summary>
/// <param name="Api">The API used for the request.</param>
/// <param name="ApiKey">The API key used.</param>
/// <param name="ApiSecret">The API secret used.</param>
/// <param name="Student">The student WebLab ID.</param>
/// <param name="Assignment">The assignment ID.</param>
public record SubmissionRequestInfo(WebLab Api, string ApiKey, string? ApiSecret, int Student, int Assignment)
    : DataRequestInfo<SubmissionRequestInfo, SubmissionResponseData>(Api, ApiKey, ApiSecret)
{
    /// <inheritdoc/>
    public override string DataType => "SUBMISSION";

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, string> AdditionalQueryParts { get; } = new Dictionary<string, string>()
    {
        { "student", Student.ToString(CultureInfo.InvariantCulture) },
        { "assignment", Assignment.ToString(CultureInfo.InvariantCulture) },
    };
}