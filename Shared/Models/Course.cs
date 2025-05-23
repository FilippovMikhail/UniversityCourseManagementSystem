public class Course
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public int CurrentEnrollments { get; set; }
    public DateTime CreatedAt { get; set; }
}