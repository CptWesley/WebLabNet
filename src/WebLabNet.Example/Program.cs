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

            IEnumerable<SubmissionInfo> submissions = await webLab.GetSubmissionsAsync(67542).ConfigureAwait(false);

            SubmissionInfo submissionInfo = submissions.First(x => x.Student.NetId == "wjbaartman");
            Submission submission = await webLab.GetSubmissionAsync(submissionInfo).ConfigureAwait(false);
            Console.WriteLine(submission);
        }
    }
}
