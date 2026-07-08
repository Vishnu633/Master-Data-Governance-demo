using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Hofinsoft.Mdg.Models.Dto;

namespace Hofinsoft.Mdg.Services
{
    public class GeminiService
    {
        private readonly HttpClient _client;
        private readonly ILogger<GeminiService> _logger;
        private readonly string? _apiKey;

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

        public GeminiService(HttpClient client, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _client = client;
            _logger = logger;
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Gemini API key is not configured. Set GEMINI_API_KEY environment variable or Gemini:ApiKey configuration.");
            }
        }

        public async Task<float[]?> GenerateEmbeddingAsync(string text)
        {
            if (!IsConfigured) return null;

            try
            {
                var url = $"/v1beta/models/text-embedding-004:embedContent?key={_apiKey}";
                var requestBody = new
                {
                    model = "models/text-embedding-004",
                    content = new
                    {
                        parts = new[] { new { text = text } }
                    }
                };

                var response = await _client.PostAsJsonAsync(url, requestBody);
                if (!response.IsSuccessStatusCode)
                {
                    var errContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini Embedding API returned non-success code {StatusCode}: {Error}", response.StatusCode, errContent);
                    return null;
                }

                var responseData = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>();
                return responseData?.Embedding?.Values;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini Embedding API");
                return null;
            }
        }

        public async Task<string> ChatAsync(string systemPrompt, string userMessage)
        {
            if (!IsConfigured)
            {
                return "Gemini API key is not configured. Please set the GEMINI_API_KEY environment variable.";
            }

            try
            {
                var url = $"/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemPrompt } }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = userMessage } }
                        }
                    }
                };

                var response = await _client.PostAsJsonAsync(url, requestBody);
                if (!response.IsSuccessStatusCode)
                {
                    var errContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini Chat API returned non-success code {StatusCode}: {Error}", response.StatusCode, errContent);
                    return "Sorry, I encountered an error communicating with the Gemini AI service.";
                }

                var responseData = await response.Content.ReadFromJsonAsync<GeminiChatResponse>();
                var reply = responseData?.Candidates?[0]?.Content?.Parts?[0]?.Text;
                return reply ?? "I could not generate a response.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini Chat API");
                return "An error occurred while processing your request.";
            }
        }

        public async Task<AiClassificationResult?> ClassifyMaterialAsync(string freeText, List<string> availableProfiles)
        {
            if (!IsConfigured) return null;

            string profilesList = string.Join(", ", availableProfiles);
            string systemPrompt = $@"You are an AI material master data classification assistant.
Your task is to parse a natural language description of a material and map it to one of the available classification profiles.
Available classification profiles: [{profilesList}]

You must extract:
1. The matching 'Noun' and 'Modifier' from the available profiles (case-sensitive, exactly matching one of the options).
2. Any attribute values described in the text.
3. The targeted plant (usually starts with 'PLT' followed by a number, e.g. PLT1, PLT2).
4. A confidence score between 0.0 and 1.0.

Response MUST be a valid, raw JSON object ONLY, with no markdown code blocks, conforming to this schema:
{{
  ""noun"": ""string (e.g. BEARING)"",
  ""modifier"": ""string (e.g. BALL)"",
  ""plant"": ""string (default: PLT1)"",
  ""attributes"": {{
    ""Attribute_Name"": ""string value""
  }},
  ""confidence"": 0.95
}}";

            try
            {
                var reply = await ChatAsync(systemPrompt, freeText);
                
                // Clean the output if it contains markdown formatting blocks
                string jsonText = reply.Trim();
                if (jsonText.StartsWith("```"))
                {
                    int startIdx = jsonText.IndexOf('{');
                    int endIdx = jsonText.LastIndexOf('}');
                    if (startIdx >= 0 && endIdx >= 0 && endIdx > startIdx)
                    {
                        jsonText = jsonText.Substring(startIdx, endIdx - startIdx + 1);
                    }
                }

                var result = JsonSerializer.Deserialize<AiClassificationResult>(jsonText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse AI classification output");
                return null;
            }
        }

        // --- Helper response models ---
        private class GeminiEmbeddingResponse
        {
            [JsonPropertyName("embedding")]
            public GeminiEmbeddingData? Embedding { get; set; }
        }

        private class GeminiEmbeddingData
        {
            [JsonPropertyName("values")]
            public float[]? Values { get; set; }
        }

        private class GeminiChatResponse
        {
            [JsonPropertyName("candidates")]
            public GeminiCandidate[]? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            [JsonPropertyName("content")]
            public GeminiContent? Content { get; set; }
        }

        private class GeminiContent
        {
            [JsonPropertyName("parts")]
            public GeminiPart[]? Parts { get; set; }
        }

        private class GeminiPart
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}
