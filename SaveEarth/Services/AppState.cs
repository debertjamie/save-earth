using SaveEarth.Models;

namespace SaveEarth.Services;

/// <summary>
/// In-memory session/singleton holding the registered users, the current user,
/// the seed challenge catalog and each user's current-day tracking.
/// Single-threaded (WPF UI thread) and intentionally simple.
/// </summary>
public class AppState
{
    public static AppState Current { get; } = new();

    private readonly Dictionary<string, User> _usersByEmail = new();
    private readonly Dictionary<string, DailyTracking> _todayTracking = new();
    private readonly List<Challenge> _challenges;

    public User? CurrentUser { get; private set; }

    private AppState()
    {
        _challenges = new List<Challenge>
        {
            new Challenge(
                "public-transport",
                "Use Public Transport",
                "Swap private driving for the bus or train for a day.",
                50, 7, 2.4f),
            new Challenge(
                "meatless-lunch",
                "Meatless Lunch",
                "Choose a plant-based lunch instead of meat.",
                30, 5, 1.5f),
            new Challenge(
                "lights-off",
                "Lights Off",
                "Turn off lights and appliances you are not using.",
                20, 10, 0.8f),
            new Challenge(
                "short-showers",
                "Shorter Showers",
                "Keep showers under five minutes to save energy.",
                25, 14, 0.6f),
        };
    }

    public bool RegisterUser(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var key = email.Trim().ToLowerInvariant();
        if (_usersByEmail.ContainsKey(key))
        {
            return false;
        }

        try
        {
            var user = new User(Guid.NewGuid().ToString(), name, email, password);
            _usersByEmail[key] = user;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public User? Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var key = email.Trim().ToLowerInvariant();
        if (_usersByEmail.TryGetValue(key, out var user) && user.Login(email, password))
        {
            CurrentUser = user;
            return user;
        }

        return null;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public IReadOnlyList<Challenge> GetChallenges()
    {
        return _challenges;
    }

    public DailyTracking GetOrCreateTodayTracking()
    {
        if (CurrentUser is null)
        {
            throw new InvalidOperationException("No user is logged in.");
        }

        var userId = CurrentUser.UserId;
        if (!_todayTracking.TryGetValue(userId, out var tracking))
        {
            tracking = new DailyTracking(userId, DateTime.Today, 0f);
            _todayTracking[userId] = tracking;
        }

        return tracking;
    }

    public float GetTodayEmission()
    {
        if (CurrentUser is not null && _todayTracking.TryGetValue(CurrentUser.UserId, out var tracking))
        {
            return tracking.DailyEmission;
        }

        return 0f;
    }

    public int GetCurrentPoints()
    {
        return CurrentUser?.TotalPoints ?? 0;
    }
}
