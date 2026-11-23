using System.Windows;
using System.Windows.Controls;
using SaveEarth.Services;

namespace SaveEarth.Views;

public partial class WelcomePage : Page
{
    public WelcomePage()
    {
        InitializeComponent();
        ModeStatusText.Text = Database.IsAvailable ? "Mode: Database" : "Mode: Offline (data tidak tersimpan)";
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        NavService.Go(new LoginPage());
    }
}
