using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                    result.Add(ToSubmission(row, assignmentId));
                }
            }

            return result;
        }

        /// <summary>
        /// Parses the submission HTML.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The submission code.</returns>
        public static async Task<string> ParseSubmission(string source)
        {
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(req => req.Content(source)).ConfigureAwait(false);

            return document.All.First(x => x.ClassList.Contains("inputTextarea")).InnerHtml;
        }

        private static SubmissionInfo ToSubmission(IHtmlTableRowElement element, string assignmentId)
        {
            string url = element.Children[1].Children[0].GetAttribute("href");
            Student student = ToStudent(element.Children[0], url);
            return new SubmissionInfo(student, assignmentId, url);
        }

        private static Student ToStudent(IElement element, string url)
        {
            string email = element.Children[2].Children[0].InnerHtml;
            string parseable = element.Children[1].GetAttribute("title");

            Match match = Regex.Match(parseable, @"netid:(\w+) studnr:(\w+). Opens in new window");

            string netId = match.Groups[1].Value;
            string studentId = match.Groups[2].Value;

            bool forGrade = element.Children.Length < 4 || element.Children[3].InnerHtml != "not enrolled for grade";

            string name = element.Children[1].Text();

            match = Regex.Match(url, @"\/submission\/([^\/]+)\/view");
            string weblabId = match.Groups[1].Value;

            return new Student(name, forGrade, email, netId, studentId, weblabId);
        }
    }
}
