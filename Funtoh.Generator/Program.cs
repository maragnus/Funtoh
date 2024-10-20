using System.Text;
using Funtoh.Generator;
using OpenAI.Chat;
using TinyPng;

const string JsonPath = @"W:\Funtoh\Data\";

var generator = new Generator();

if (false)
{
    await Serialize<Brand>("Brand", "Please list 25 real and prominent brands that would benefit from partnering with social media influencers, street performers, and artists for publicity. Provide a two or three paragraph description for the brand. The description must introduce the brand and include details on what that brand expects from promoters.");
}

if (false)
{
    await Serialize<Profile>("Sponsor", 
        "Please create a fictitious Sponsor user profile for each brand. They are a representative of that brand and will be hiring promoters to promote it.",
        "Description should be written in their tone and based on their level of professionalism to introduce themself to promoters, how they work.",
        GetBrands().ToCommaDelimited(b => b.Name));
}

if (false)
{
    await Serialize<Specialty>("Specialty", "I need to create some promoters. Can you help me come up with a thorough list of 75 specialties for promoters that would be useful to sponsors? Examples: artistic (chalk murals, spray painting), social media (frequent content creation, prominent following), social (attends children's sports, attends social clubs, public speaker), commuter");
}

if (false)
{
    await SerializeRepeat<Profile>("Promoter", 5,
        "Please create 10 fictitious, diverse promoter user profiles. Each profile needs three to six specialties that apply to them.",
        "Description should be written in the user's tone and their level of professionalism to introduce themself to sponsors and can include emojis. It should be a sales pitch to Sponsors and how their skills can benefit a brand.",
        "List of contacts should include all the social media accounts that apply to their specialty.",
        GetSpecialty().GroupBy(s=>s.Category).Select(b => $"{b.Key} specialties: {b.ToCommaDelimited(s => s.Name)}").ToLineDelimited());
}


if (false)
{
    var promoters = GetPromoters();
    var hustleOutput = new List<PromoterHustle>(100);
    Random.Shared.Shuffle(promoters);
    foreach (var promoter in promoters.Take(30))
    {
        Console.WriteLine($"Generating hustles for {promoter.FullName}");
        var items = await generator.GenerateList<PromoterHustle>($"Create one, two, or three hustles for {promoter.FullName} based on their specialties.",
            "Descriptions are written in their tone, personality, and level of professionalism to say what they're offering, and how their skills will benefit the brand and emojis are allowed.",
            "Hustles can be a single delivery like an art project, or an ongoing project that lasts days to months such as social media campaign.",
            "Hustles must have a Default option, but may also include Improved, or Elaborate options to offer sponsors more premium deliverables.",
            "Hustles can have one or more phases based on complexity. Not all phases require a price.",
            "For each option, each phases must provide deliverables, proof of work, and optionally a cost.",
            "Requirements lists what is needed from the Sponsor for the promoter to do their phase.",
            "Deliverables lists what the promoter will provide over the course of the phase.",
            "Proof of work lists what the promoter will provide to keep the sponsor updated on the promoter's progress.",
            $"{promoter.FullName} Profile: ",
            $"Age: {promoter.Age}",
            $"{promoter.Specialties.ToCommaDelimited(s => $"{s.Name} ({s.Category})")}",
            $"Personality: {promoter.Personality}",
            $"Ethnicity: {promoter.Ethnicity}",
            $"Pronouns: {promoter.Pronouns}"
        );
        hustleOutput.AddRange(items);
    }
    await File.WriteAllTextAsync($@"{JsonPath}\PromoterHustle.json", JsonSerializer.Serialize(hustleOutput, Generator.JsonSerializerOptions));
}

if (false)
{
    var sponsors = GetSponsors();
    var specialties = GetSpecialty();
    var hustleOutput = new List<SponsorHustle>(100);
    Random.Shared.Shuffle(sponsors);
    foreach (var sponsor in sponsors.Take(20))
    {
        var brand = sponsor.Brands.FirstOrDefault();
        Console.WriteLine($"Generating hustles for {sponsor.FullName} {brand}");
        var items = await generator.GenerateList<SponsorHustle>($"Create one, two, or three hustles for the brand {brand.Name} written by the brand sponsor, {sponsor.FullName}, using the provided specialties.",
            "Descriptions are written in the sponsor's tone, personality, and level of professionalism to say what they're offering, and how their skills will benefit the brand and emojis are allowed.",
            "Hustles can be a single delivery like an art project, or an ongoing project that lasts days to months such as social media campaign.",
            "Hustles must have a Default option, but may also include Improved, or Elaborate options to offer sponsors more premium deliverables.",
            "Hustles can have one or more phases based on complexity. Not all phases require a price.",
            "For each option, each phases must provide deliverables, proof of work, and optionally a cost.",
            "Requirements lists the prerequisites that a promoter must meet to be accepted for this hustle.",
            "Deliverables lists what the promoter will provide over the course of the phase.",
            "Proof of work lists what the promoter will provide to keep the sponsor updated on the promoter's progress.",
            "", $"Brand Description: {brand.Description}",
            "", $"{sponsor.FullName}'s Profile: ",
            $"Age: {sponsor.Age}",
            $"Personality: {sponsor.Personality}",
            $"Ethnicity: {sponsor.Ethnicity}",
            $"Pronouns: {sponsor.Pronouns}",
            "", "Specialties:",
            $"{specialties.ToCommaDelimited(s => $"{s.Name} ({s.Category})")}"
        );
        hustleOutput.AddRange(items);
    }
    await File.WriteAllTextAsync($@"{JsonPath}\SponsorHustle.json", JsonSerializer.Serialize(hustleOutput, Generator.JsonSerializerOptions));
}

