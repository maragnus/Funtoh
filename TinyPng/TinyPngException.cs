namespace TinyPng;

public class TinyPngException : Exception
{
    public string? Error { get;}

    public TinyPngException(ErrorResponse errorResult) : base(errorResult.Message)
    {
        Error = errorResult.Error;
    }
    
    public TinyPngException() : base("Unexpected response from Tinify service") {}

    public TinyPngException(string message) : base(message) {}

}