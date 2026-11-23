using System.Windows;
using System.Windows.Controls;
using SaveEarth.Services;

namespace SaveEarth.Views;

public partial class DashboardPage : Page
{
    // Simpan AQI terakhir agar bisa dipakai update rekomendasi
    private int _lastUsAqi = 0;

    public DashboardPage()
    {
        InitializeComponent();
        LoadDashboard();
    }

    private void LoadDashboard()
    {
        var user = AppState.Current.CurrentUser;
        if (user is null)
        {
            NavService.Go(new LoginPage());
            return;
        }

        GreetingText.Text = $"Hi, {user.Name} 👋";
        PointsText.Text = AppState.Current.GetCurrentPoints().ToString();

        var activeCount = user.Challenges.Count(c => !c.IsCompleted);
        ActiveChallengesText.Text = activeCount.ToString();

        var emission = AppState.Current.GetTodayEmission();
        EmissionText.Text = $"{emission:0.##} kg CO₂";

        RefreshRecommendation(emission);
    }

    private void RefreshRecommendation(float emission)
    {
        var suggestions = new RecommendationEngine().GetSuggestions(emission, _lastUsAqi);
        RecommendationText.Text = suggestions.Length > 0
            ? suggestions[0]
            : "Keep up the good work!";
    }

    private async void AirQuality_Click(object sender, RoutedEventArgs e)
    {
        var cityName = CityBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(cityName))
        {
            AirQualityText.Text = "⚠️ Masukkan nama kota terlebih dahulu.";
            return;
        }

        AirQualityButton.IsEnabled = false;
        AirQualityText.Text = "Mencari...";

        var cityResult = await AirQualityService.FindCityAsync(cityName);
        if (cityResult is null)
        {
            AirQualityText.Text = AirQualityService.LastError is null
                ? "❌ Kota tidak ditemukan."
                : $"❌ Gagal terhubung: {AirQualityService.LastError}";
            AirQualityButton.IsEnabled = true;
            return;
        }

        var (city, lat, lon) = cityResult.Value;
        AirQualityText.Text = "Mengambil data kualitas udara...";

        var aqResult = await AirQualityService.GetAirQualityAsync(lat, lon);
        if (aqResult is null)
        {
            AirQualityText.Text = AirQualityService.LastError is null
                ? "❌ Gagal mengambil data kualitas udara."
                : $"❌ Gagal mengambil data: {AirQualityService.LastError}";
            AirQualityButton.IsEnabled = true;
            return;
        }

        var category = AirQualityService.Categorize(aqResult.UsAqi);
        AirQualityText.Text =
            $"📍 {city}\n" +
            $"US AQI: {aqResult.UsAqi}  |  PM2.5: {aqResult.Pm25:0.#} µg/m³\n" +
            $"Kategori: {category}";

        // Update rekomendasi dengan AQI baru
        _lastUsAqi = aqResult.UsAqi;
        var emission = AppState.Current.GetTodayEmission();
        var suggestions = new RecommendationEngine().GetSuggestions(emission, _lastUsAqi);
        RecommendationText.Text = suggestions.Length > 0
            ? string.Join("\n• ", suggestions.Prepend(""))
            : "Keep up the good work!";
        if (RecommendationText.Text.StartsWith("\n• "))
            RecommendationText.Text = "• " + RecommendationText.Text.TrimStart('\n', '•', ' ');

        AirQualityButton.IsEnabled = true;
    }

    private void DailyTracker_Click(object sender, RoutedEventArgs e)
    {
        NavService.Go(new DailyTrackerPage());
    }

    private void Challenges_Click(object sender, RoutedEventArgs e)
    {
        NavService.Go(new ChallengesPage());
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        AppState.Current.Logout();
        NavService.Go(new WelcomePage());
    }
}

