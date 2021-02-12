using System.IO;

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
        public static void Main()
        {
            using WebLab webLab = new WebLab
            {
                Cookie = File.ReadAllText("cookie.txt"),
            };
        }
    }
}
