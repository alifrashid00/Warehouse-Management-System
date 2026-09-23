using System.Text.Json;
using StockFlow.Application.Categories;
using StockFlow.Application.Common;
using StockFlow.Application.Inventory;

namespace StockFlow.Application.Assistant;

public class AssistantService
{
    private const int MaxQuestionLength = 500;
    private const int MaxToolRounds = 5;

    private const string SystemPrompt =
        "You are the StockFlow warehouse assistant. Answer questions about products, categories and stock levels. " +
        "Always use the provided tools to get data and never invent products, quantities or prices. " +
        "A product is low on stock when its quantity is below its reorder level. " +
        "Keep answers short and factual. If the tools return nothing relevant, say so.";

    private static readonly IReadOnlyList<ToolDefinition> Tools =
    [
        new ToolDefinition(
            "get_stock_levels",
            "Get current stock for products: quantity on hand, reorder level, unit cost and whether it is low. " +
            "Optionally filter by text (matches product name, SKU or category) or only low-stock products.",
            """
            {
              "type": "object",
              "properties": {
                "search": { "type": "string", "description": "Text to match against product name, SKU or category name." },
                "low_only": { "type": "boolean", "description": "If true, return only products below their reorder level." }
              }
            }
            """),
        new ToolDefinition(
            "list_categories",
            "List all product categories.",
            """{ "type": "object", "properties": {} }"""),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAiChatClient _aiClient;
    private readonly StockService _stockService;
    private readonly CategoryService _categoryService;

    public AssistantService(IAiChatClient aiClient, StockService stockService, CategoryService categoryService)
    {
        _aiClient = aiClient;
        _stockService = stockService;
        _categoryService = categoryService;
    }

    public async Task<Result<AskResponse>> AskAsync(AskRequest request, CancellationToken cancellationToken)
    {
        var question = request.Question?.Trim();
        if (string.IsNullOrEmpty(question) || question.Length > MaxQuestionLength)
        {
            var error = new Error("ASSISTANT_INVALID_QUESTION", $"Question must be 1 to {MaxQuestionLength} characters.", ErrorType.Validation);
            return Result<AskResponse>.Failure(error);
        }

        var messages = new List<ChatMessage> { ChatMessage.System(SystemPrompt), ChatMessage.User(question) };
        var toolsUsed = new List<string>();

        try
        {
            for (var round = 0; round < MaxToolRounds; round++)
            {
                var reply = await _aiClient.CompleteAsync(messages, Tools, cancellationToken);
                messages.Add(reply);

                if (reply.ToolCalls is null)
                {
                    var answer = string.IsNullOrWhiteSpace(reply.Content) ? "I could not produce an answer." : reply.Content;
                    return Result<AskResponse>.Success(new AskResponse(answer, toolsUsed));
                }

                foreach (var call in reply.ToolCalls)
                {
                    toolsUsed.Add(call.Name);
                    var toolResult = await ExecuteToolAsync(call, cancellationToken);
                    messages.Add(ChatMessage.Tool(call.Id, call.Name, toolResult));
                }
            }
        }
        catch (AiProviderException ex)
        {
            return Result<AskResponse>.Failure(new Error("ASSISTANT_PROVIDER_ERROR", ex.Message, ErrorType.ExternalService));
        }

        var loopError = new Error("ASSISTANT_TOO_MANY_STEPS", "The assistant needed too many steps to answer. Try a simpler question.", ErrorType.ExternalService);
        return Result<AskResponse>.Failure(loopError);
    }

    private async Task<string> ExecuteToolAsync(ToolCall call, CancellationToken cancellationToken)
    {
        // Tool output is data for the model. Errors are returned as text so the model can react, never thrown.
        try
        {
            switch (call.Name)
            {
                case "get_stock_levels":
                    using (var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson))
                    {
                        var search = args.RootElement.TryGetProperty("search", out var s) && s.ValueKind == JsonValueKind.String ? s.GetString() : null;
                        var lowOnly = args.RootElement.TryGetProperty("low_only", out var l) && l.ValueKind == JsonValueKind.True;
                        var levels = await _stockService.GetStockLevelsAsync(search, lowOnly, cancellationToken);
                        return JsonSerializer.Serialize(levels, JsonOptions);
                    }

                case "list_categories":
                    var categories = await _categoryService.GetAllAsync(cancellationToken);
                    return JsonSerializer.Serialize(categories, JsonOptions);

                default:
                    return JsonSerializer.Serialize(new { error = $"Unknown tool '{call.Name}'." });
            }
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(new { error = "Tool arguments were not valid JSON." });
        }
    }
}
