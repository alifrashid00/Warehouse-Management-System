using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockFlow.Application.Assistant;

namespace StockFlow.Infrastructure.Ai;

/// <summary>Talks to Gemini through its OpenAI-compatible chat completions endpoint.</summary>
public class GeminiChatClient : IAiChatClient
{
    private readonly HttpClient _httpClient;
    private readonly AiSettings _settings;
    private readonly ILogger<GeminiChatClient> _logger;

    public GeminiChatClient(HttpClient httpClient, IOptions<AiSettings> settings, ILogger<GeminiChatClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ChatMessage> CompleteAsync(IReadOnlyList<ChatMessage> messages, IReadOnlyList<ToolDefinition> tools, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new AiProviderException("The AI provider is not configured (missing Ai:ApiKey).");
        }

        var body = BuildRequestBody(messages, tools);
        var url = new Uri(new Uri(_settings.BaseUrl.TrimEnd('/') + "/"), "chat/completions");

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new AiProviderException("Could not reach the AI provider.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiProviderException("The AI provider timed out.", ex);
        }

        using (response)
        {
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI provider returned {StatusCode}: {Body}", (int)response.StatusCode, Truncate(responseText));

                var message = response.StatusCode == HttpStatusCode.TooManyRequests
                    ? "The AI provider rate limit was reached. Try again in a minute."
                    : $"The AI provider returned an error ({(int)response.StatusCode}).";
                throw new AiProviderException(message);
            }

            return ParseResponse(responseText);
        }
    }

    private JsonObject BuildRequestBody(IReadOnlyList<ChatMessage> messages, IReadOnlyList<ToolDefinition> tools)
    {
        var body = new JsonObject
        {
            ["model"] = _settings.Model,
            ["messages"] = new JsonArray(messages.Select(ToJson).ToArray()),
        };

        if (tools.Count > 0)
        {
            body["tools"] = new JsonArray(tools.Select(t => (JsonNode)new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = t.Name,
                    ["description"] = t.Description,
                    ["parameters"] = JsonNode.Parse(t.ParametersJsonSchema),
                },
            }).ToArray());
            body["tool_choice"] = "auto";
        }

        return body;
    }

    private static JsonNode ToJson(ChatMessage message)
    {
        // The assistant's own earlier reply goes back exactly as received (keeps Gemini's thought signature).
        if (message.Role == "assistant" && message.ProviderPayload is not null)
        {
            return JsonNode.Parse(message.ProviderPayload)!;
        }

        var json = new JsonObject { ["role"] = message.Role, ["content"] = message.Content };

        if (message.Role == "tool")
        {
            json["tool_call_id"] = message.ToolCallId;
            json["name"] = message.ToolName;
        }

        return json;
    }

    private static ChatMessage ParseResponse(string responseText)
    {
        var root = JsonNode.Parse(responseText);
        var message = root?["choices"]?[0]?["message"];
        if (message is null)
        {
            throw new AiProviderException("The AI provider returned an unexpected response.");
        }

        var toolCalls = message["tool_calls"]?.AsArray()
            .Select(call => new ToolCall(
                call!["id"]?.GetValue<string>() ?? string.Empty,
                call["function"]!["name"]!.GetValue<string>(),
                call["function"]!["arguments"]?.GetValue<string>() ?? "{}"))
            .ToList();

        return new ChatMessage(
            "assistant",
            message["content"]?.GetValue<string>(),
            toolCalls is { Count: > 0 } ? toolCalls : null,
            ProviderPayload: message.ToJsonString());
    }

    private static string Truncate(string text) => text.Length <= 500 ? text : text[..500];
}
