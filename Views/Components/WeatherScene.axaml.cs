// Views/Components/WeatherScene.axaml.cs
using Avalonia.Controls;
using Weatherly.Models;

namespace Weatherly.Views.Components;

public partial class WeatherScene : UserControl
{
    public WeatherCondition Condition { get; private set; }

    public WeatherScene()
    {
        InitializeComponent();

        Condition = WeatherCondition.Cloudy;
    }

    public void SetWeather(WeatherCondition condition)
    {
        Condition = condition;

        WeatherIcon.Path = GetIconPath(condition);
    }

    private static string GetIconPath(WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Clear =>
                "/Assets/Weather/clear-day.svg",

            WeatherCondition.PartlyCloudy =>
                "/Assets/Weather/partly-cloudy-day.svg",

            WeatherCondition.Cloudy =>
                "/Assets/Weather/overcast.svg",

            WeatherCondition.Drizzle =>
                "/Assets/Weather/drizzle.svg",

            WeatherCondition.Rain =>
                "/Assets/Weather/rain.svg",

            WeatherCondition.HeavyRain =>
                "/Assets/Weather/heavy-rain.svg",

            WeatherCondition.Snow =>
                "/Assets/Weather/snow.svg",

            WeatherCondition.Sleet =>
                "/Assets/Weather/sleet.svg",

            WeatherCondition.Storm =>
                "/Assets/Weather/thunderstorms.svg",

            WeatherCondition.Fog =>
                "/Assets/Weather/fog.svg",

            WeatherCondition.Night =>
                "/Assets/Weather/clear-night.svg",

            _ =>
                "/Assets/Weather/overcast.svg"
        };
    }
}