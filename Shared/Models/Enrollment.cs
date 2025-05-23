public class Enrollment
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public EnrollmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SagaId { get; set; } = string.Empty;
}

public enum EnrollmentStatus
{
    Pending, // Ожидает обработки
    Reserved, // Место зарезервированно
    Confirmed, // Запись подтверждена
    Failed, // Не удалось записать
    Cancelled // Отменена
}