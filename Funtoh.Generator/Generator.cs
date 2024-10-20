using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;
using OpenAI.Chat;
using OpenAI.Images;
using JsonSerializer = System.Text.Json.JsonSerializer;

public enum ImageSize
{
    Square,
    Portrait,
    Landscape
}

public enum ImageQuality
{
    Standard,
    HighDefinition
}

public class Generator
{
    private string openAiKey = "";

    private readonly SystemChatMessage _systemMessage;
    private readonly ChatClient _chatClient;
    private readonly ChatCompletionOptions _chatOptions;
    private readonly JSchemaGenerator _schemaGenerator = new();
    private readonly Dictionary<Type, ChatResponseFormat> _schemas = new();
    private readonly ImageClient _imageClient;
    private readonly ChatClient _miniClient;
    private readonly ChatCompletionOptions _miniOptions;
    private readonly SystemChatMessage _miniMessage;

    record MiniResponse(string Response);

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public Generator()
    {
        _schemaGenerator.DefaultRequired = Required.Always;
        _schemaGenerator.GenerationProviders.Add(new OpenAiSchemaGenerationProvider());
        _schemaGenerator.GenerationProviders.Add(new StringEnumGenerationProvider());
        
        var roleDescription =
            "You are a helpful, creative writing assistant tasked with creating convincing demonstration data for the Funtoh app to simulate years of history in the app.";
        var appDescription =
            "The Funtoh app facilitates hustles between brand sponsors and promoters. A hustle is a contract between a sponsor and a promoter where the promoter promotes the sponsor's brand to do agreed upon work using a proof-of-work form. The users of the app are all diverse residents of Philadelphia with unique personalities and writing styles.";
        var sb = new StringBuilder().AppendLine(roleDescription).AppendLine(appDescription);
        _systemMessage = ChatMessage.CreateSystemMessage(sb.ToString());
        
        _miniMessage = ChatMessage.CreateSystemMessage("You are a helpful, creative writing assistant.");
        
        _chatClient = new ChatClient(model: "gpt-4o", apiKey: openAiKey);
        _chatOptions = new ChatCompletionOptions();
        _miniClient = new ChatClient(model: "gpt-4o-mini", apiKey: openAiKey);
        _miniOptions = new ChatCompletionOptions() { ResponseFormat = GetSchema<MiniResponse>() };
        _imageClient = new ImageClient(model: "dall-e-3", apiKey: openAiKey);
    }

    private class OpenAiSchemaGenerationProvider : JSchemaGenerationProvider
    {
        public override JSchema? GetSchema(JSchemaTypeGenerationContext context)
        {
            var schema = context.Generator.Generate(context.ObjectType);
            schema.AllowAdditionalPropertiesSpecified = true;
            schema.AllowAdditionalProperties = false;
            return schema;
        }
    }


    public async Task GenerateImage(string path, string prompt, ImageSize size, ImageQuality quality)
    {
        var response = await _imageClient.GenerateImageAsync(prompt, new ImageGenerationOptions()
        {
            Size = size switch
            {
                ImageSize.Square => GeneratedImageSize.W1024xH1024,
                ImageSize.Portrait => GeneratedImageSize.W1024xH1792,
                ImageSize.Landscape => GeneratedImageSize.W1792xH1024,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            },
            Quality = quality switch
            {
                ImageQuality.Standard => GeneratedImageQuality.Standard,
                ImageQuality.HighDefinition => GeneratedImageQuality.High,
                _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, null)
            },
            Style = GeneratedImageStyle.Natural,
            ResponseFormat = GeneratedImageFormat.Bytes
        });
        Console.Write("> ");
        Console.WriteLine(response.Value.RevisedPrompt);
        await File.WriteAllBytesAsync(path, response.Value.ImageBytes.ToArray());
    }

    public async Task<T> QuickGenerate<T>(params string[] prompt)
    {
        var sb = new StringBuilder();
        foreach (var line in prompt)
            sb.AppendLine(line);
        _miniOptions.ResponseFormat = GetSchema<T>();
        var chatResult = await _miniClient.CompleteChatAsync([_miniMessage, sb.ToString()], _miniOptions);
        var response = chatResult.Value.Content[0].Text;
        return JsonSerializer.Deserialize<T>(response, JsonSerializerOptions)
               ?? throw new Exception($"Failed to parse response: {chatResult.Value.Content[0]}");
    }

    public async Task<T> Generate<T>(params string[] prompt)
    {
        var sb = new StringBuilder();
        foreach (var line in prompt)
            sb.AppendLine(line);
        _chatOptions.ResponseFormat = GetSchema<T>();
        var chatResult = await _chatClient.CompleteChatAsync([_systemMessage, sb.ToString()], _chatOptions);
        var response = chatResult.Value.Content[0].Text;
        return JsonSerializer.Deserialize<T>(response, JsonSerializerOptions)
               ?? throw new Exception($"Failed to parse response: {chatResult.Value.Content[0]}");
    }

    public async Task<T[]> GenerateList<T>(params string[] prompt)
    {
        var sb = new StringBuilder();
        foreach (var line in prompt)
            sb.AppendLine(line);
        _chatOptions.ResponseFormat = GetListSchema<T>();
        var chatResult = await _chatClient.CompleteChatAsync([_systemMessage, sb.ToString()], _chatOptions);
        var response = chatResult.Value.Content[0].Text;
        var collection = JsonSerializer.Deserialize<Collection<T>>(response, JsonSerializerOptions)
                         ?? throw new Exception($"Failed to parse response: {chatResult.Value.Content[0]}");
        return collection.Items;
    }

    private class Collection<T>
    {
        public T[] Items { get; set; } = [];
    }

    private ChatResponseFormat GetListSchema<T>()
    {
        var type = typeof(Collection<T>);
        var typeName = typeof(T).Name;
        return GetSchema<T>(type, typeName);
    }

    private ChatResponseFormat GetSchema<T>()
    {
        var type = typeof(T);
        var typeName = type.Name;
        return GetSchema<T>(type, typeName);
    }

    private ChatResponseFormat GetSchema<T>(Type type, string typeName)
    {
        if (_schemas.TryGetValue(type, out var schema))
            return schema;

        var schemaJson = _schemaGenerator.Generate(type);

        var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(typeName,
            BinaryData.FromString(schemaJson.ToString()), jsonSchemaIsStrict: true);
        _schemas.Add(type, responseFormat);
        return responseFormat;
    }
}