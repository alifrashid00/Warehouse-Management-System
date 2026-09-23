namespace StockFlow.Application.Assistant;

public sealed record AskResponse(string Answer, IReadOnlyList<string> ToolsUsed);
