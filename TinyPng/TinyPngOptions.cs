namespace TinyPng;

[PublicAPI]
public class TinyPngOptions
{
    public const string SectionName = "TinyPng";
    public string? ApiKey { get; set; }
    public string ApiEndPoint { get; set; } = "https://api.tinify.com";
}