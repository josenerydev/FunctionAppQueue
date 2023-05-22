using System.Threading.Tasks;

namespace FunctionAppQueue
{
    public interface IQueueService
    {
        Task SendToPoisonQueueAsync(string message);
    }
}