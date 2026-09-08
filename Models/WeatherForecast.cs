// Models/WeatherForecast.cs
namespace Weatherly.Models;

public sealed class WeatherForecast
{
    public Location Location { get; init; } = new();

    public CurrentWeather Current { get; init; } = new();

    public List<HourlyWeather> Hourly { get; init; } = [];

    public List<DailyWeather> Daily { get; init; } = [];
}

public sealed class CurrentWeather
{
    public DateTime Time { get; init; }

    public double Temperature { get; init; }

    public double ApparentTemperature { get; init; }

    public int RelativeHumidity { get; init; }

    public double WindSpeed { get; init; }

    public int WeatherCode { get; init; }

    public bool IsDay { get; init; }
}

public sealed class HourlyWeather
{
    public DateTime Time { get; init; }

    public double Temperature { get; init; }

    public double ApparentTemperature { get; init; }

    public int RelativeHumidity { get; init; }

    public double WindSpeed { get; init; }

    public double PrecipitationProbability { get; init; }

    public double Precipitation { get; init; }

    public int WeatherCode { get; init; }

    public bool IsDay { get; init; }
}

public sealed class DailyWeather
{
    public DateTime Date { get; init; }

    public double TemperatureMax { get; init; }

    public double TemperatureMin { get; init; }

    public double ApparentTemperatureMax { get; init; }

    public double ApparentTemperatureMin { get; init; }

    public double Precipitation { get; init; }

    public double PrecipitationProbability { get; init; }

    public int WeatherCode { get; init; }

    public DateTime Sunrise { get; init; }

    public DateTime Sunset { get; init; }
}