namespace TinyPng;

public enum ImageType
{
    Jpeg,
    Png,
    WebP
}

public static class ImageTypes
{
    public static string GetMimeType(ImageType imageType) => imageType switch
    {
        ImageType.Jpeg => "image/jpeg",
        ImageType.WebP => "image/webp",
        ImageType.Png => "image/png",
        _ => throw new ArgumentOutOfRangeException(nameof(imageType), imageType, "Unsupported ImageType")
    };
}