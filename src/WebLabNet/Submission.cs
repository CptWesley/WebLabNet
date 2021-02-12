namespace WebLabNet
{
    /// <summary>
    /// Represents a submission.
    /// </summary>
    public class Submission
    {
        private readonly string url;

        /// <summary>
        /// Initializes a new instance of the <see cref="Submission"/> class.
        /// </summary>
        /// <param name="student">The student who made the submission.</param>
        /// <param name="url">The URL of the submission.</param>
        internal Submission(Student student, string url)
        {
            Student = student;
            this.url = url;
        }

        /// <summary>
        /// Gets the student.
        /// </summary>
        /// <value>
        /// The student.
        /// </value>
        public Student Student { get; }
    }

#pragma warning disable
    /// <summary>
    /// Represents a student.
    /// </summary>
    /// <param name="Name">The name of the student.</param>
    /// <param name="EnrolledForGrade">Indicates whether the student is enrolled for a grade or not.</param>
    /// <param name="NetId">The netid of the student.</param>
    /// <param name="StudentId">The student number of the student.</param>
    /// <param name="WebLabId">The ID used by weblab for the student.</param>
    public record Student(string Name, bool EnrolledForGrade, string NetId, string StudentId, string WebLabId);

#pragma warning restore
}
