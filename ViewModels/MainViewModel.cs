// ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Weatherly.Models;
using Weatherly.Services;

namespace Weatherly.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly HttpClient _httpClient;
    private readonly GeocodingService _geocodingService;
    private readonly WeatherService _weatherService;
    private readonly ForecastAnalyzer _forecastAnalyzer;

    private bool _isLoading;
    private string _errorMessage = string.Empty;

    private string _cityName = "Щучинск";
    private string _dateText = string.Empty;
    private string _currentTemperature = "--°";
    private string _currentCondition = "Загрузка...";
    private string _feelsLike = string.Empty;
    private string _todayMinMax = string.Empty;
    private string _wind = string.Empty;
    private string _humidity = string.Empty;
    private string _sunrise = string.Empty;
    private string _sunset = string.Empty;

    private string _analysisText = string.Empty;
    private string _bestWalkTime = string.Empty;
    private string _clothingText = string.Empty;
    private string _weatherTip = string.Empty;

    private string _currentIconPath =
        "/Assets/Weather/overcast.svg";

    public ObservableCollection<HourlyWeatherItem> Hourly { get; } = [];

    public ObservableCollection<DailyWeatherItem> Daily { get; } = [];

    public string CityName
    {
        get => _cityName;
        private set => SetField(ref _cityName, value);
    }

    public string DateText
    {
        get => _dateText;
        private set => SetField(ref _dateText, value);
    }

    public string CurrentTemperature
    {
        get => _currentTemperature;
        private set => SetField(ref _currentTemperature, value);
    }

    public string CurrentCondition
    {
        get => _currentCondition;
        private set => SetField(ref _currentCondition, value);
    }

    public string FeelsLike
    {
        get => _feelsLike;
        private set => SetField(ref _feelsLike, value);
    }

    public string TodayMinMax
    {
        get => _todayMinMax;
        private set => SetField(ref _todayMinMax, value);
    }

    public string Wind
    {
        get => _wind;
        private set => SetField(ref _wind, value);
    }

    public string Humidity
    {
        get => _humidity;
        private set => SetField(ref _humidity, value);
    }

    public string Sunrise
    {
        get => _sunrise;
        private set => SetField(ref _sunrise, value);
    }

    public string Sunset
    {
        get => _sunset;
        private set => SetField(ref _sunset, value);
    }

    public string AnalysisText
    {
        get => _analysisText;
        private set => SetField(ref _analysisText, value);
    }

    public string BestWalkTime
    {
        get => _bestWalkTime;
        private set => SetField(ref _bestWalkTime, value);
    }

    public string ClothingText
    {
        get => _clothingText;
        private set => SetField(ref _clothingText, value);
    }

    public string WeatherTip
    {
        get => _weatherTip;
        private set => SetField(ref _weatherTip, value);
    }

    public string CurrentIconPath
    {
        get => _currentIconPath;
        private set => SetField(ref _currentIconPath, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetField(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    public MainViewModel()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        _geocodingService =
            new GeocodingService(_httpClient);

        _weatherService =
            new WeatherService(_httpClient);

        _forecastAnalyzer =
            new ForecastAnalyzer();
    }

    public async Task LoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var locations =
                await _geocodingService.SearchAsync(
                    "Щучинск",
                    cancellationToken: cancellationToken);

            var location = locations.FirstOrDefault();

            if (location is null)
            {
                throw new InvalidOperationException(
                    "Не удалось найти город Щучинск.");
            }

            var forecast =
                await _weatherService.GetForecastAsync(
                    location,
                    cancellationToken);

            UpdateWeather(forecast);
        }
        catch (OperationCanceledException)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                ErrorMessage =
                    "Загрузка погоды была прервана.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Не удалось загрузить погоду: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateWeather(
        WeatherForecast forecast)
    {
        var current = forecast.Current;

        CityName =
            forecast.Location.Name;

        DateText =
            current.Time.ToString(
                "dddd, d MMMM",
                new System.Globalization.CultureInfo("ru-RU"));

        CurrentTemperature =
            $"{Math.Round(current.Temperature):0}°";

        CurrentCondition =
            WeatherCodeMapper.GetDescription(
                current.WeatherCode,
                current.IsDay);

        FeelsLike =
            $"Ощущается как {Math.Round(current.ApparentTemperature):0}°";

        var today =
            forecast.Daily.FirstOrDefault();

        if (today is not null)
        {
            TodayMinMax =
                $"{Math.Round(today.TemperatureMin):0}° / " +
                $"{Math.Round(today.TemperatureMax):0}°";

            Sunrise =
                today.Sunrise.ToString("HH:mm");

            Sunset =
                today.Sunset.ToString("HH:mm");
        }

        Wind =
            $"{Math.Round(current.WindSpeed):0} м/с";

        Humidity =
            $"{current.RelativeHumidity}%";

        CurrentIconPath =
            WeatherCodeMapper.GetIconPath(
                current.WeatherCode,
                current.IsDay);

        UpdateHourly(forecast.Hourly);
        UpdateDaily(forecast.Daily);

        var analysis =
            _forecastAnalyzer.Analyze(forecast);

        AnalysisText =
            analysis.Summary;

        BestWalkTime =
            analysis.BestWalkTime;

        ClothingText =
            analysis.Clothing;

        WeatherTip =
            $"{analysis.Tip} {analysis.TemperatureTrend}";
    }

    private void UpdateHourly(
        IReadOnlyList<HourlyWeather> hourly)
    {
        Hourly.Clear();

        var startIndex =
            hourly
                .Select((weather, index) =>
                    new { weather, index })
                .FirstOrDefault(x =>
                    x.weather.Time >= DateTime.Now)?
                .index ?? 0;

        foreach (var weather in hourly
                     .Skip(startIndex)
                     .Take(7))
        {
            Hourly.Add(
                new HourlyWeatherItem(weather));
        }
    }

    private void UpdateDaily(
        IReadOnlyList<DailyWeather> daily)
    {
        Daily.Clear();

        foreach (var weather in daily.Take(7))
        {
            Daily.Add(
                new DailyWeatherItem(weather));
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private void SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(
                field,
                value))
        {
            return;
        }

        field = value;

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class HourlyWeatherItem
{
    public string Time { get; }

    public string Temperature { get; }

    public string PrecipitationProbability { get; }

    public string Wind { get; }

    public string Humidity { get; }

    public string IconPath { get; }

    public HourlyWeatherItem(
        HourlyWeather weather)
    {
        Time =
            weather.Time.ToString("HH:mm");

        Temperature =
            $"{Math.Round(weather.Temperature):0}°";

        PrecipitationProbability =
            $"{Math.Round(weather.PrecipitationProbability):0}%";

        Wind =
            $"{Math.Round(weather.WindSpeed):0} м/с";

        Humidity =
            $"{weather.RelativeHumidity}%";

        IconPath =
            WeatherCodeMapper.GetIconPath(
                weather.WeatherCode,
                weather.IsDay);
    }
}

public sealed class DailyWeatherItem
{
    public string Day { get; }

    public string Date { get; }

    public string Temperature { get; }

    public double TemperatureMax { get; }

    public double TemperatureMin { get; }

    public string PrecipitationProbability { get; }

    public string IconPath { get; }

    public DailyWeatherItem(
        DailyWeather weather)
    {
        Day =
            weather.Date.ToString(
                "dddd",
                new System.Globalization.CultureInfo(
                    "ru-RU"));

        Date =
            weather.Date.ToString(
                "d MMMM",
                new System.Globalization.CultureInfo(
                    "ru-RU"));

        TemperatureMax =
            weather.TemperatureMax;

        TemperatureMin =
            weather.TemperatureMin;

        Temperature =
            $"{Math.Round(weather.TemperatureMin):0}° / " +
            $"{Math.Round(weather.TemperatureMax):0}°";

        PrecipitationProbability =
            $"{Math.Round(weather.PrecipitationProbability):0}%";

        IconPath =
            WeatherCodeMapper.GetIconPath(
                weather.WeatherCode,
                true);
    }
}

public static class WeatherCodeMapper
{
    public static string GetDescription(
        int code,
        bool isDay)
    {
        return code switch
        {
            0 => isDay
                ? "Ясно"
                : "Ясная ночь",

            1 or 2 => isDay
                ? "Переменная облачность"
                : "Переменная облачность",

            3 => "Пасмурно",

            45 or 48 => "Туман",

            51 or 53 or 55 or 56 or 57 =>
                "Морось",

            61 or 63 =>
                "Дождь",

            65 =>
                "Сильный дождь",

            66 or 67 =>
                "Дождь со снегом",

            71 or 73 or 75 or 77 =>
                "Снег",

            80 or 81 or 82 =>
                "Ливень",

            85 or 86 =>
                "Снегопад",

            95 =>
                "Гроза",

            96 or 99 =>
                "Гроза с градом",

            _ =>
                "Неизвестно"
        };
    }

    public static string GetIconPath(
        int code,
        bool isDay)
    {
        return code switch
        {
            0 => isDay
                ? "/Assets/Weather/clear-day.svg"
                : "/Assets/Weather/clear-night.svg",

            1 or 2 => isDay
                ? "/Assets/Weather/partly-cloudy-day.svg"
                : "/Assets/Weather/partly-cloudy-night.svg",

            3 =>
                "/Assets/Weather/overcast.svg",

            45 or 48 =>
                "/Assets/Weather/fog.svg",

            51 or 53 or 55 or 56 or 57 =>
                "/Assets/Weather/drizzle.svg",

            61 or 63 =>
                "/Assets/Weather/rain.svg",

            65 or 80 or 81 or 82 =>
                "/Assets/Weather/heavy-rain.svg",

            66 or 67 =>
                "/Assets/Weather/sleet.svg",

            71 or 73 or 75 or 77 or 85 or 86 =>
                "/Assets/Weather/snow.svg",

            95 or 96 or 99 =>
                "/Assets/Weather/thunderstorms.svg",

            _ =>
                "/Assets/Weather/overcast.svg"
        };
    }
}