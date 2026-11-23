using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace SaveEarth.Services;

public record AirQualityResult(int UsAqi, double Pm25);

public static class AirQualityService
{
    private static readonly HttpClient _http;

    static AirQualityService()
    {
        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(10)
        };
        _http = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("SaveEarth/1.0 (Windows; C# WPF)");
    }

    /// <summary>
    /// Pesan error terakhir jika terjadi exception pada panggilan HTTP / parsing.
    /// Bernilai null jika hasil kosong karena kota memang tidak ditemukan.
    /// </summary>
    public static string? LastError { get; private set; }

    // Geocoding: cari kota by name, kembalikan (name, lat, lon) atau null
    public static async Task<(string City, double Lat, double Lon)?> FindCityAsync(string name)
    {
        LastError = null;
        try
        {
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(name)}&count=1&language=id&format=json";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                return null; // Kota tidak ditemukan

            var r0 = results[0];
            var city = r0.GetProperty("name").GetString() ?? name;
            var lat = r0.GetProperty("latitude").GetDouble();
            var lon = r0.GetProperty("longitude").GetDouble();
            return (city, lat, lon);
        }
        catch (HttpRequestException ex)
        {
            LastError = $"HttpRequestException: {ex.Message}";
            return null;
        }
        catch (TaskCanceledException ex)
        {
            LastError = $"Timeout: {ex.Message}";
            return null;
        }
        catch (JsonException ex)
        {
            LastError = $"JsonException: {ex.Message}";
            return null;
        }
        catch (Exception ex)
        {
            LastError = $"{ex.GetType().Name}: {ex.Message}";
            return null;
        }
    }

    // Air quality: ambil US AQI dan PM2.5 untuk koordinat tertentu
    public static async Task<AirQualityResult?> GetAirQualityAsync(double lat, double lon)
    {
        LastError = null;
        try
        {
            var latStr = lat.ToString("F5", CultureInfo.InvariantCulture);
            var lonStr = lon.ToString("F5", CultureInfo.InvariantCulture);
            var url = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={latStr}&longitude={lonStr}&current=us_aqi,pm2_5";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var current = doc.RootElement.GetProperty("current");

            int aqi = 0;
            if (current.TryGetProperty("us_aqi", out var aqiElem) && aqiElem.ValueKind == JsonValueKind.Number)
            {
                if (!aqiElem.TryGetInt32(out aqi) && aqiElem.TryGetDouble(out var aqiD))
                {
                    aqi = (int)Math.Round(aqiD);
                }
            }

            double pm25 = 0;
            if (current.TryGetProperty("pm2_5", out var pm25Elem) && pm25Elem.ValueKind == JsonValueKind.Number)
            {
                pm25Elem.TryGetDouble(out pm25);
            }

            return new AirQualityResult(aqi, pm25);
        }
        catch (HttpRequestException ex)
        {
            LastError = $"HttpRequestException: {ex.Message}";
            return null;
        }
        catch (TaskCanceledException ex)
        {
            LastError = $"Timeout: {ex.Message}";
            return null;
        }
        catch (JsonException ex)
        {
            LastError = $"JsonException: {ex.Message}";
            return null;
        }
        catch (Exception ex)
        {
            LastError = $"{ex.GetType().Name}: {ex.Message}";
            return null;
        }
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
