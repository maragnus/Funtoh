using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TinyPng;

public class TinyPngClient
{
    private readonly ITinyPngHttpClient _httpClient;
    private readonly ILogger<TinyPngClient> _logger;

    public TinyPngClient(ITinyPngHttpClient httpClient, ILogger<TinyPngClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public TinyPngClient(string apiKey)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.tinify.com");
        client.DefaultRequestHeaders.Authorization = TinyPngClient.BuildAuthorizationHeader(apiKey);
        _httpClient = new TinyPngHttpClient(client);
        _logger = NullLogger<TinyPngClient>.Instance;
    }

    public ShrinkOperation Shrink(string fileName) => new ShrinkOperation(this, _logger).FromFileName(fileName);
    public ShrinkOperation Shrink(Uri uri) => new ShrinkOperation(this, _logger).FromUrl(uri);
    public ShrinkOperation Shrink(Stream stream) => new ShrinkOperation(this, _logger).FromStream(stream);
    public ShrinkOperation Shrink(byte[] imageData) => new ShrinkOperation(this, _logger).FromBytes(imageData);

    internal async Task<HttpResponseMessage> SendShrinkOperation(string imageResourceUrl, HttpContent content,
        CancellationToken cancellationToken)
    {
        return await _httpClient.SendRequest(imageResourceUrl, content, cancellationToken).ConfigureAwait(false);
    }

    internal static AuthenticationHeaderValue BuildAuthorizationHeader(string apiKey)
    {
        var authValue = $"api:{apiKey}";
        var apiKeyEncoded = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(authValue));
        return new AuthenticationHeaderValue("basic", apiKeyEncoded);
    }
}