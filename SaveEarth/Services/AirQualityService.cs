using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace SaveEarth.Services;

public record AirQualityResult(int UsAqi, double Pm25);

public static class AirQualityService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Geocoding: cari kota by name, kembalikan (name, lat, lon) atau null
    public static async Task<(string City, double Lat, double Lon)?> FindCityAsync(string name)
    {
        try
        {
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(name)}&count=1&language=id&format=json";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                return null;

            var r0 = results[0];
            var city = r0.GetProperty("name").GetString() ?? name;
            var lat = r0.GetProperty("latitude").GetDouble();
            var lon = r0.GetProperty("longitude").GetDouble();
            return (city, lat, lon);
        }
        catch (HttpRequestException) { return null; }
        catch (TaskCanceledException) { return null; }
        catch (JsonException) { return null; }
    }

    // Air quality: ambil US AQI dan PM2.5 untuk koordinat tertentu
    public static async Task<AirQualityResult?> GetAirQualityAsync(double lat, double lon)
    {
        try
        {
            var latStr = lat.ToString("F5", CultureInfo.InvariantCulture);
            var lonStr = lon.ToString("F5", CultureInfo.InvariantCulture);
            var url = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={latStr}&longitude={lonStr}&current=us_aqi,pm2_5";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var current = doc.RootElement.GetProperty("current");
            var aqi = current.GetProperty("us_aqi").GetInt32();
            var pm25 = current.GetProperty("pm2_5").GetDouble();
            return new AirQualityResult(aqi, pm25);
        }
        catch (HttpRequestException) { return null; }
        catch (TaskCanceledException) { return null; }
        catch (JsonException) { return null; }
    }

    // Kategori kualitas udara berdasarkan US AQI
    public static string Categorize(int aqi) => aqi switch
    {
        <= 50 => "Baik",
        <= 100 => "Sedang",
        <= 150 => "Tidak sehat bagi kelompok sensitif",
        <= 200 => "Tidak sehat",
        _ => "Sangat tidak sehat"
    };
}
