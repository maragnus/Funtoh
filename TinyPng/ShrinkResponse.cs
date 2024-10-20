namespace TinyPng;

public class ErrorResponse
{
    [JsonPropertyName("error")] public string? Error { get; set; }

    [JsonPropertyName("message")] public string? Message { get; set; }
}

public abstract class ShrinkResponse
{
    public string ImageResourceUrl { get; }
    public int? CompressionCount { get; }
    public int? Width { get; }
    public int? Height { get; }

    protected ShrinkResponse(string imageResourceUrl, ShrinkHeaders headers)
    {
        ImageResourceUrl = imageResourceUrl;
        CompressionCount = headers.CompressionCount;
        Width = headers.ImageWidth;
        Height = headers.ImageHeight;
    }
}

[PublicAPI]
public class ShrinkDownloadResponse : ShrinkResponse
{
    public ShrinkDownloadResponse(string imageResourceUrl, ShrinkHeaders headers, byte[] imageData)
        : base(imageResourceUrl, headers)
    {
        ImageData = imageData;
        Type = headers.ContentType;
    }

    public byte[] ImageData { get; }
    public string? Type { get; }
}

[PublicAPI]
public class ShrinkAwsStoreResponse : ShrinkResponse
{
    public ShrinkAwsStoreResponse(string imageResourceUrl, ShrinkHeaders headers)
        : base(imageResourceUrl, headers)
    {
        Url = headers.Location ?? throw new TinyPngException("Location header expected");
    }

    public string Url { get; }
}

[PublicAPI]
public class ShrinkGcsStoreResponse : ShrinkResponse
{
    public ShrinkGcsStoreResponse(string imageResourceUrl, ShrinkHeaders headers)
        : base(imageResourceUrl, headers)
    {
        Url = headers.Location ?? throw new TinyPngException("Location header expected");
    }

    public string Url { get; }
}

public class ShrinkHeaders
{
    public int? CompressionCount { get; set; }
    public int? ImageWidth { get; set; }
    public int? ImageHeight { get; set; }
    public string? ContentType { get; set; }
    public string? Location { get; set; }
}