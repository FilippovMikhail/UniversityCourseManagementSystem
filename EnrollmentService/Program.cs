var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IMessageBus, RabbitMQMessageBus>();
builder.Services.AddSingleton<EnrollmentRepository>();

var app = builder.Build();

app.MapPost("/enrollments", async (EnrollmentRequest request, IMessageBus messageBus, EnrollmentRepository repo) =>
{
    var enrollmentId = Guid.NewGuid().ToString();
    var sagaId = Guid.NewGuid().ToString();

    // Создаем запись со статусом В ожидании
    var enrollment = new Enrollment
    {
        Id = enrollmentId,
        StudentId = request.StudentId,
        CourseId = request.CourseId,
        Status = EnrollmentStatus.Pending,
        CreatedAt = DateTime.UtcNow,
        SagaId = sagaId
    };

    repo.Add(enrollment);

    System.Console.WriteLine($"EnrollmentService: Начинаем процесс записи студента {request.StudentId} на курс {request.CourseId}");

    // Инициируем Saga процесс
    await messageBus.PublishAsync(new EnrollmentRequested
    {
        SagaId = sagaId,
        StudentId = request.StudentId,
        CourseId = request.CourseId,
        EnrollmentId = enrollmentId
    }, routingKey: "course.enrollment.requests");

    return Results.Ok(new { EnrollmentId = enrollmentId, SagaId = sagaId });
});

app.MapGet("/enrollments/student/{studentId}", (string studentId, EnrollmentRepository repo) =>
    repo.GetByStudentId(studentId));

// Обработчик событий Saga
var messageBus = app.Services.GetRequiredService<IMessageBus>();
var enrollmentRepo = app.Services.GetRequiredService<EnrollmentRepository>();

// Успешное резервирование места
await messageBus.SubscribeAsync<SpotReserved>(queueName: "enrollment.spot.reserved", routingKey: "enrollment.spot.reserved", async (spotReserved) =>
{
    System.Console.WriteLine($"EnrollmentService: Место зарезервированно для записи {spotReserved.EnrollmentId}");

    var enrollment = enrollmentRepo.GetById(spotReserved.EnrollmentId);
    if (enrollment is not null)
    {
        enrollment.Status = EnrollmentStatus.Reserved;
        enrollmentRepo.Update(enrollment);

        // Подтверждаем запись
        await messageBus.PublishAsync(new EnrollmentConfirmed
        {
            SagaId = spotReserved.SagaId,
            EnrollmentId = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId
        }, routingKey: "notification.enrollment.confirmed");
    }
});

// Неудачное резервирование места
await messageBus.SubscribeAsync<SpotReservationFailed>(queueName: "enrollment.spot.failed", routingKey: "enrollment.spot.failed", async (reservationFailed) =>
{
    System.Console.WriteLine($"EnrollmentService: Не удалось зарезервировать место. {reservationFailed.Reason}");

    var enrollment = enrollmentRepo.GetById(reservationFailed.EnrollmentId);
    if (enrollment is not null)
    {
        enrollment.Status = EnrollmentStatus.Failed;
        enrollmentRepo.Update(enrollment);

        // Неудача
        await messageBus.PublishAsync(new EnrollmentFailed
        {
            SagaId = reservationFailed.SagaId,
            EnrollmentId = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            Reason = reservationFailed.Reason
        }, routingKey: "notification.enrollment.failed");
    }
});

messageBus.StartConsuming();

app.Run();

public class EnrollmentRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
}