using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using TinyPng.Request;

namespace TinyPng;

[PublicAPI]
public class ShrinkOperation
{
    private readonly TinyPngClient _client;
    private readonly ILogger _logger;
    private readonly ShrinkRequest _request = new();
    private HttpContent? _httpContent;
    private string? _sourceFileName;
    private SemaphoreSlim _semaphore = new SemaphoreSlim(1);
    private ShrinkUploadResponse? _uploadResponse;

    internal ShrinkOperation(TinyPngClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }

    internal ShrinkOperation FromFileName(string fileName)
    {
        _sourceFileName = fileName;
        return this;
    }

    internal ShrinkOperation FromBytes(byte[] bytes)
    {
        _httpContent = new ByteArrayContent(bytes);
        return this;
    }

    internal ShrinkOperation FromStream(Stream stream)
    {
        _httpContent = new StreamContent(stream);
        return this;
    }

    internal ShrinkOperation FromUrl(Uri url)
    {
        _httpContent = JsonContent.Create(
            new UploadRequest
            {
                Source = new Source
                {
                    Url = url.ToString()
                }
            });
        return this;
    }

    public ShrinkOperation Convert(params ImageType[] imageTypes)
    {
        _request.Convert = imageTypes.Length switch
        {
            0 => null,
            1 => new ConvertType() { Type = ImageTypes.GetMimeType(imageTypes[0]) },
            _ => new ConvertTypes() { Types = imageTypes.Select(ImageTypes.GetMimeType).ToArray() },
        };
        return this;
    }

    public ShrinkOperation PreserveMetadata(params Metadata[] preserveMetadata)
    {
        _request.PreserveMetadata = preserveMetadata.Select(TinyPng.PreserveMetadata.Translate).ToArray();
        return this;
    }

    public ShrinkOperation Transform(string color)
    {
        _request.Transform = new Transform
        {
            Background = color
        };
        return this;
    }

    public ShrinkOperation Resize(ResizeMethod resizeMethod, int? width, int? height)
    {
        _request.Resize = new Resize
        {
            Method = ResizeMethods.Translate(resizeMethod),
            Width = width,
            Height = height
        };
        return this;
    }

    /// <summary>Upload source image to TinyPNG</summary>
    /// <returns>Resource url to used to perform operations</returns>
    /// <remarks>The source image is only uploaded once. Future uploads are ignored.</remarks>
    public async ValueTask<ShrinkUploadResponse> Upload(CancellationToken cancellationToken = default)
    {
        if (_uploadResponse != null) return _uploadResponse;

        if (_sourceFileName != null)
        {
            var bytes = await File.ReadAllBytesAsync(_sourceFileName, cancellationToken).ConfigureAwait(false);
            _httpContent = new ByteArrayContent(bytes);
            _sourceFileName = null;
        }

        if (_httpContent == null)
            throw new InvalidOperationException("No image source provided");
        
        using var httpContent = _httpContent;
        _httpContent = null;
        using var response = await _client.SendShrinkOperation("/shrink", httpContent, cancellationToken).ConfigureAwait(false);
        
        _uploadResponse = await response.Content.ReadFromJsonAsync<ShrinkUploadResponse>(options: null, cancellationToken).ConfigureAwait(false)
            ?? throw new TinyPngException();
        _uploadResponse.Output!.Url = response.Headers.Location!.ToString()
            ?? throw new TinyPngException();
        
        return _uploadResponse;
    }

    private static ShrinkHeaders ParseHeaders(HttpResponseMessage response)
    {

        int? ParseHeader(string header) =>
            response.Headers.TryGetValues(header, out var values)
                && int.TryParse(values.FirstOrDefault() ?? "", out var parsedValue)
                ? parsedValue : null;

        return new ShrinkHeaders
        {
            ContentType = response.Content.Headers.ContentType?.MediaType,
            CompressionCount = ParseHeader("Compression-Count"),
            ImageWidth = ParseHeader("Image-Width"),
            ImageHeight = ParseHeader("Image-Height"),
            Location = response.Headers.Location?.ToString()
        };
    }

    private async Task<HttpResponseMessage> Store(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Store: {Json}", JsonSerializer.Serialize(_request, new JsonSerializerOptions { WriteIndented = true }));
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await Upload(cancellationToken).ConfigureAwait(false);
            using var content = JsonContent.Create(_request);
            var response =
                await _client.SendShrinkOperation(_uploadResponse!.Output!.Url!, content, cancellationToken)
                    .ConfigureAwait(false);
            return response;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>Upload source image and downloads compressed version</summary>
    /// <remarks>The source image is only uploaded once. <see cref="ShrinkOperation"/> can be reused to perform more operations on the same source image.</remarks>
    public async Task<ShrinkDownloadResponse> Download(CancellationToken cancellationToken = default)
    {
        _request.Store = null;
        using var response = await Store(cancellationToken).ConfigureAwait(false);
        var headers = ParseHeaders(response);
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        return new ShrinkDownloadResponse(_uploadResponse!.Output!.Url!, headers, bytes);
    }

    /// <summary>Upload source image and stores compressed version in Amazon S3</summary>
    /// <remarks>The source image is only uploaded once. <see cref="ShrinkOperation"/> can be reused to perform more operations on the same source image.</remarks>
    public async Task<ShrinkAwsStoreResponse> AwsStore(AwsStore store, CancellationToken cancellationToken = default)
    {
        _request.Store = store;
        using var response = await Store(cancellationToken).ConfigureAwait(false);
        var headers = ParseHeaders(response);
        return new ShrinkAwsStoreResponse(_uploadResponse!.Output!.Url!, headers);
    }

    /// <summary>Upload source image and stores compressed version in Google Cloud Storage</summary>
    /// <remarks>The source image is only uploaded once. <see cref="ShrinkOperation"/> can be reused to perform more operations on the same source image.</remarks>
    public async Task<ShrinkGcsStoreResponse> GcsStore(GcsStore store, CancellationToken cancellationToken = default)
    {
        _request.Store = store;
        using var response = await Store(cancellationToken).ConfigureAwait(false);
        var headers = ParseHeaders(response);
        return new ShrinkGcsStoreResponse(_uploadResponse!.Output!.Url!, headers);
    }
}
