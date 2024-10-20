using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Funtoh.Data;

public class DataContext
{
    private List<Profile> _profiles = new(100);
    private List<Hustle> _hustles = new(100);
    private Dictionary<Guid, Profile> _profileById;
    private Dictionary<string, Profile> _profileByName;
    private Dictionary<Guid, Hustle> _hustlesById;
    private Dictionary<string, Hustle> _hustlesByName;

    private readonly static JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    public IEnumerable<Profile> Profiles => _profiles;
    public IEnumerable<Hustle> Hustles => _hustles;

    private record DataFile(Profile[] Profiles, Hustle[] Hustles);

    public DataContext()
    {
        try
        {
            LoadDataFile();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load data file: {ex.Message}");
        }
    }

    public void LoadDataFile()
    {
        var type = typeof(DataContext);
        var fileName = $"{type.Namespace}.DataFile.json";
        using var file = type.Assembly.GetManifestResourceStream(fileName) ?? throw new Exception($"Could not access resource {fileName}");
        // using var file = File.OpenRead(@"W:\Funtoh\Data\DataFile.json") ?? throw new Exception("Could not open data file");
        var dataFile = JsonSerializer.Deserialize<DataFile>(file, JsonOptions) ??
                       throw new Exception("Could not deserialize data file");
        _hustles.AddRange(dataFile.Hustles);
        _profiles.AddRange(dataFile.Profiles);
        _profileById = dataFile.Profiles.ToDictionary(p => p.ProfileId);
        _profileByName = dataFile.Profiles.ToDictionary(p => p.FullName);
        _hustlesById = dataFile.Hustles.ToDictionary(h => h.HustleId);
        _hustlesByName = dataFile.Hustles.ToDictionary(h => h.Title);

        foreach (var profile in _profiles)
        {
            profile.Location = "Philadelphia, PA";
            profile.HustleIds = _hustles
                .Where(h => h.OwnerProfileId == profile.ProfileId)
                .Select(h => h.HustleId)
                .ToArray();
            profile.Review = profile.Reviews.Length == 0 ? 0 : Math.Round(profile.Reviews.Average(r => r.Stars), 1);
            profile.ReviewCount = profile.Reviews.Length;
            foreach (var review in profile.Reviews) 
                review.ReviewByProfile = _profileByName[review.ReviewBy];
            profile.Hustles = profile.HustleIds.Select(id => _hustlesById[id]).ToArray();
        }

        foreach (var hustle in _hustles)
        {
            hustle.OwnerProfile = _profileById[hustle.OwnerProfileId];
            if (hustle.Type == HustleType.Sponsor && hustle.Brand == null)
                hustle.Brand = hustle.OwnerProfile.Brands.First().Name;
        }
    }

    public void SaveDataFile()
    {
        using (var file = File.Create(@$"W:\Funtoh\Data\DataFile.json") ??
                          throw new Exception($"Could write data file"))
            JsonSerializer.Serialize(file, new DataFile(_profiles.ToArray(), _hustles.ToArray()), JsonOptions);
    }

    private static T[] LoadItems<T>(string name)
    {
        var type = typeof(DataContext);
        var fileName = $"{type.Namespace}.{name}.json";
        // using var file = type.Assembly.GetManifestResourceStream(fileName) ?? throw new Exception($"Could not access resource {fileName}");
        using var file = File.OpenRead(@$"W:\Funtoh\Data\{name}.json") ??
                         throw new Exception($"Could not access resource {fileName}");
        return JsonSerializer.Deserialize<T[]>(file, JsonOptions)
               ?? throw new Exception($"Could not deserialize {fileName}");
    }

    private static T[] LoadDictionary<T>(string name)
    {
        var type = typeof(DataContext);
        var fileName = $"{type.Namespace}.{name}.json";
        // using var file = type.Assembly.GetManifestResourceStream(fileName) ?? throw new Exception($"Could not access resource {fileName}");
        using var file = File.OpenRead(@$"W:\Funtoh\Data\{name}.json") ??
                         throw new Exception($"Could not access resource {fileName}");
        var dict = JsonSerializer.Deserialize<Dictionary<string, T[]>>(file, JsonOptions)
                   ?? throw new Exception($"Could not deserialize {fileName}");
        return dict.Values.SelectMany(x => x).ToArray();
    }

    private static T[] Concat<T>(params T[][] arrays)
    {
        var list = new List<T>(arrays.Sum(x => x.Length));
        foreach (var array in arrays)
            list.AddRange(array);
        return list.ToArray();
    }

    public static async Task BuildDataFile()
    {
        var profiles = new List<Profile>();
        profiles.AddRange(LoadItems<Profile>("Promoter"));
        profiles.AddRange(LoadItems<Profile>("Sponsor"));
        var profileByName = profiles.ToDictionary(p => p.FullName);

        foreach (var profile in profiles)
            profile.ProfileId = Guid.NewGuid();

        var hustles = new List<Hustle>();
        foreach (var hustle in LoadItems<PromoterHustle>("PromoterHustle"))
        {
            var owner = profileByName[hustle.PromoterName];
            hustles.Add(new Hustle
            {
                HustleId = Guid.NewGuid(),
                OwnerProfileId = owner.ProfileId,
                OwnerProfile = owner,
                Type = HustleType.Promoter,
                Title = hustle.Title,
                Description = hustle.Description,
                Requirements = hustle.Requirements,
                Options = hustle.Options,
                Phases = hustle.Phases,
                MinimumPrice = hustle.MinimumPrice,
                MaximumPrice = hustle.MaximumPrice,
            });
        }

        foreach (var hustle in LoadItems<SponsorHustle>("SponsorHustle"))
        {
            var owner = profileByName[hustle.SponsorName];
            hustles.Add(new Hustle
            {
                HustleId = Guid.NewGuid(),
                OwnerProfileId = owner.ProfileId,
                OwnerProfile = owner,
                Type = HustleType.Sponsor,
                Title = hustle.Title,
                Description = hustle.Description,
                Requirements = hustle.Requirements,
                Options = hustle.Options,
                Phases = hustle.Phases,
                MinimumPrice = hustle.MinimumPrice,
                MaximumPrice = hustle.MaximumPrice,
            });
        }

        var reviews = Concat(LoadDictionary<Review>("PromoterReviews"), LoadDictionary<Review>("SponsorReviews"));

        foreach (var profile in profiles)
        {
            profile.HustleIds = hustles.Where(h => h.OwnerProfile == profile).Select(h => h.HustleId).ToArray();
            profile.Reviews = reviews.Where(r => r.ReviewOf == profile.FullName).ToArray();
        }

        await using (var file = File.Create(@$"W:\Funtoh\Data\DataFile.json") ??
                                throw new Exception($"Could write data file"))
            await JsonSerializer.SerializeAsync(file, new DataFile(profiles.ToArray(), hustles.ToArray()), JsonOptions);
    }

    public Profile FindProfile(Guid profileId)
    {
        if (_profileById.TryGetValue(profileId, out var profile)) return profile;
        return Profile.Empty;
    }
    
    public Profile FindProfile(string userName)
    {
        if (_profileByName.TryGetValue(userName, out var profile)) return profile;
        return Profile.Empty;
    }

    public Hustle FindHustle(Guid hustleId)
    {
        if (_hustlesById.TryGetValue(hustleId, out var hustle)) return hustle;
        return Hustle.Empty;
    }
    
    public Hustle FindHustle(string userName)
    {
        if (_hustlesByName.TryGetValue(userName, out var hustle)) return hustle;
        return Hustle.Empty;
    }
}