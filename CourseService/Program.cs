var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IMessageBus, RabbitMQMessageBus>();
builder.Services.AddSingleton<CourseRepository>();

var app = builder.Build();

app.MapGet("/courses", (CourseRepository repo) => repo.GetAll());
app.MapGet("/courses/{id}", (string id, CourseRepository repo) =>
{
    var course = repo.GetById(id);
    return course != null ? Results.Ok(course) : Results.NotFound();
});

// Обработчики событий Saga
var messageBus = app.Services.GetRequiredService<IMessageBus>();
var courseRepo = app.Services.GetRequiredService<CourseRepository>();

// Обработка запроса на резервирование места
await messageBus.SubscribeAsync<EnrollmentRequested>(queueName: "course.enrollment.requests", routingKey: "course.enrollment.requests", async (enrollmentRequest) =>
{
    System.Console.WriteLine($"CourseService: Получен запрос на резервирование места для курса {enrollmentRequest.CourseId}");
    if (courseRepo.TryReverseSpot(enrollmentRequest.CourseId))
    {
        System.Console.WriteLine($"CourseService: Место зарезервированно для курса {enrollmentRequest.CourseId}");
        await messageBus.PublishAsync(new SpotReserved
        {
            SagaId = enrollmentRequest.SagaId,
            CourseId = enrollmentRequest.CourseId,
            EnrollmentId = enrollmentRequest.EnrollmentId
        }, routingKey: "enrollment.spot.reserved");
    }
    else
    {
        System.Console.WriteLine($"CourseService: Нет свободных мест на курсе {enrollmentRequest.CourseId}");
        await messageBus.PublishAsync(new SpotReservationFailed
        {
            SagaId = enrollmentRequest.SagaId,
            CourseId = enrollmentRequest.CourseId,
            EnrollmentId = enrollmentRequest.EnrollmentId,
            Reason = "Нет свободных мест"
        }, routingKey: "enrollment.spot.failed");
    }
});

// Компенсирующее действие - отмена резервирования
await messageBus.SubscribeAsync<SpotReservationCanelled>(queueName: "course.spot.cancellation", routingKey: "course.spot.cancellation", async (cancellation) =>
{
    Console.WriteLine($"CourseService: Отмена резервирования места для курса {cancellation.CourseId}");
    courseRepo.ReleaseSpot(cancellation.CourseId);
});

messageBus.StartConsuming();
app.Run();
