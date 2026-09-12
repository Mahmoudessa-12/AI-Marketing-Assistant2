using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebApplication1.Services;

public class OpenAiMarketingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenAiMarketingService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GenerateCampaignAsync(
        string campaignGoal,
        decimal targetBudget,
        int durationWeeks,
        List<Dictionary<string, string>> products,
        string outputLanguage = "English")
    {
        // Read Hugging Face settings
        var apiKey = _configuration["HuggingFace:ApiKey"];
        var model = _configuration["HuggingFace:Model"];
        var baseUrl = _configuration["HuggingFace:BaseUrl"];

        // Validate configuration
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("Hugging Face API key is missing.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new Exception("Hugging Face model is missing.");
        }

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new Exception("Hugging Face BaseUrl is missing.");
        }

        // Convert products to JSON
        var productJson = JsonSerializer.Serialize(
            products,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        // System prompt
        var systemPrompt =
            "You are an expert e-commerce marketing strategist.\n\n" +

            "Your job is to create realistic marketing campaigns " +
            "using ONLY the product information provided by the user.\n\n" +

            "The user wants the campaign output in this language:\n" +
            outputLanguage +
            "\n\n" +

            "IMPORTANT LANGUAGE RULE:\n" +
            "- Write ALL human-readable campaign content in the requested language.\n" +
            "- The campaign goal may be written in Arabic or English.\n" +
            "- Understand the campaign goal regardless of its language.\n" +
            "- JSON property names MUST remain exactly as specified.\n" +
            "- Do not translate JSON property names.\n\n" +

            "STRICT GUARDRAILS:\n" +
            "- Never invent product facts.\n" +
            "- Never invent prices.\n" +
            "- Never invent discounts.\n" +
            "- Never invent ratings.\n" +
            "- Never invent stock quantities.\n" +
            "- Never invent shipping information.\n" +
            "- Never invent guarantees.\n" +
            "- Never invent certifications.\n" +
            "- Never invent reviews.\n" +
            "- Never invent sales numbers.\n" +
            "- Never invent performance statistics.\n" +
            "- Never claim information that is not present in the CSV.\n" +
            "- If information is missing, do not make it up.\n" +
            "- The total campaign budget must not exceed the target budget.\n\n" +

            "Return ONLY valid JSON.\n" +
            "Do not return Markdown.\n" +
            "Do not add explanations outside the JSON.";

        // User prompt
        var userPrompt =
            "Create an e-commerce marketing campaign.\n\n" +

            "Output Language:\n" +
            outputLanguage +
            "\n\n" +

            "Campaign Goal:\n" +
            campaignGoal +
            "\n\n" +

            "Target Budget:\n" +
            targetBudget.ToString() +
            "\n\n" +

            "Duration:\n" +
            durationWeeks.ToString() +
            " weeks\n\n" +

            "Product Dataset:\n" +
            productJson +
            "\n\n" +

            "Return JSON using exactly this structure:\n\n" +

            "{\n" +
            "  \"campaign\": {\n" +
            "    \"summary\": \"string\",\n" +
            "    \"calendar\": [\n" +
            "      {\n" +
            "        \"week\": 1,\n" +
            "        \"channel\": \"string\",\n" +
            "        \"objective\": \"string\",\n" +
            "        \"audience\": \"string\",\n" +
            "        \"contentType\": \"string\",\n" +
            "        \"copy\": \"string\",\n" +
            "        \"budget\": 0,\n" +
            "        \"kpi\": \"string\"\n" +
            "      }\n" +
            "    ],\n" +
            "    \"personas\": [\n" +
            "      {\n" +
            "        \"name\": \"string\",\n" +
            "        \"description\": \"string\",\n" +
            "        \"painPoints\": [\n" +
            "          \"string\"\n" +
            "        ],\n" +
            "        \"copyVariations\": [\n" +
            "          \"string\",\n" +
            "          \"string\",\n" +
            "          \"string\"\n" +
            "        ]\n" +
            "      }\n" +
            "    ]\n" +
            "  }\n" +
            "}\n\n" +

            "Return ONLY valid JSON.";

        // Request body
        var requestBody = new
        {
            model = model,

            messages = new[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },

                new
                {
                    role = "user",
                    content = userPrompt
                }
            },

            temperature = 0.3,

            max_tokens = 4000
        };

        // Convert request to JSON
        var requestJson = JsonSerializer.Serialize(requestBody);

        // Create HTTP request
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            baseUrl);

        // Add Authorization header
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);

        // Add JSON body
        request.Content = new StringContent(
            requestJson,
            Encoding.UTF8,
            "application/json");

        // Send request
        using var response =
            await _httpClient.SendAsync(request);

        // Read response
        var responseText =
            await response.Content.ReadAsStringAsync();

        // Handle API errors
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                "Hugging Face API Error: " +
                response.StatusCode +
                " - " +
                responseText);
        }

        // Parse Hugging Face response
        using var document =
            JsonDocument.Parse(responseText);

        // Check choices
        if (!document.RootElement.TryGetProperty(
                "choices",
                out var choices))
        {
            throw new Exception(
                "Hugging Face response does not contain 'choices'.");
        }

        if (choices.GetArrayLength() == 0)
        {
            throw new Exception(
                "Hugging Face returned no choices.");
        }

        // Get AI message
        var content =
            choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new Exception(
                "AI returned an empty response.");
        }

        // Return AI JSON
        return content;
    }
}