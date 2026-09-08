// Services/ForecastAnalyzer.cs
using Weatherly.Models;

namespace Weatherly.Services;

public sealed class ForecastAnalyzer
{
    public WeatherAnalysis Analyze(WeatherForecast forecast)
    {
        var current = forecast.Current;
        var hourly = forecast.Hourly;
        var daily = forecast.Daily;

        return new WeatherAnalysis
        {
            Summary = BuildSummary(current, hourly),
            BestWalkTime = BuildBestWalkTime(hourly),
            Clothing = BuildClothing(current),
            Tip = BuildTip(current, hourly),
            ActivitySuitability = BuildActivitySuitability(current, hourly),
            RainWarning = BuildRainWarning(hourly),
            TemperatureTrend = BuildTemperatureTrend(daily),
            IsGoodForWalk = IsGoodForWalk(hourly),
            ShouldTakeUmbrella = ShouldTakeUmbrella(hourly)
        };
    }

    private static string BuildSummary(
        CurrentWeather current,
        IReadOnlyList<HourlyWeather> hourly)
    {
        var temperature = Math.Round(current.Temperature);

        if (IsStorm(current.WeatherCode))
        {
            return $"Сейчас гроза, около {temperature}°. Лучше пока оставаться в помещении.";
        }

        if (IsHeavyRain(current.WeatherCode))
        {
            return $"Сейчас сильный дождь, около {temperature}°. Для прогулки погода неблагоприятная.";
        }

        if (IsRain(current.WeatherCode))
        {
            return $"Сейчас дождь, около {temperature}°. Если собираетесь выйти, лучше взять зонт.";
        }

        if (IsSnow(current.WeatherCode))
        {
            return $"Сейчас снег, около {temperature}°. На улице может быть скользко.";
        }

        if (IsFog(current.WeatherCode))
        {
            return $"Сейчас туман, около {temperature}°. Видимость может быть снижена.";
        }

        if (current.WindSpeed >= 12)
        {
            return $"Сейчас около {temperature}° и довольно ветрено. На улице может ощущаться заметно холоднее.";
        }

        if (current.Temperature >= 25)
        {
            return $"Сейчас около {temperature}°. Довольно тепло — лучше избегать долгого пребывания под прямым солнцем.";
        }

        if (current.Temperature <= 0)
        {
            return $"Сейчас около {temperature}°. На улице холодно, стоит одеться теплее.";
        }

        if (hourly.Any(h => h.PrecipitationProbability >= 60))
        {
            return $"Сейчас около {temperature}°. В ближайшие часы возможны осадки.";
        }

        return $"Сейчас около {temperature}°. Погода в целом спокойная.";
    }

    private static string BuildBestWalkTime(
        IReadOnlyList<HourlyWeather> hourly)
    {
        var candidates = hourly
            .Where(h =>
                h.PrecipitationProbability < 30 &&
                h.WindSpeed < 9 &&
                h.Temperature > -15 &&
                h.Temperature < 30 &&
                h.IsDay)
            .OrderBy(h => WalkScore(h))
            .ToList();

        if (candidates.Count == 0)
        {
            return "Подходящего времени для прогулки сегодня почти нет.";
        }

        var best = candidates.First();

        var endTime = best.Time.AddHours(1);

        return $"Лучшее время для прогулки — " +
               $"{best.Time:HH:mm}–{endTime:HH:mm}. " +
               $"Около {Math.Round(best.Temperature)}°, " +
               $"вероятность осадков {Math.Round(best.PrecipitationProbability)}%.";
    }

    private static double WalkScore(HourlyWeather weather)
    {
        var temperatureScore =
            Math.Abs(weather.Temperature - 18);

        var rainScore =
            weather.PrecipitationProbability * 0.15;

        var windScore =
            weather.WindSpeed * 0.5;

        return temperatureScore +
               rainScore +
               windScore;
    }

    private static string BuildClothing(
        CurrentWeather current)
    {
        var temperature = current.Temperature;
        var wind = current.WindSpeed;

        if (temperature >= 28)
        {
            return "Лёгкая одежда. Лучше выбрать что-нибудь дышащее и не забыть про защиту от солнца.";
        }

        if (temperature >= 22)
        {
            return "Лёгкая одежда будет достаточно комфортной.";
        }

        if (temperature >= 15)
        {
            if (wind >= 8)
            {
                return "Лёгкая куртка или ветровка пригодится из-за ветра.";
            }

            return "Лёгкая куртка или кофта — оптимальный вариант.";
        }

        if (temperature >= 8)
        {
            return "Лучше надеть куртку или тёплую кофту.";
        }

        if (temperature >= 0)
        {
            return "Нужна тёплая куртка. Если планируете долго быть на улице, пригодится дополнительный слой.";
        }

        if (temperature >= -10)
        {
            return "Оденьтесь тепло: зимняя куртка, тёплая обувь и несколько слоёв одежды.";
        }

        return "Очень холодно. Нужна полноценная зимняя одежда, включая тёплую обувь, шапку и перчатки.";
    }

