namespace TinyPng;

public enum Metadata
{
    Copyright,
    Creation,
    Location
}

public static class PreserveMetadata
{
    public static string Translate(Metadata metadata) => metadata switch
    {
        Metadata.Copyright => "copyright",
        Metadata.Creation => "creation",
        Metadata.Location => "location",
        _ => throw new ArgumentOutOfRangeException(nameof(metadata), metadata, null)
    };
}