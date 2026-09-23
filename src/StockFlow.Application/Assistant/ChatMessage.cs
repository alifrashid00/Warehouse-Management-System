namespace StockFlow.Application.Assistant;

public sealed record ToolCall(string Id, string Name, string ArgumentsJson);

/// <param name="ProviderPayload">
/// Opaque provider data (for Gemini: the raw assistant message, including its thought signature).
/// It must be sent back unchanged on the next request, so the service never inspects it.
/// </param>
public sealed record ChatMessage(
    string Role,
    string? Content,
    IReadOnlyList<ToolCall>? ToolCalls = null,
    string? ToolCallId = null,
    string? ToolName = null,
    string? ProviderPayload = null)
{
    public static ChatMessage System(string content) => new("system", content);
    public static ChatMessage User(string content) => new("user", content);
    public static ChatMessage Tool(string toolCallId, string toolName, string content) => new("tool", content, ToolCallId: toolCallId, ToolName: toolName);
}
