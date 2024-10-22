namespace Funtoh.Data;

public enum ContactPublicity
{
    Private,
    Collaborators,
    Public
}

public record Contact(string ServiceName, string Value, int? FollowerCount, ContactPublicity Publicity)
{
    public static IEnumerable<string> ServiceNames => new HashSet<string>
    {
        "Bluesky", "Discord", "Email", "Facebook", "Flickr", "Github", "Google+", "IMDB", "Instagram", "Kickstarter",
        "LinkedIn", "Medium", "Meetup", "OpenId", "Patreon", "PayPal", "Phone", "Pinterest", "Playstation", "Reddit",
        "Skype", "Snapchat", "Soundcloud", "Spotify", "StackExchange", "StackOverflow", "Steam", "Stripe", "Threads",
        "Tiktok", "Tumblr", "Twitch", "Twitter", "Upwork", "VK", "Vimeo", "WeChat", "Weibo", "WhatsApp", "Wordpress",
        "Yelp", "YouTube", "Xbox"
    };

    public static Contact Empty { get; } = new Contact("Phone", "", null, ContactPublicity.Private);
}