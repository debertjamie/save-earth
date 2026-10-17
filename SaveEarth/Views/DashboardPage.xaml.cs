using System.Windows;
using System.Windows.Controls;
using SaveEarth.Services;

namespace SaveEarth.Views;

public partial class DashboardPage : Page
{
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

        var suggestions = new RecommendationEngine().GetSuggestions(emission);
        RecommendationText.Text = suggestions.Length > 0
            ? suggestions[0]
            : "Keep up the good work!";
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
