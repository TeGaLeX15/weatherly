// Services/GeocodingService.cs
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Weatherly.Models;

namespace Weatherly.Services;

public sealed class GeocodingService
{
    private const string BaseUrl =
        "https://geocoding-api.open-meteo.com/v1/search";

    private readonly HttpClient _httpClient;

    public GeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Location>> SearchAsync(
        string query,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var url =
            $"{BaseUrl}" +
            $"?name={Uri.EscapeDataString(query.Trim())}" +
            $"&count={count}" +
            "&language=ru" +
            "&format=json";

        var response =
            await _httpClient.GetFromJsonAsync<GeocodingResponse>(
                url,
                cancellationToken);

        if (response?.Results is null)
        {
            return [];
        }

        return response.Results
            .Select(MapLocation)
            .ToList();
    }

    private static Location MapLocation(GeocodingResult result)
    {
        return new Location
        {
            Id = result.Id,
            Name = result.Name ?? string.Empty,
            Country = result.Country ?? string.Empty,
            CountryCode = result.CountryCode ?? string.Empty,
            Admin1 = result.Admin1 ?? string.Empty,
            Latitude = result.Latitude,
            Longitude = result.Longitude,
            Elevation = result.Elevation,
            Timezone = result.Timezone ?? string.Empty
        };
    }

    private sealed class GeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<GeocodingResult>? Results { get; init; }
    }

    private sealed class GeocodingResult
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("elevation")]
        public double Elevation { get; init; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; init; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; init; }

        [JsonPropertyName("country")]
        public string? Country { get; init; }

        [JsonPropertyName("admin1")]
        public string? Admin1 { get; init; }
    }
}