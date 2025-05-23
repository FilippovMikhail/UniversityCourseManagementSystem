public abstract record BaseEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string SagaId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

// События для Saga процесса записи на курс
public record EnrollmentRequested : BaseEvent
{
    public string StudentId { get; init; } = string.Empty;
    public string CourseId { get; init; } = string.Empty;
    public string EnrollmentId { get; init; } = string.Empty;
}

public record SpotReserved : BaseEvent
{
    public string CourseId { get; init; } = string.Empty;
    public string EnrollmentId { get; init; } = string.Empty;
}

public record SpotReservationFailed : BaseEvent
{
    public string CourseId { get; init; } = string.Empty;
    public string EnrollmentId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record EnrollmentConfirmed : BaseEvent
{
    public string EnrollmentId { get; init; } = string.Empty;
    public string StudentId { get; init; } = string.Empty;
    public string CourseId { get; init; } = string.Empty;
}

public record EnrollmentFailed : BaseEvent
{
    public string EnrollmentId { get; init; } = string.Empty;
    public string StudentId { get; init; } = string.Empty;
    public string CourseId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record NotificationSent : BaseEvent
{
    public string RecipientId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

// Компенсирующие события для отката изменений
public record SpotReservationCanelled : BaseEvent
{
    public string CourseId { get; init; } = string.Empty;
    public string EnrollmentId { get; init; } = string.Empty;
}