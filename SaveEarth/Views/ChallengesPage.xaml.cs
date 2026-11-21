using System.Windows;
using System.Windows.Controls;
using SaveEarth.Services;

namespace SaveEarth.Views;

public partial class ChallengesPage : Page
{
    private sealed record ChallengeRow(
        string ChallengeId,
        string Title,
        string Description,
        string RewardText,
        bool CanJoin,
        string JoinLabel);

    private sealed record ActiveRow(string Title);

    public ChallengesPage()
    {
        InitializeComponent();

        if (AppState.Current.CurrentUser is null)
        {
            NavService.Go(new LoginPage());
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        var user = AppState.Current.CurrentUser;
        if (user is null)
        {
            NavService.Go(new LoginPage());
            return;
        }

        var joinedIds = user.Challenges
            .Where(c => !c.IsCompleted)
            .Select(c => c.ChallengeId)
            .ToHashSet();

        var rows = AppState.Current.GetChallenges()
            .Select(c =>
            {
                var joined = joinedIds.Contains(c.ChallengeId);
                return new ChallengeRow(
                    c.ChallengeId,
                    c.Title,
                    c.Description,
                    $"{c.RewardPoints} pts · {c.TargetDays} days",
                    !joined,
                    joined ? "Joined" : "Join");
            })
            .ToList();

        ChallengesList.ItemsSource = rows;

        var active = user.Challenges
            .Where(c => !c.IsCompleted)
            .Select(c =>
            {
                var catalog = AppState.Current.GetChallenges()
                    .FirstOrDefault(x => x.ChallengeId == c.ChallengeId);
                return new ActiveRow(catalog?.Title ?? c.ChallengeId);
            })
            .ToList();

        ActiveList.ItemsSource = active;
        NoActiveText.Visibility = active.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Join_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string challengeId)
        {
            AppState.Current.CurrentUser?.JoinChallenge(challengeId);
            Refresh();
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        NavService.Go(new DashboardPage());
    }
}
