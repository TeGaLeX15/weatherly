// Models/WeatherAnalysis.cs
namespace Weatherly.Models;

public sealed class WeatherAnalysis
{
    public string Summary { get; init; } = string.Empty;

    public string BestWalkTime { get; init; } = string.Empty;

    public string Clothing { get; init; } = string.Empty;

    public string Tip { get; init; } = string.Empty;

    public string ActivitySuitability { get; init; } = string.Empty;

    public string RainWarning { get; init; } = string.Empty;

    public string TemperatureTrend { get; init; } = string.Empty;

    public bool IsGoodForWalk { get; init; }

    public bool ShouldTakeUmbrella { get; init; }
}