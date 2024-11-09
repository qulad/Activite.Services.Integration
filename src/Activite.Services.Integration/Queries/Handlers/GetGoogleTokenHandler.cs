using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Activite.Services.Integration.DTOs;
using Convey.CQRS.Queries;
using Microsoft.Extensions.Logging;

namespace Activite.Services.Integration.Queries.Handlers;

public class GetGoogleTokenHandler : IQueryHandler<GetGoogleToken, GoogleTokenDto>
{
    private const string HttpClientName = nameof(GetGoogleTokenHandler);
    private const string ContentTypeValue = "application/json";
    private const string UrlTemplate = "https://oauth2.googleapis.com/tokeninfo?access_token={{accessToken}}";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetGoogleTokenHandler> _logger;

    public GetGoogleTokenHandler(IHttpClientFactory httpClientFactory, ILogger<GetGoogleTokenHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<GoogleTokenDto> HandleAsync(GetGoogleToken query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrEmpty(query.AccessToken);

        using var httpClient = _httpClientFactory.CreateClient(HttpClientName);

        var url = UrlTemplate.Replace("{{accessToken}}", query.AccessToken);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url)
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response?.Content is null)
        {
            _logger.LogError("Response content is null");

            throw new ArgumentException("Response content is null");
        }

        if (response.Content?.Headers?.ContentType?.MediaType is not ContentTypeValue)
        {
            _logger.LogError("Invalid ContentType");

            throw new ArgumentException("Invalid ContentType");
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrEmpty(responseBody))
        {
            _logger.LogError("ResponseBody is null");

            throw new ArgumentException("Received response is null");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorResult = JsonSerializer.Deserialize<ErrorResponse>(responseBody);

            ArgumentNullException.ThrowIfNull(errorResult);

            return new GoogleTokenDto
            {
                ErrorMessage = errorResult.ErrorMessage
            };
        }

        var result = JsonSerializer.Deserialize<Response>(responseBody);

        ArgumentNullException.ThrowIfNull(result);

        var googleToken = new GoogleTokenDto
        {
            AuthorizedParty = result.AuthorizedParty,
            Audience = result.Audience,
            Subject = result.Subject,
            Scope = result.Scope,
            Email = result.Email,
            ErrorMessage = string.Empty
        };

        return googleToken;
    }
}

sealed file class ErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("error_description")]
    public string ErrorMessage { get; set; }
}

sealed file class Response
{
    [JsonPropertyName("azp")]
    public string AuthorizedParty { get; set; }

    [JsonPropertyName("aud")]
    public string Audience { get; set; }

    [JsonPropertyName("sub")]
    public string Subject { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}