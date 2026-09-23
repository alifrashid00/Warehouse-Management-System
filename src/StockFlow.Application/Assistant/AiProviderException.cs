namespace StockFlow.Application.Assistant;

public class AiProviderException : Exception
{
    public AiProviderException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
