namespace StockFlow.Application.Assistant;

public interface IAiChatClient
{
    /// <exception cref="AiProviderException">The provider is unreachable, rejected the request or is not configured.</exception>
    Task<ChatMessage> CompleteAsync(IReadOnlyList<ChatMessage> messages, IReadOnlyList<ToolDefinition> tools, CancellationToken cancellationToken);
}
