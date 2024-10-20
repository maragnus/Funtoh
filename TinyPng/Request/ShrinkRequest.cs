namespace TinyPng.Request;

[PublicAPI]
public class ShrinkUploadResponse
{
    [JsonPropertyName("output")] public Output? Output { get; set; }

}

[PublicAPI]
public class Output
{
    [JsonPropertyName("size")] public int? Size { get; set; }

    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonIgnore] public string? Url { get; set; }
}

[PublicAPI]
public class UploadRequest
{
    [JsonPropertyName("source")] public Source? Source { get; set; }
}

[PublicAPI]
public class Source
{
    [JsonPropertyName("url")] public string? Url { get; set; }
}

[PublicAPI]
public class ShrinkRequest
{
    [JsonPropertyName("convert")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Convert? Convert { get; set; }

    [JsonPropertyName("transform")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Transform? Transform { get; set; }

    [JsonPropertyName("preserve")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? PreserveMetadata { get; set; }

    [JsonPropertyName("resize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Resize? Resize { get; set; }

    [JsonPropertyName("store")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Store? Store { get; set; }
}

[PublicAPI]
[JsonDerivedType(typeof(ConvertType))]
[JsonDerivedType(typeof(ConvertTypes))]
public abstract class Convert
{
}

[PublicAPI]
public class ConvertType : Convert
{
    [JsonPropertyName("type")] public string? Type { get; set; }
}

[PublicAPI]
public class ConvertTypes : Convert
{
    [JsonPropertyName("type")] public string[]? Types { get; set; }
}

[PublicAPI]
public class Transform
{
    [JsonPropertyName("background")] public string? Background { get; set; }
}

[PublicAPI]
public class Resize
{
    [JsonPropertyName("method")] public string Method { get; set; } = "";

    [JsonPropertyName("width")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public int? Height { get; set; }
}

[PublicAPI]
[JsonDerivedType(typeof(AwsStore))]
[JsonDerivedType(typeof(GcsStore))]
public abstract class Store
{
    [JsonPropertyName("service")] public string? Service { get; set; }
}

[PublicAPI]
public class AwsStore : Store
{
    public AwsStore()
    {
        Service = "aws";
    }

    [JsonPropertyName("aws_access_key_id")]
    public string? AwsAccessKeyId { get; set; }

    [JsonPropertyName("aws_secret_access_key")]
    public string? AwsSecretAccessKey { get; set; }

    [JsonPropertyName("region")] public string? Region { get; set; }

    [JsonPropertyName("path")] public string? Path { get; set; }
}

[PublicAPI]
public class GcsStore : Store
{
    public GcsStore()
    {
        Service = "gcs";
    }

    [JsonPropertyName("gcp_access_token")] public string? GcpAccessToken { get; set; }

    [JsonPropertyName("path")] public string? Path { get; set; }
}