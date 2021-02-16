using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace WebLabNet
{
    /// <summary>
    /// Internal class for helping with parsing.
    /// </summary>
    internal static class ParserHelper
    {
        /// <summary>
        /// Parses the submissions HTML.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="assignmentId">The ID of the assignment.</param>
        /// <returns>The submissions.</returns>
        public static async Task<IEnumerable<SubmissionInfo>> ParseSubmissions(string source, string assignmentId)
        {
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(req => req.Content(source)).ConfigureAwait(false);

            IElement? table = document.All.Where(el => el.LocalName == "tbody").Skip(1).FirstOrDefault();
            List<SubmissionInfo> result = new List<SubmissionInfo>();

            foreach (IElement child in table.Children)
            {
                if (child is IHtmlTableRowElement row)
                {
                    result.Add(ParseSubmissionInfo(document, row, assignmentId));
                }
            }

            return result;
        }

        /// <summary>
        /// Parses the submission HTML.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The submission code.</returns>
        public static async Task<Submission> ParseSubmission(string source)
        {
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(req => req.Content(source)).ConfigureAwait(false);

            string[] snippets = document.All.Where(x => x.ClassList.Contains("inputTextarea")).Select(x => x.InnerHtml).ToArray();

            string solution = HttpUtility.HtmlDecode(snippets[0]);
            string test = HttpUtility.HtmlDecode(snippets[1]);

            IElement studentElement = document.All.First(x => x.HasAttribute("href") && x.GetAttribute("href").Contains("dossier"));

            Match match = Regex.Match(source, @"{""name"":""student"", ""value"":""([^""]+)""}");

            string weblabId = match.Groups[1].Value.Trim();

            (string netId, string studentId) = ExtractIds(studentElement);
            string name = studentElement.Text().Trim();

            Student student = new Student(name, netId, studentId, weblabId);

            IElement savedString = document.All.First(x => x.TextContent == "Last saved at");
            string dateString = savedString.ParentElement.Children[1].Children[0].Text().Trim();
            DateTime date = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
            return new Submission(student, solution, test, date);
        }

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "We just want to attempt to retrieve the date.")]
        private static SubmissionInfo ParseSubmissionInfo(IDocument document, IHtmlTableRowElement element, string assignmentId)
        {
            string url = element.Children[1].Children[0].GetAttribute("href");
            StudentVerbose student = ToStudent(element.Children[0], url);
            bool started = element.Children[2].Children.Any(x => x.ClassList.Contains("text-success"));
            int spectests = 0;
            if (element.Children[2].Children.Length >= 3)
            {
                string spectestString = element.Children[2].Children[2].Children[0].Text();
                Match match = Regex.Match(spectestString, @"\d+");
                if (match.Success)
                {
                    spectests = int.Parse(match.Value, CultureInfo.InvariantCulture);
                }
            }

            bool completed = element.Children[3].Children.Any(x => x.ClassList.Contains("text-success"));
            bool passed = element.Children[5].Children.Any(x => x.ClassList.Contains("text-success"));
            double grade = GetGrade(element.Children[4]);

            Guid guid = Guid.Empty;
            DateTime date = DateTime.MinValue;

            if (started)
            {
                string? guidString = element.Children[4].Children.FirstOrDefault(x => x.Id != null && x.Id.StartsWith("status-", StringComparison.Ordinal))?.Id?.Substring(7);
                if (guidString != null)
                {
                    guid = Guid.Parse(guidString);
                }

                string searchPattern = $"netid:{student.NetId} studnr:{student.StudentId}. Opens in new window";
                IElement? foundInfo = document.All.FirstOrDefault(x => x.Id == guidString);
                if (foundInfo != null)
                {
                    try
                    {
                        date = ExtractDate(foundInfo);
                    }
                    catch
                    {
                    }
                }
            }

            return new SubmissionInfo(student, guid, assignmentId, url, started, spectests, completed, grade, passed, date);
        }

        private static DateTime ExtractDate(IElement element)
        {
            element = element
                .Children[0]
                .Children[0]
                .Children[1]
                .Children[1]
                .Children[0]
                .Children[1]
                .Children[4];

            string text = element.Text();

            Match match = Regex.Match(text, @"saved(.+)before");
            string dateString = match.Groups[1].Value.Trim();
            DateTime date = DateTime.Parse(dateString, CultureInfo.InvariantCulture);

            return date;
        }

        private static double GetGrade(IElement element)
        {
            if (element.Children.Length <= 0)
            {
                return 0;
            }

            string gradeString = element.Children[0].Text();

            if (string.IsNullOrWhiteSpace(gradeString))
            {
                gradeString = element.Text();
            }

            Match match = Regex.Match(gradeString, @"-?\d+\.\d+");
            gradeString = match.Value;
            return double.Parse(gradeString, CultureInfo.InvariantCulture);
        }

        private static StudentVerbose ToStudent(IElement element, string url)
        {
            string email = element.Children[2].Children[0].InnerHtml.Trim();
            (string netId, string studentId) = ExtractIds(element.Children[1]);

            bool forGrade = element.Children.Length < 4 || element.Children[3].InnerHtml != "not enrolled for grade";

            string name = element.Children[1].Text().Trim();

            Match match = Regex.Match(url, @"\/submission\/([^\/]+)\/view");
            string weblabId = match.Groups[1].Value.Trim();

            return new StudentVerbose(name, forGrade, email, netId, studentId, weblabId);
        }

        private static (string NetId, string StudentId) ExtractIds(IElement element)
        {
            string parseable = element.GetAttribute("title");

            Match match = Regex.Match(parseable, @"netid:(\w+) studnr:(\w+). Opens in new window");

            string netId = match.Groups[1].Value.Trim();
            string studentId = match.Groups[2].Value.Trim();

            return (netId, studentId);
        }
    }
}
