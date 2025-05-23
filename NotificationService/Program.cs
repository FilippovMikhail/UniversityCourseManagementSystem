var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IMessageBus, RabbitMQMessageBus>();

var app = builder.Build();

var messageBus = app.Services.GetRequiredService<IMessageBus>();
var notificationSender = new NotificationSender();

// Обработка успешной записи на курс
await messageBus.SubscribeAsync<EnrollmentConfirmed>(queueName: "notification.enrollment.confirmed", routingKey: "notification.enrollment.confirmed", async (confirmed) =>
{
    await notificationSender.SendNotificationAsync(
        confirmed.StudentId,
        $"Поздравляем! Вы успешно записались на курс {confirmed.CourseId}"
    );

    await messageBus.PublishAsync(new NotificationSent
    {
        SagaId = confirmed.SagaId,
        RecipientId = confirmed.StudentId,
        Message = "Уведомление о успешной записи отправлено"
    }, routingKey: "notification.enrollment.success");
});

// Обработка неудачной записи на курс
await messageBus.SubscribeAsync<EnrollmentFailed>(queueName: "notification.enrollment.failed", routingKey: "notification.enrollment.failed", async (failed) =>
{
    await notificationSender.SendNotificationAsync(
        failed.StudentId,
        $"К сожалению, не удалось записать вас на курс {failed.CourseId}. Причина: {failed.Reason}"
    );

    await messageBus.PublishAsync(new NotificationSent
    {
        SagaId = failed.SagaId,
        RecipientId = failed.StudentId,
        Message = "Уведомление о неудачной записи отправлено"
    }, routingKey: "notification.enrollment.failed");
});

messageBus.StartConsuming();

app.Run();
