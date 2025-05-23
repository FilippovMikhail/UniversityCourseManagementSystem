// Базза данных курсов в памяти
using System.Collections.Concurrent;

public class CourseRepository
{
    private readonly ConcurrentDictionary<string, Course> _courses = new();

    public CourseRepository()
    {
        // Добавляем тестовые курсы
        var course1 = new Course
        {
            Id = "course-1",
            Name = "Введение в программирование",
            Description = "Основы программирования на C#",
            InstructorId = "instructor-1",
            MaxStudents = 3,
            CurrentEnrollments = 0,
            CreatedAt = DateTime.UtcNow
        };
        _courses[course1.Id] = course1;
    }

    public Course? GetById(string id) => _courses.TryGetValue(id, out var course) ? course : null;
    public IEnumerable<Course> GetAll() => _courses.Values;
    public void Update(Course course) => _courses[course.Id] = course;

    public bool TryReverseSpot(string courseId)
    {
        if (_courses.TryGetValue(courseId, out var course))
        {
            if (course.CurrentEnrollments < course.MaxStudents)
            {
                course.CurrentEnrollments++;
                return true;
            }
        }
        return false;
    }

    public void ReleaseSpot(string courseId)
    {
        if (_courses.TryGetValue(courseId, out var course) && course.CurrentEnrollments > 0)
        {
            course.CurrentEnrollments--;
        }
    }
}