if (false)
{
    var reviews = new List<Review>(100);
    var sponsors = GetSponsors().ToDictionary(p => p.FullName!);
    var promoters = GetPromoters();
    var hustles = GetSponsorHustles();
    Random.Shared.Shuffle(promoters);
    
    foreach (var promoter in promoters.Take(15))
    {
        Console.WriteLine($"Generating reviews by {promoter.FullName}");
        Random.Shared.Shuffle(hustles);
        var hustleCount = Random.Shared.Next(1, 3);
        foreach (var hustle in hustles.Take(hustleCount))
        {
            if (!sponsors.TryGetValue(hustle.SponsorName!, out var sponsor))
            {
                Console.WriteLine($"Sponsor {hustle.SponsorName} could not be found for Hustle {hustle.Title}");
                continue;
            }
            
            var stars = Math.Min(Random.Shared.Next(3, 7), 5);
            var review = await generator.Generate<ReviewPair>(
                $@"The sponsor, {sponsor.FullName}, hired promoter, {promoter.FullName}, for the ""{hustle.Title}"" hustle.",
                $"In the voice of {sponsor.FullName}, write a {stars}-star review of {promoter.FullName} in reviewOfPromoter",
                $"And in the voice of {promoter.FullName}, write a {stars}-star review of {sponsor.FullName} in reviewOfSponsor",
                "Reviews may contain emojis if the writer would use them.", 
                "", $"{sponsor.FullName}'s Sponsor Profile: ",
                $"Age: {sponsor.Age}",
                $"Personality: {sponsor.Personality}",
                $"Ethnicity: {sponsor.Ethnicity}",
                "", $"{promoter.FullName}'s Promoter Profile: ",
                $"Age: {promoter.Age}",
                $"Pronouns: {promoter.Pronouns}",
                $"Specialties: {promoter.Specialties.ToCommaDelimited(s => $"{s.Name} ({s.Category})")}",
                $"Personality: {promoter.Personality}",
                $"Ethnicity: {promoter.Ethnicity}",
                "", "Hustle profile:",
                $"Title: {hustle.Title}",
                $"Description: {hustle.Description}",
                $"Requirements: {hustle.Requirements}",
                $"Deliverables: {hustle.Phases.SelectMany(x=>x.Options.SelectMany(y=>y.Deliverables)).ToCommaDelimited()}",
                "");

            var reviewDate = DateTime.Now.ToUniversalTime().Date.AddDays(-Random.Shared.Next(10, 365));
            reviews.Add(new Review(promoter.FullName!, sponsor.FullName!, ReviewerType.Sponsor, hustle.Title, reviewDate, review.ReviewOfPromoter.Stars, review.ReviewOfPromoter.Comment));
            reviews.Add(new Review(sponsor.FullName!, promoter.FullName!, ReviewerType.Promoter, hustle.Title, reviewDate, review.ReviewOfSponsor.Stars, review.ReviewOfSponsor.Comment));
        }
    }

    await File.WriteAllTextAsync($@"{JsonPath}\SponsorReviews.json",
        JsonSerializer.Serialize(reviews.GroupBy(x => x.ReviewOf).ToDictionary(x => x.Key),
            Generator.JsonSerializerOptions));
}

// Merge generated files into the DataFile.json
if (false)
{
    await DataContext.BuildDataFile();

    var context = new DataContext();
    Console.WriteLine(context.Profiles.Count());
}

if (false)
{
    var context = new DataContext();
    var request = context.Profiles
        .Where(profile => string.IsNullOrWhiteSpace(profile.Pronouns))
        .Select(profile => new Person(profile.FullName, profile.Age, ""))
        .ToArray();
    var result = await generator.GenerateList<Person>("Please assign pronouns to these people:", JsonSerializer.Serialize(request.ToList(), Generator.JsonSerializerOptions));
    foreach (var person in result)
    {
        context.Profiles.FirstOrDefault(p=>p.FullName == person.Name)!.Pronouns = person.Pronouns;
    }
    context.SaveDataFile();
}

