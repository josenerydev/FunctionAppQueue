namespace SenderMessege
{
    public interface IQueueService
    {
        Task SendMessageAsync(string message, string queueName);
    }
}