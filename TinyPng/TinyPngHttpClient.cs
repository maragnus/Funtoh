using System.Net.Http.Json;

namespace TinyPng;


public interface ITinyPngHttpClient
{
    Task<HttpResponseMessage> SendRequest(string url, HttpContent content,
        CancellationToken cancellationToken);
}

internal class TinyPngHttpClient : ITinyPngHttpClient
{
    private readonly HttpClient _httpClient;

    public TinyPngHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> SendRequest(string url, HttpContent content,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode) 
            return response;
        
        var errorResult =
            await response.Content.ReadFromJsonAsync<ErrorResponse>(options: null, cancellationToken).ConfigureAwait(false)
            ?? new ErrorResponse
            {
                Error = response.StatusCode.ToString(),
                Message = "Unexpected response from Tinify service"
            };
        throw new TinyPngException(errorResult);
    }
}