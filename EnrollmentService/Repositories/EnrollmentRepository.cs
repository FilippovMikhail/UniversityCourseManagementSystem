using System.Collections.Concurrent;

public class EnrollmentRepository
{
    private readonly ConcurrentDictionary<string, Enrollment> _enrollments = new();

    public void Add(Enrollment enrollment) => _enrollments[enrollment.Id] = enrollment;

    public Enrollment? GetById(string id) => _enrollments.TryGetValue(id, out var enrollment) ? enrollment : null;

    public void Update(Enrollment enrollment) => _enrollments[enrollment.Id] = enrollment;

    public IEnumerable<Enrollment> GetByStudentId(string studentId) =>
        _enrollments.Values.Where(_ => _.StudentId == studentId);
}