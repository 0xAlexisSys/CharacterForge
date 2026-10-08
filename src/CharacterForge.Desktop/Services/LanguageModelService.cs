using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;

namespace CharacterForge.Desktop.Services;

public sealed class LanguageModelService(HttpClient httpClient)
{
    private const int MaxTokens = 4096;

    // TODO: Make endpoint URL configurable.
    private static readonly Uri DefaultEndpoint = new("http://localhost:8080/v1/chat/completions");

    private static string ReadAsset(string relativePath)
    {
        using Stream stream = AssetLoader.Open(new($"avares://CharacterForge.Desktop/Assets/{relativePath}"));
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    public async Task<string> GenerateAsync(string systemPromptName, string grammarName, string userPrompt, CancellationToken cancellationToken = default)
    {
        string systemPrompt = ReadAsset($"SystemPrompts/{systemPromptName}.txt").Trim();
        string grammar = ReadAsset($"Grammars/{grammarName}.gbnf");

        // TODO: Make samplers configurable.
        object request = new
        {
            stream = false,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt,
                },
                new
                {
                    role = "user",
                    content = userPrompt,
                },
            },
            max_completion_tokens = MaxTokens,
            max_tokens = MaxTokens, // Deprecated but some OpenAI-compatible APIs might not know max_completion_tokens.
            temperature = 1.0F,
            top_k = 64,
            top_p = 0.95F,
            min_p = 0.0F,
            repeat_penalty = 1.0F,
            presence_penalty = 0.0F,
            frequency_penalty = 0.0F,
            grammar = grammar,
            reasoning_effort = "none",
            chat_template_kwargs = new {enable_thinking = false},
        };

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(DefaultEndpoint, request, cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using JsonDocument document = JsonDocument.Parse(responseContent);
        return document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }
}