if (false)
{
    var context = new DataContext();
    foreach (var profile in context.Profiles)
    {
        var profilePath = @$"C:\Work\Funtoh\Data\GeneratedProfiles\{profile.ProfileId}.png";

        if (File.Exists(profilePath)) continue;
        
        var pronouns = profile.Pronouns!.ToLower();
        var personality = profile.Personality!.ToLower();
        var gender =pronouns .Contains("her") ? "woman" : pronouns.Contains("him") ? "man" : "non-binary";
        var occupations = await generator.QuickGenerate<OccupationResponse>(
            $"List most likely occupations of a {profile.Ethnicity} {gender} with these specialties:",
            profile.Specialties.ToCommaDelimited(s => $"{s.Name} ({s.Category})"));
        var occupation = occupations.Occupations.First().ToLower();
        
        var sb = new StringBuilder();
        sb.AppendLine($"I NEED to test how the tool works with extremely simple prompts. DO NOT add any detail, just use it AS-IS:");
        sb.AppendLine($"Photograph of a {personality}, {profile.Age}-year-old, {profile.Ethnicity} {gender} dressed as a {occupation} utilizing the entire 1024x1024 space.");
        // sb.AppendLine("The image should not contain any text, labels, borders, measurements nor design elements of any kind.");
        await generator.GenerateImage(profilePath, sb.ToString(), ImageSize.Square, ImageQuality.HighDefinition);
    }
}

if (false)
{
    var context = new DataContext();
    foreach (var hustle in context.Hustles)
    {
        var imagePath = @$"C:\Work\Funtoh\Data\Hustles\{hustle.HustleId}.png";
        if (File.Exists(imagePath)) continue;

        var response = await generator.QuickGenerate<DeliverableResponse>(
            $"""Describe a simple, engaging scene from this description of the "{hustle.Title}" hustle:""", hustle.Description!);
        
        var sb = new StringBuilder(response.Deliverables.FirstOrDefault());
        // sb.AppendLine($"I NEED to test how the tool works with extremely simple prompts. DO NOT add any detail, just use it AS-IS:");
        // sb.Append($"Simple cover photo for this hustle: ");
        // sb.AppendLine($"{response.Deliverables.FirstOrDefault()}");
        // sb.AppendLine("The image must be a simple photograph.");
        await generator.GenerateImage(imagePath, sb.ToString(), ImageSize.Landscape, ImageQuality.Standard);
    }
}

if (false)
{
    var tinyPng = new TinyPngClient("9jLSW686GCwqJLGb6K17dvBwzpB458jW");
    foreach (var image in Directory.GetFiles(@"C:\Work\Funtoh\Data\GeneratedProfiles"))
    {
        var destination = $@"C:\Work\Funtoh\Data\Profiles\{Path.GetFileName(image)}";
        if (File.Exists( destination)) continue;
        
        var response = await tinyPng.Shrink(image).Resize(ResizeMethod.Cover, 256, 256).Download();
        await File.WriteAllBytesAsync(destination, response.ImageData);
    }
}

if (true)
{
    var tinyPng = new TinyPngClient("9jLSW686GCwqJLGb6K17dvBwzpB458jW");
    foreach (var image in Directory.GetFiles(@"C:\Work\Funtoh\Data\GeneratedHustles"))
    {
        var destination = $@"C:\Work\Funtoh\Data\Hustles\{Path.GetFileNameWithoutExtension(image)}.jpg";
        if (File.Exists( destination)) continue;
        
        var response = await tinyPng.Shrink(image).Convert(ImageType.Jpeg).Download();
        await File.WriteAllBytesAsync(destination, response.ImageData);
    }
}

Brand[] GetBrands() => DeserializeFile<Brand>();
Specialty[] GetSpecialty() => DeserializeFile<Specialty>();
Profile[] GetSponsors() => DeserializeFile<Profile>("Sponsor");
Profile[] GetPromoters() => DeserializeFile<Profile>("Promoter");
PromoterHustle[] GetPromoterHustles() => DeserializeFile<PromoterHustle>();
SponsorHustle[] GetSponsorHustles() => DeserializeFile<SponsorHustle>();

async Task Serialize<T>(string name, params string[] prompts)
{
    Console.WriteLine($"Generating {name}");
    var items = await generator.GenerateList<T>(prompts);
    Console.WriteLine("Saving");
    await File.WriteAllTextAsync($@"{JsonPath}\{name}.json", JsonSerializer.Serialize(items, Generator.JsonSerializerOptions));
}

async Task SerializeRepeat<T>(string name, int repeat, params string[] prompts)
{
    var items = new List<T>(100);
    for (var i = 0; i < repeat; i++)
    {
        Console.WriteLine($"Generating {name} ({i + 1} of {repeat})");
        Console.WriteLine("Saving");
        items.AddRange(await generator.GenerateList<T>(prompts));
    }

    await File.WriteAllTextAsync($@"{JsonPath}\{name}.json", JsonSerializer.Serialize(items, Generator.JsonSerializerOptions));
}

T[] DeserializeFile<T>(string? name = null)
{
    using var stream = File.OpenRead($@"{JsonPath}\{name ?? typeof(T).Name}.json");
    return JsonSerializer.Deserialize<T[]>(stream, Generator.JsonSerializerOptions) ?? throw new Exception("Failed to load brands");
}

record Brand(string Name, string Description);

record ReviewPair(ReviewItem ReviewOfSponsor, ReviewItem ReviewOfPromoter);
record ReviewItem(int Stars, string Comment);

record Person(string Name, int Age, string Pronouns);

record OccupationResponse(string[] Occupations);
record DeliverableResponse(string[] Deliverables);
