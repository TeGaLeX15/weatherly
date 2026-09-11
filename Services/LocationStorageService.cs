// Services/LocationStorageService.cs
using System.Text.Json;
using Weatherly.Models;

namespace Weatherly.Services;

public sealed class LocationStorageService
{
    private readonly string _filePath;

    public LocationStorageService()
    {
        var directory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Weatherly");

        Directory.CreateDirectory(directory);

        _filePath =
            Path.Combine(
                directory,
                "location.json");
    }

    public async Task<Location?> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            await using var stream =
                File.OpenRead(_filePath);

            return await JsonSerializer.DeserializeAsync<Location>(
                stream,
                cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public async Task SaveAsync(
        Location location,
        CancellationToken cancellationToken = default)
    {
        await using var stream =
            File.Create(_filePath);

        await JsonSerializer.SerializeAsync(
            stream,
            location,
            cancellationToken: cancellationToken);
    }
}