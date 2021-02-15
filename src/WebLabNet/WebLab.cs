using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebLabNet
{
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
        /// Gets or sets the cookie. Note that this is only used when the HttpClient instance is managed by this class.
        /// </summary>
        public string? Cookie { get; set; }

        /// <summary>
        /// Pushes the grade asynchronous.
        /// </summary>
        /// <param name="netId">The net identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="saveDate">The save date.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The http response of pushing the grade.</returns>
        public Task<HttpResponseMessage> PushGradeAsync(string netId, double grade, string comment, long saveDate, string apiKey)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
                {
                    { "netid", netId },
                    { "grade", grade.ToString(CultureInfo.InvariantCulture) },
                    { "comment", comment },
                    { "saveDate", saveDate.ToString(CultureInfo.InvariantCulture) },
                };

            string json = JsonConvert.SerializeObject(values);
            using HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            return client.PostAsync($"https://weblab.tudelft.nl/pushGrade/{apiKey}", content);
        }

        /// <summary>
        /// Pushes the grade asynchronous.
        /// </summary>
        /// <param name="netId">The net identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="saveDate">The save date of the submission.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The http response of pushing the grade.</returns>
        public Task<HttpResponseMessage> PushGradeAsync(string netId, double grade, string comment, DateTime saveDate, string apiKey)
        {
            long epoch = (long)(saveDate - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return PushGradeAsync(netId, grade, comment, epoch, apiKey);
        }

        /// <summary>
        /// Pushes the grade asynchronous.
        /// </summary>
        /// <param name="student">The student.</param>
        /// <param name="grade">The grade.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="saveDate">The save date of the submission.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The http response of pushing the grade.</returns>
        public Task<HttpResponseMessage> PushGradeAsync(Student student, double grade, string comment, DateTime saveDate, string apiKey)
        {
            if (student is null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            return PushGradeAsync(student.NetId, grade, comment, saveDate, apiKey);
        }

        /// <summary>
        /// Pushes the grade asynchronous.
        /// </summary>
        /// <param name="student">The student.</param>
        /// <param name="grade">The grade.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="saveDate">The save date of the submission.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The http response of pushing the grade.</returns>
        public Task<HttpResponseMessage> PushGradeAsync(Student student, double grade, string comment, long saveDate, string apiKey)
        {
            if (student is null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            return PushGradeAsync(student.NetId, grade, comment, saveDate, apiKey);
        }

        /// <summary>
        /// Gets the submissions.
        /// </summary>
        /// <param name="assignmentId">The id used by weblab for the assignment.</param>
        /// <returns>The submissions.</returns>
        public Task<IEnumerable<SubmissionInfo>> GetSubmissions(int assignmentId)
            => GetSubmissions(assignmentId.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets the submissions.
        /// </summary>
        /// <param name="assignmentId">The id used by weblab for the assignment.</param>
        /// <returns>The submissions.</returns>
        public async Task<IEnumerable<SubmissionInfo>> GetSubmissions(string assignmentId)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://weblab.tudelft.nl/assignment/{assignmentId}/submissions");
            request.Headers.Add("Cookie", Cookie);
            using HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return await ParserHelper.ParseSubmissions(result, assignmentId).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the submitted code at the given url.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The submitted code.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054", Justification = "We want to maintain the url as a string.")]
        public async Task<Submission> GetSubmission(string url)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", Cookie);
            using HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return await ParserHelper.ParseSubmission(result).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the submitted code at the given url.
        /// </summary>
        /// <param name="submission">The submission to retrieve.</param>
        /// <returns>The submitted code.</returns>
        public Task<Submission> GetSubmission(SubmissionInfo submission)
        {
            if (submission is null)
            {
                throw new ArgumentNullException(nameof(submission));
            }

            return GetSubmission(submission.Url);
        }

        /// <summary>
        /// Gets the submitted code at the given url.
        /// </summary>
        /// <param name="assignmentId">The ID of the assingment.</param>
        /// <param name="weblabId">The ID of the weblab user.</param>
        /// <returns>The submitted code.</returns>
        public Task<Submission> GetSubmission(string assignmentId, string weblabId)
            => GetSubmission($"https://weblab.tudelft.nl/x/y/assignment/{assignmentId}/submission/{weblabId}");

        /// <summary>
        /// Gets the submitted code at the given url.
        /// </summary>
        /// <param name="assignmentId">The ID of the assingment.</param>
        /// <param name="weblabId">The ID of the weblab user.</param>
        /// <returns>The submitted code.</returns>
        public Task<Submission> GetSubmission(int assignmentId, int weblabId)
            => GetSubmission(assignmentId.ToString(CultureInfo.InvariantCulture), weblabId.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets the submitted code at the given url.
        /// </summary>
        /// <param name="assignmentId">The ID of the assingment.</param>
        /// <param name="weblabId">The ID of the weblab user.</param>
        /// <returns>The submitted code.</returns>
        public Task<Submission> GetSubmission(string assignmentId, int weblabId)
            => GetSubmission(assignmentId, weblabId.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets the submitted code at the given url.
        /// </summary>
        /// <param name="assignmentId">The ID of the assingment.</param>
        /// <param name="weblabId">The ID of the weblab user.</param>
        /// <returns>The submitted code.</returns>
        public Task<Submission> GetSubmission(int assignmentId, string weblabId)
            => GetSubmission(assignmentId.ToString(CultureInfo.InvariantCulture), weblabId);

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
}
