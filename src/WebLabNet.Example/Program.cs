using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebLabNet.Example
{
    /// <summary>
    /// Entry class of the program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <returns>The task of running the application.</returns>
        public static async Task Main()
        {
            using WebLab webLab = new WebLab
            {
                Cookie = File.ReadAllText("cookie.txt"),
            };

            IEnumerable<SubmissionInfo> submissions = await webLab.GetSubmissions(67542).ConfigureAwait(false);
            SubmissionInfo submission = submissions.First(x => x.Student.NetId == "wjbaartman");
            string code = await webLab.GetSubmissionCode(submission).ConfigureAwait(false);

            double grade = 5.0;

            await webLab.PushGradeAsync(submission.Student.NetId, grade, "blaaaaa", DateTime.UtcNow, "apikeyhier").ConfigureAwait(false);

            Console.WriteLine(code);
        }
    }
}
