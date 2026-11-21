using System.Windows;
using System.Windows.Controls;
using SaveEarth.Services;

namespace SaveEarth.Views;

public partial class LoginPage : Page
{
    private bool _registerMode;

    public LoginPage()
    {
        InitializeComponent();
        ApplyMode();
    }

    private void ApplyMode()
    {
        if (_registerMode)
        {
            HeaderText.Text = "Register";
            NamePanel.Visibility = Visibility.Visible;
            PrimaryButton.Content = "Register";
            ToggleButton.Content = "Have an account? Login";
        }
        else
        {
            HeaderText.Text = "Login";
            NamePanel.Visibility = Visibility.Collapsed;
            PrimaryButton.Content = "Login";
            ToggleButton.Content = "Need an account? Register";
        }

        ErrorText.Text = string.Empty;
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _registerMode = !_registerMode;
        ApplyMode();
    }

    private void PrimaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_registerMode)
        {
            Register();
        }
        else
        {
            Login();
        }
    }

    private void Login()
    {
        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ErrorText.Text = "Please enter your email and password.";
            return;
        }

        var user = AppState.Current.Login(email, password);
        if (user is null)
        {
            ErrorText.Text = "Invalid email or password.";
            return;
        }

        NavService.Go(new DashboardPage());
    }

    private void Register()
    {
        var name = NameBox.Text.Trim();
        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ErrorText.Text = "Please fill in your name, email and password.";
            return;
        }

        if (!AppState.Current.RegisterUser(name, email, password))
        {
            ErrorText.Text = "Email already registered or invalid.";
            return;
        }

        var user = AppState.Current.Login(email, password);
        if (user is null)
        {
            ErrorText.Text = "Could not sign you in. Please try logging in.";
            return;
        }

        NavService.Go(new DashboardPage());
    }
}
