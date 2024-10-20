using System.Text.Json.Serialization;

namespace Funtoh.Data;

public enum HustleType
{
    Sponsor,
    Promoter
}

public class Hustle
{
    public static Hustle Empty { get; } = new()
    {
        Type = HustleType.Sponsor,
        Title = "Loading...",
        OwnerProfile = Profile.Empty
    };
    
    public Guid HustleId { get; set; }
    [JsonIgnore]
    public Profile OwnerProfile { get; set; }
    public Guid OwnerProfileId { get; set; }
    public required HustleType Type { get; set; }
    public required string Title { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string[] Requirements { get; set; } = [];
    public HustlePhase[] Phases { get; set; } = [];
    public decimal? MinimumPrice { get; set; }
    public decimal? MaximumPrice { get; set; }
    public HustleOption[] Options { get; set; } = [];
    public string[] Categories { get; set; } = [];
}

public class ActiveHustle
{
    public required Profile Sponsor { get; set; }
    public required Profile Promoter { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string[] Requirements { get; set; } = [];
    public HustlePhase[] Phases { get; set; } = [];
    public decimal? MinimumPrice { get; set; }
    public decimal? MaximumPrice { get; set; }
    public HustleOption[] Options { get; set; } = [];
}

public class PromoterHustle
{
    public required string PromoterName { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string[] Requirements { get; set; } = [];
    public HustlePhase[] Phases { get; set; } = [];
    public decimal? MinimumPrice { get; set; }
    public decimal? MaximumPrice { get; set; }
    public HustleOption[] Options { get; set; } = [];
}

public class SponsorHustle
{
    public required string SponsorName { get; set; }
    public required string Brand { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string[] Requirements { get; set; } = [];
    public HustlePhase[] Phases { get; set; } = [];
    public decimal? MinimumPrice { get; set; }
    public decimal? MaximumPrice { get; set; }
    public HustleOption[] Options { get; set; } = [];
}

public enum HustleOptionLevel
{
    Default,
    Improved,
    Elaborate
}

public record HustleOption(HustleOptionLevel Level, string Title, string Description);

public class HustlePhase
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DeliverableSchedule DeliverySchedule { get; set; }
    public int? DeliveryIterations { get; set; }
    public int? MaximumIterations { get; set; }
    public HustlePhaseOption[] Options { get; set; } = [];
}

public class HustlePhaseOption
{
    public HustleOptionLevel Level { get; set; }
    public decimal? Price { get; set; }
    public string[] Deliverables { get; set; } = [];
    public string[] ProofOfWork { get; set; } = [];
}

public enum DeliverableSchedule
{
    Single,
    Daily,
    Weekly,
    Monthly,
}
