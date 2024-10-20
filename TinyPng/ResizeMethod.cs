namespace TinyPng;

public enum ResizeMethod
{
    Scale,
    Fit,
    Cover,
    Thumb
}

public static class ResizeMethods
{
    public static string Translate(ResizeMethod resizeMethod) => resizeMethod switch
    {
        ResizeMethod.Scale => "scale",
        ResizeMethod.Fit => "fit",
        ResizeMethod.Cover => "cover",
        ResizeMethod.Thumb => "thumb",
        _ => throw new ArgumentOutOfRangeException(nameof(resizeMethod), resizeMethod, "Unsupported ResizeMethod")
    };
}