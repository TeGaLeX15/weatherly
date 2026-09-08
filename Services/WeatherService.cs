// Services/WeatherService.cs
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Weatherly.Models;

namespace Weatherly.Services;

public sealed class WeatherService
{
    private const string BaseUrl =
        "https://api.open-meteo.com/v1/forecast";

    private readonly HttpClient _httpClient;

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherForecast> GetForecastAsync(
        Location location,
        CancellationToken cancellationToken = default)
    {
        var latitude = location.Latitude.ToString(
            CultureInfo.InvariantCulture);

        var longitude = location.Longitude.ToString(
            CultureInfo.InvariantCulture);

        var url =
            $"{BaseUrl}" +
            $"?latitude={latitude}" +
            $"&longitude={longitude}" +
            "&current=" +
            "temperature_2m," +
            "apparent_temperature," +
            "relative_humidity_2m," +
            "wind_speed_10m," +
            "weather_code," +
            "is_day" +
            "&hourly=" +
            "temperature_2m," +
            "apparent_temperature," +
            "relative_humidity_2m," +
            "wind_speed_10m," +
            "precipitation_probability," +
            "precipitation," +
            "weather_code," +
            "is_day" +
            "&daily=" +
            "temperature_2m_max," +
            "temperature_2m_min," +
            "apparent_temperature_max," +
            "apparent_temperature_min," +
            "precipitation_sum," +
            "precipitation_probability_max," +
            "weather_code," +
            "sunrise," +
            "sunset" +
            "&temperature_unit=celsius" +
            "&wind_speed_unit=ms" +
            "&precipitation_unit=mm" +
            "&timezone=auto" +
            "&forecast_days=7";

        var response =
            await _httpClient.GetFromJsonAsync<WeatherApiResponse>(
                url,
                cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException(
                "Open-Meteo вернул пустой ответ.");
        }

        return MapForecast(location, response);
    }

    private static WeatherForecast MapForecast(
        Location location,
        WeatherApiResponse response)
    {
        var current = MapCurrent(response);

        var hourly = MapHourly(response);

        var daily = MapDaily(response);

        return new WeatherForecast
        {
            Location = location,
            Current = current,
            Hourly = hourly,
            Daily = daily
        };
    }

    private static CurrentWeather MapCurrent(
        WeatherApiResponse response)
    {
        var current = response.Current
            ?? throw new InvalidOperationException(
                "В ответе отсутствует current.");

        return new CurrentWeather
        {
            Time = ParseDateTime(current.Time),
            Temperature = current.Temperature,
            ApparentTemperature = current.ApparentTemperature,
            RelativeHumidity = current.RelativeHumidity,
            WindSpeed = current.WindSpeed,
            WeatherCode = current.WeatherCode,
            IsDay = current.IsDay == 1
        };
    }

    private static List<HourlyWeather> MapHourly(
        WeatherApiResponse response)
    {
        var hourly = response.Hourly
            ?? throw new InvalidOperationException(
                "В ответе отсутствует hourly.");

        var count = hourly.Time.Count;

        var result = new List<HourlyWeather>(count);

        for (var i = 0; i < count; i++)
        {
            result.Add(new HourlyWeather
            {
                Time = ParseDateTime(hourly.Time[i]),
                Temperature = hourly.Temperature[i],
                ApparentTemperature = hourly.ApparentTemperature[i],
                RelativeHumidity = hourly.RelativeHumidity[i],
                WindSpeed = hourly.WindSpeed[i],
                PrecipitationProbability =
                    hourly.PrecipitationProbability[i],
                Precipitation = hourly.Precipitation[i],
                WeatherCode = hourly.WeatherCode[i],
                IsDay = hourly.IsDay[i] == 1
            });
        }

        return result;
    }

    private static List<DailyWeather> MapDaily(
        WeatherApiResponse response)
    {
        var daily = response.Daily
            ?? throw new InvalidOperationException(
                "В ответе отсутствует daily.");

        var count = daily.Time.Count;

        var result = new List<DailyWeather>(count);

        for (var i = 0; i < count; i++)
        {
            result.Add(new DailyWeather
            {
                Date = DateTime.Parse(
                    daily.Time[i],
                    CultureInfo.InvariantCulture),

                TemperatureMax = daily.TemperatureMax[i],

                TemperatureMin = daily.TemperatureMin[i],

                ApparentTemperatureMax =
                    daily.ApparentTemperatureMax[i],

                ApparentTemperatureMin =
                    daily.ApparentTemperatureMin[i],

                Precipitation = daily.Precipitation[i],

                PrecipitationProbability =
                    daily.PrecipitationProbability[i],

                WeatherCode = daily.WeatherCode[i],

                Sunrise = ParseDateTime(
                    daily.Sunrise[i]),

                Sunset = ParseDateTime(
                    daily.Sunset[i])
            });
        }

        return result;
    }

    private static DateTime ParseDateTime(string value)
    {
        return DateTime.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
    }


    private sealed class WeatherApiResponse
    {
        [JsonPropertyName("current")]
        public CurrentResponse? Current { get; init; }

        [JsonPropertyName("hourly")]
        public HourlyResponse? Hourly { get; init; }

        [JsonPropertyName("daily")]
        public DailyResponse? Daily { get; init; }
    }


    private sealed class CurrentResponse
    {
        [JsonPropertyName("time")]
        public string Time { get; init; } = string.Empty;

        [JsonPropertyName("temperature_2m")]
        public double Temperature { get; init; }

        [JsonPropertyName("apparent_temperature")]
        public double ApparentTemperature { get; init; }

        [JsonPropertyName("relative_humidity_2m")]
        public int RelativeHumidity { get; init; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; init; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; init; }

        [JsonPropertyName("is_day")]
        public int IsDay { get; init; }
    }


    private sealed class HourlyResponse
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; init; } = [];

        [JsonPropertyName("temperature_2m")]
        public List<double> Temperature { get; init; } = [];

        [JsonPropertyName("apparent_temperature")]
        public List<double> ApparentTemperature { get; init; } = [];

        [JsonPropertyName("relative_humidity_2m")]
        public List<int> RelativeHumidity { get; init; } = [];

        [JsonPropertyName("wind_speed_10m")]
        public List<double> WindSpeed { get; init; } = [];

        [JsonPropertyName("precipitation_probability")]
        public List<double> PrecipitationProbability { get; init; } = [];

        [JsonPropertyName("precipitation")]
        public List<double> Precipitation { get; init; } = [];

        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode { get; init; } = [];

        [JsonPropertyName("is_day")]
        public List<int> IsDay { get; init; } = [];
    }


    private sealed class DailyResponse
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; init; } = [];

        [JsonPropertyName("temperature_2m_max")]
        public List<double> TemperatureMax { get; init; } = [];

        [JsonPropertyName("temperature_2m_min")]
        public List<double> TemperatureMin { get; init; } = [];

        [JsonPropertyName("apparent_temperature_max")]
        public List<double> ApparentTemperatureMax { get; init; } = [];

        [JsonPropertyName("apparent_temperature_min")]
        public List<double> ApparentTemperatureMin { get; init; } = [];

        [JsonPropertyName("precipitation_sum")]
        public List<double> Precipitation { get; init; } = [];

        [JsonPropertyName("precipitation_probability_max")]
        public List<double> PrecipitationProbability { get; init; } = [];

        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode { get; init; } = [];

        [JsonPropertyName("sunrise")]
        public List<string> Sunrise { get; init; } = [];

        [JsonPropertyName("sunset")]
        public List<string> Sunset { get; init; } = [];
    }
}