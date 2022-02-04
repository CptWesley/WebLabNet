using System;
using System.IO;
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
            using WebLab webLab = new WebLab();

            string apiKey = File.ReadAllText("apikey.txt");
            string apiSecret = File.ReadAllText("apisecret.txt");
            string gradeApiKey = File.ReadAllText("gradeapikey.txt");

            var submissions = await webLab.GetSubmissionsAsync(90830, apiKey, apiSecret).ConfigureAwait(false);
            if (submissions.Success)
            {
                foreach (var submissionInfo in submissions.Data!.Submissions)
                {
                    var submission = await submissionInfo.GetSubmissionAsync().ConfigureAwait(false);
                    var data = submission.Data;
                    if (data is not null && data.Student == 41321)
                    {
                        Console.WriteLine(data.SolutionCode);
                        await data.PushGradeAsync(7, "TEST", gradeApiKey).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
