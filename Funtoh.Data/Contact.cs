namespace Funtoh.Data;

public enum ContactPublicity
{
    Private,
    Collaborators,
    Public
}

public record Contact(string ServiceName, string Value, int? FollowerCount, ContactPublicity Publicity)
{
    private static IEnumerable<string> ServiceNames => new[]
    {
        "Email",
        "Facebook",
        "Instagram",
        "LinkedIn",
        "Phone",
        "Reddit", 
        "Snapchat",
        "Threads",
        "Tiktok",
        "Tumblr",
        "Twitch",
        "Twitter",
        "Vevo",
        "WeChat",
        "WhatsApp",
        "YouTube",
    };

    public static Contact Empty { get; } = new Contact("Phone", "", null, ContactPublicity.Private);
}