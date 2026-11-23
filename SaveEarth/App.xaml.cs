using System.Windows;
using SaveEarth.Services;

namespace SaveEarth;

/// <summary>
/// Interaction logic for App.xaml.
/// Initializes the database connection on startup.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Database.Initialize();
    }
}
