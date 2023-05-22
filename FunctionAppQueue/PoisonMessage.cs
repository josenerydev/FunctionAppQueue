namespace FunctionAppQueue
{
    public class PoisonMessage<T>
    {
        public T OriginalMessage { get; set; }
        public MessageMetadata Metadata { get; set; }
    }
}