using System.Windows;
using System.Windows.Controls;
using SaveEarth.Services;

namespace SaveEarth.Views;

public partial class DailyTrackerPage : Page
{
    public DailyTrackerPage()
    {
        InitializeComponent();

        if (AppState.Current.CurrentUser is null)
        {
            NavService.Go(new LoginPage());
        }
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;

        if (ActivityBox.SelectedItem is not ComboBoxItem item || item.Tag is not string activityType)
        {
            ErrorText.Text = "Please select an activity type.";
            return;
        }

        if (!float.TryParse(ValueBox.Text.Trim(), out var value) || value < 0f)
        {
            ErrorText.Text = "Please enter a valid non-negative number.";
            return;
        }

        try
        {
            var tracking = AppState.Current.GetOrCreateTodayTracking();
            tracking.EditLog(activityType, value, NotesBox.Text);
            var emission = new EmissionCalculator().CalculateDailyEmission(tracking);

            ResultText.Text = $"{emission:0.##} kg CO₂";
            ResultCard.Visibility = Visibility.Visible;
        }
        catch (ArgumentException)
        {
            ErrorText.Text = "Could not log that activity. Please check your input.";
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        NavService.Go(new DashboardPage());
    }
}
