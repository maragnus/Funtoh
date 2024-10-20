using System.Text.Json.Serialization;

namespace Funtoh.Data;

public enum ReviewerType
{
    General,
    Sponsor,
    Promoter
}

public record Review(
    string ReviewOf,
    string ReviewBy,
    ReviewerType ReviewByType,
    string? HustleTitle,
    DateTime Posted,
    int Stars,
    string? Comment)
{
    [JsonIgnore]
    public Profile ReviewByProfile { get; set; } = Profile.Empty;
};