using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebLabNet.Internal;

namespace WebLabNet;

/// <summary>
/// Represents the base for all responses.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract record ResponseData<TRequest, TResponse>(string ApiVersion, string DataTimestamp)
    where TRequest : RequestInfo<TRequest, TResponse>
    where TResponse : ResponseData<TRequest, TResponse>
{
    /// <summary>
    /// Gets the <see cref="DataTimestamp"/> as a date.
    /// </summary>
    [JsonIgnore]
    public DateTime DataTimestampDate { get; } = DateHelper.Parse(DataTimestamp);

    /// <summary>
    /// Gets the response info associated with this object.
    /// </summary>
    [JsonIgnore]
    public ResponseInfo<TRequest, TResponse> ResponseInfo { get; internal set; } = null!;
}

/// <summary>
/// Represents a response for a push grade request with netid.
/// </summary>
public record PushGradeNetidResponseData(string ApiVersion, string DataTimestamp) : ResponseData<PushGradeNetidRequestInfo, PushGradeNetidResponseData>(ApiVersion, DataTimestamp)
{
}

/// <summary>
/// Represents a response for a push grade request with WebLab ID.
/// </summary>
public record PushGradeWebLabIdResponseData(string ApiVersion, string DataTimestamp) : ResponseData<PushGradeWebLabIdRequestInfo, PushGradeWebLabIdResponseData>(ApiVersion, DataTimestamp)
{
}

/// <summary>
/// Represents an object to represent the spec test task data for a submission.
/// </summary>
/// <param name="RanAt">The timestamp when the last spec tests were run.</param>
/// <param name="BuildStatus">The build status.</param>
/// <param name="NumPassedTests">The number of spec tests that passed.</param>
/// <param name="NumFailedTests">The number of spec tests that failed.</param>
/// <param name="NumTotalTests">The total number of spec tests.</param>
public record TaskForGrade(string RanAt, string BuildStatus, int NumPassedTests, int NumFailedTests, int NumTotalTests)
{
    /// <summary>
    /// Gets the <see cref="RanAt"/> as a date.
    /// </summary>
    [JsonIgnore]
    public DateTime RanAtDate { get; } = DateHelper.Parse(RanAt);
}

/// <summary>
/// Represents the response to a SUBMISSION request.
/// </summary>
public record SubmissionResponseData(string ApiVersion, string DataTimestamp, int Student, bool StudentForGrade, double Grade, string LastSavedAt, string AssignmentType, string Deadline, string SolutionCode, string UserTestCode, TaskForGrade? TaskForGrade) : ResponseData<SubmissionRequestInfo, SubmissionResponseData>(ApiVersion, DataTimestamp)
{
    /// <summary>
    /// Gets the <see cref="LastSavedAt"/> as a date.
    /// </summary>
    [JsonIgnore]
    public DateTime LastSavedAtDate { get; } = DateHelper.Parse(LastSavedAt);

    /// <summary>
    /// Gets the <see cref="Deadline"/> as a date.
    /// </summary>
    [JsonIgnore]
    public DateTime DeadLineDate { get; } = DateHelper.Parse(Deadline);

    /// <summary>
    /// Gets the assignment ID.
    /// </summary>
    [JsonIgnore]
    public int Assignment => ResponseInfo.Request.Assignment;

    /// <summary>
    /// Pushes a grade for this submission.
    /// </summary>
    /// <param name="grade">The grade to push.</param>
    /// <param name="comment">The comment to add.</param>
    /// <param name="saveTime">The time of the submission.</param>
    /// <param name="keepNLastComments">The number of comments to keep.</param>
    /// <param name="apiKey">The API key to use.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The response.</returns>
    public Task<ResponseInfo<PushGradeWebLabIdRequestInfo, PushGradeWebLabIdResponseData>> PushGradeAsync(double grade, string comment, DateTime saveTime, int keepNLastComments, string apiKey, string? apiSecret = null)
        => ResponseInfo.Request.Api.PushGradeAsync(Student, grade, comment, saveTime, keepNLastComments, apiKey, apiSecret);

