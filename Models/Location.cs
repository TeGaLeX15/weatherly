// Models/Location.cs
namespace Weatherly.Models;

public sealed class Location
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Country { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;

    public string Admin1 { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double Elevation { get; init; }

    public string Timezone { get; init; } = string.Empty;
}