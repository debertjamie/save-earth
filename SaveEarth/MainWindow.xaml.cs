using System.Windows;
using System.Windows.Controls;
using SaveEarth.Views;

namespace SaveEarth;

/// <summary>
/// Interaction logic for MainWindow.xaml. Acts as the Frame-based navigation
/// host for the app and shows the Welcome page on load.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Navigate(new WelcomePage());
    }

    public void Navigate(Page page)
    {
        MainFrame.Navigate(page);
    }
}
