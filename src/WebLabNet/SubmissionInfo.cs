using System.Diagnostics.CodeAnalysis;

namespace WebLabNet
{
#pragma warning disable CS1572
#pragma warning disable CS1573
#pragma warning disable CS1591

    /// <summary>
    /// Represents a submission.
    /// </summary>
    /// <param name="Student">The student who made the submission.</param>
    /// <param name="AssignmentId">The ID of the assignment.</param>
    /// <param name="Url">The URL of the submission.</param>
    [SuppressMessage("Microsoft.Design", "CA1054", Justification = "We want to maintain the url as a string.")]
    public record SubmissionInfo(Student Student, string AssignmentId, string Url)
    {
    }

    /// <summary>
    /// Represents a student.
    /// </summary>
    /// <param name="Name">The name of the student.</param>
    /// <param name="EnrolledForGrade">Indicates whether the student is enrolled for a grade or not.</param>
    /// <param name="Email">The email address of the student.</param>
    /// <param name="NetId">The netid of the student.</param>
    /// <param name="StudentId">The student number of the student.</param>
    /// <param name="WebLabId">The ID used by weblab for the student.</param>
    public record Student(string Name, bool EnrolledForGrade, string Email, string NetId, string StudentId, string WebLabId);

#pragma warning restore
}