    private static string BuildTip(
        CurrentWeather current,
        IReadOnlyList<HourlyWeather> hourly)
    {
        if (IsStorm(current.WeatherCode))
        {
            return "Во время грозы лучше не находиться на открытой местности.";
        }

        if (ShouldTakeUmbrella(hourly))
        {
            return "Возьмите зонт — вероятность осадков в ближайшие часы заметная.";
        }

        if (current.WindSpeed >= 12)
        {
            return "На улице сильный ветер. Одевайтесь с учётом того, что будет ощущаться холоднее.";
        }

        if (current.Temperature >= 25)
        {
            return "Если выходите надолго, лучше выбрать место в тени и взять воду.";
        }

        if (current.Temperature <= 0)
        {
            return "Осторожнее на дорогах и тротуарах: после осадков возможен гололёд.";
        }

        return "Погода подходит для обычных дел и прогулки.";
    }

    private static string BuildActivitySuitability(
        CurrentWeather current,
        IReadOnlyList<HourlyWeather> hourly)
    {
        if (IsStorm(current.WeatherCode))
        {
            return "Неблагоприятно для активностей на улице.";
        }

        if (current.WindSpeed >= 15)
        {
            return "Неблагоприятно для активностей на улице из-за сильного ветра.";
        }

        if (hourly.Any(h => h.PrecipitationProbability >= 70))
        {
            return "Лучше планировать активность в помещении или выбрать сухое окно.";
        }

        if (current.Temperature >= 10 &&
            current.Temperature <= 24 &&
            current.WindSpeed < 10)
        {
            return "Очень хорошие условия для активности на улице.";
        }

        return "В целом подходящие условия, но стоит учитывать температуру и ветер.";
    }

    private static string BuildRainWarning(
        IReadOnlyList<HourlyWeather> hourly)
    {
        var rainHours = hourly
            .Where(h => h.PrecipitationProbability >= 50)
            .OrderBy(h => h.Time)
            .ToList();

        if (rainHours.Count == 0)
        {
            return "Значительных осадков в ближайшие часы не ожидается.";
        }

        var first = rainHours.First();
        var last = rainHours.Last();

        if (first.Time.Hour == last.Time.Hour)
        {
            return $"Вероятны осадки около {first.Time:HH:mm}.";
        }

        return $"Осадки наиболее вероятны примерно с {first.Time:HH:mm} до {last.Time:HH:mm}.";
    }

    private static string BuildTemperatureTrend(
        IReadOnlyList<DailyWeather> daily)
    {
        if (daily.Count < 2)
        {
            return "Недостаточно данных для определения тренда.";
        }

        var today = daily[0];
        var tomorrow = daily[1];

        var difference =
            tomorrow.TemperatureMax -
            today.TemperatureMax;

        if (difference >= 5)
        {
            return "Завтра станет заметно теплее.";
        }

        if (difference >= 2)
        {
            return "Завтра немного потеплеет.";
        }

        if (difference <= -5)
        {
            return "Завтра станет заметно холоднее.";
        }

        if (difference <= -2)
        {
            return "Завтра немного похолодает.";
        }

        return "Температура в ближайшие дни сильно не изменится.";
    }

    private static bool IsGoodForWalk(
        IReadOnlyList<HourlyWeather> hourly)
    {
        return hourly.Any(h =>
            h.IsDay &&
            h.PrecipitationProbability < 40 &&
            h.WindSpeed < 12 &&
            h.Temperature > -15 &&
            h.Temperature < 30);
    }

    private static bool ShouldTakeUmbrella(
        IReadOnlyList<HourlyWeather> hourly)
    {
        return hourly.Any(h =>
            h.PrecipitationProbability >= 40 ||
            h.Precipitation >= 0.5);
    }

    private static bool IsRain(int code)
    {
        return code is
            51 or 53 or 55 or 56 or 57 or
            61 or 63 or
            66 or 67 or
            80 or 81 or 82;
    }

    private static bool IsHeavyRain(int code)
    {
        return code is 65 or 82;
    }

    private static bool IsSnow(int code)
    {
        return code is
            71 or 73 or 75 or 77 or
            85 or 86;
    }

    private static bool IsStorm(int code)
    {
        return code is 95 or 96 or 99;
    }

    private static bool IsFog(int code)
    {
        return code is 45 or 48;
    }
}