    /// <summary>
    /// Pushes a grade for this submission.
    /// </summary>
    /// <param name="grade">The grade to push.</param>
    /// <param name="comment">The comment to add.</param>
    /// <param name="keepNLastComments">The number of comments to keep.</param>
    /// <param name="apiKey">The API key to use.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The response.</returns>
    public Task<ResponseInfo<PushGradeWebLabIdRequestInfo, PushGradeWebLabIdResponseData>> PushGradeAsync(double grade, string comment, int keepNLastComments, string apiKey, string? apiSecret = null)
        => ResponseInfo.Request.Api.PushGradeAsync(Student, grade, comment, LastSavedAtDate, keepNLastComments, apiKey, apiSecret);

    /// <summary>
    /// Pushes a grade for this submission.
    /// </summary>
    /// <param name="grade">The grade to push.</param>
    /// <param name="comment">The comment to add.</param>
    /// <param name="saveTime">The time of the submission.</param>
    /// <param name="apiKey">The API key to use.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The response.</returns>
    public Task<ResponseInfo<PushGradeWebLabIdRequestInfo, PushGradeWebLabIdResponseData>> PushGradeAsync(double grade, string comment, DateTime saveTime, string apiKey, string? apiSecret = null)
        => ResponseInfo.Request.Api.PushGradeAsync(Student, grade, comment, saveTime, -1, apiKey, apiSecret);

    /// <summary>
    /// Pushes a grade for this submission.
    /// </summary>
    /// <param name="grade">The grade to push.</param>
    /// <param name="comment">The comment to add.</param>
    /// <param name="apiKey">The API key to use.</param>
    /// <param name="apiSecret">The API secret.</param>
    /// <returns>The response.</returns>
    public Task<ResponseInfo<PushGradeWebLabIdRequestInfo, PushGradeWebLabIdResponseData>> PushGradeAsync(double grade, string comment, string apiKey, string? apiSecret = null)
        => ResponseInfo.Request.Api.PushGradeAsync(Student, grade, comment, LastSavedAtDate, -1, apiKey, apiSecret);
}

/// <summary>
/// Represents a basic submission info overview.
/// </summary>
public record SubmissionInfo(int Student, bool StudentForGrade, double Grade, string LastSavedAt)
{
    /// <summary>
    /// Gets the <see cref="LastSavedAt"/> as a date.
    /// </summary>
    [JsonIgnore]
    public DateTime LastSavedAtDate { get; } = DateHelper.Parse(LastSavedAt);

    /// <summary>
    /// Gets the submission response data parent.
    /// </summary>
    [JsonIgnore]
    public SubmissionsResponseData Parent { get; internal set; } = null!;

    /// <summary>
    /// Expands the submission info to a full submission request.
    /// </summary>
    /// <returns>The response of the request.</returns>
    public Task<ResponseInfo<SubmissionRequestInfo, SubmissionResponseData>> GetSubmissionAsync()
    {
        SubmissionsRequestInfo request = Parent.ResponseInfo.Request;
        return request.Api.GetSubmissionAsync(request.Assignment, Student, request.ApiKey, request.ApiSecret);
    }
}

/// <summary>
/// Represents the response to a submissions request.
/// </summary>
public record SubmissionsResponseData(string ApiVersion, string DataTimestamp) : ResponseData<SubmissionsRequestInfo, SubmissionsResponseData>(ApiVersion, DataTimestamp)
{
    private IEnumerable<SubmissionInfo> submissions = null!;

    /// <summary>
    /// Gets or sets the submissions.
    /// </summary>
    public IEnumerable<SubmissionInfo> Submissions
    {
        get => submissions;
        set
        {
            submissions = value;
            foreach (var submission in submissions)
            {
                submission.Parent = this;
            }
        }
    }
}
