using System.Windows;
using System.Windows.Controls;

namespace SaveEarth.Services;

/// <summary>
/// Thin wrapper over the single MainWindow nav host so pages navigate without
/// repeating the cast to MainWindow.
/// </summary>
public static class NavService
{
    public static void Go(Page page)
    {
        ((MainWindow)Application.Current.MainWindow).Navigate(page);
    }
}
