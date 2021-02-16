using System;
using System.Diagnostics.CodeAnalysis;

namespace WebLabNet
{
#pragma warning disable CS1572
#pragma warning disable CS1573
#pragma warning disable CS1591
#pragma warning disable CA1801

    /// <summary>
    /// Represents a submission.
    /// </summary>
    /// <param name="Student">The student who made the submission.</param>
    /// <param name="SolutionCode">The solution code of the submission.</param>
    /// <param name="TestCode">The test code of the submission.</param>
    /// <param name="Saved">The last time the submission was saved.</param>
    public record Submission(Student Student, string SolutionCode, string TestCode, DateTime Saved);

    /// <summary>
    /// Represents a submission.
    /// </summary>
    /// <param name="Student">The student who made the submission.</param>
    /// <param name="Guid">The guid of this entry.</param>
    /// <param name="AssignmentId">The ID of the assignment.</param>
    /// <param name="Url">The URL of the submission.</param>
    /// <param name="Started">Indicates that the student has started the assignment.</param>
    /// <param name="Completed">Indicates that the student has completed the assignment.</param>
    /// <param name="Grade">The latest grade for the submission.</param>
    /// <param name="Passed">Indicates that the student has passed the assignment.</param>
    /// <param name="Saved">The last time the submission was saved.</param>
    [SuppressMessage("Microsoft.Design", "CA1054", Justification = "We want to maintain the url as a string.")]
    public record SubmissionInfo(StudentVerbose Student, Guid Guid, string AssignmentId, string Url, bool Started, int SpecTests, bool Completed, double Grade, bool Passed, DateTime Saved);

    /// <summary>
    /// Represents a student.
    /// </summary>
    /// <param name="Name">The name of the student.</param>
    /// <param name="NetId">The netid of the student.</param>
    /// <param name="StudentId">The student number of the student.</param>
    /// <param name="WebLabId">The ID used by weblab for the student.</param>
    public record Student(string Name, string NetId, string StudentId, string WebLabId);

    /// <summary>
    /// Represents a student.
    /// </summary>
    /// <param name="Name">The name of the student.</param>
    /// <param name="EnrolledForGrade">Indicates whether the student is enrolled for a grade or not.</param>
    /// <param name="Email">The email address of the student.</param>
    /// <param name="NetId">The netid of the student.</param>
    /// <param name="StudentId">The student number of the student.</param>
    /// <param name="WebLabId">The ID used by weblab for the student.</param>
    public record StudentVerbose(string Name, bool EnrolledForGrade, string Email, string NetId, string StudentId, string WebLabId) : Student(Name, NetId, StudentId, WebLabId);

#pragma warning restore
}
