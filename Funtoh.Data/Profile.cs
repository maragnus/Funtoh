using System.Text.Json.Serialization;

namespace Funtoh.Data;

public class Profile
{
    public Guid ProfileId { get; set; }
    public required string FullName { get; set; }
    public string? Ethnicity { get; set; }
    public string? Personality { get; set; }
    public string? Pronouns { get; set; }
    public Specialty[] Specialties { get; set; } = [];
    public ProfileLevel Level { get; set; }
    public int Age { get; set; }
    // public DateTime? DateOfBirth { get; set; }
    // public DateTime LastActive { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public Contact[] Contacts { get; set; } = [];
    public int HustlesPromoted { get; set; }
    public int HustlesSponsored { get; set; }
    public Review[] Reviews { get; set; } = [];
    public Brand[] Brands { get; set; } = [];
    
    public Guid[] HustleIds { get; set; } = [];
    [JsonIgnore]
    public Hustle[] Hustles { get; set; } = [];

    public double Review { get; set; }
    public int ReviewCount { get; set; }

    public static Profile Empty { get; } = new() { FullName = "Loading..."};
}

public record Brand(string Name, string? Description);

public enum SpecialtyCategory
{
    Artistic,
    SocialMedia,
    SocialEngagement,
    Commuter,
    HealthAndFitness,
    MusicAndEntertainment,
    EducationAndKnowledgeSharing,
    FoodAndBeverage,
    Sustainability
}

public record Specialty(string Name, SpecialtyCategory Category);