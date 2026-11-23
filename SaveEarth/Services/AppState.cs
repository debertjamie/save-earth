using SaveEarth.Models;

namespace SaveEarth.Services;

/// <summary>
/// Session and state manager for SaveEarth.
/// Holds the current user, seed catalog, and in-memory caches,
/// with persistence via SaveEarthRepository when Database.IsAvailable.
/// </summary>
public class AppState
{
    public static AppState Current { get; } = new();

    private readonly Dictionary<string, User> _usersByEmail = new();
    private readonly Dictionary<string, DailyTracking> _todayTracking = new();
    private readonly List<Challenge> _challenges;
    private readonly SaveEarthRepository _repository = new();

    public User? CurrentUser { get; private set; }
    public string? LastRegisterError { get; private set; }

    private AppState()
    {
        Database.Initialize();

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
        LastRegisterError = null;

        if (string.IsNullOrWhiteSpace(email))
        {
            LastRegisterError = "Please enter a valid email.";
            return false;
        }

        var key = email.Trim().ToLowerInvariant();
        if (_usersByEmail.ContainsKey(key))
        {
            LastRegisterError = "Email sudah terdaftar.";
            return false;
        }

        if (Database.IsAvailable)
        {
            var existingInDb = _repository.GetUserByEmail(email);
            if (existingInDb != null)
            {
                LastRegisterError = "Email sudah terdaftar.";
                return false;
            }
        }

        try
        {
            var user = new User(Guid.NewGuid().ToString(), name, email, password);
            _usersByEmail[key] = user;

            if (Database.IsAvailable)
            {
                _repository.TryAddUser(user);
            }

            return true;
        }
        catch (ArgumentException ex)
        {
            LastRegisterError = ex.Message;
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

        // 1. Check in-memory first
        if (_usersByEmail.TryGetValue(key, out var user))
        {
            if (user.Login(email, password))
            {
                CurrentUser = user;
                LoadUserData(user);
                return user;
            }
            return null;
        }

        // 2. Try loading from database
        if (Database.IsAvailable)
        {
            var dbUser = _repository.GetUserByEmail(email);
            if (dbUser != null && PasswordHasher.Verify(password, dbUser.PasswordHash))
            {
                _usersByEmail[key] = dbUser;
                CurrentUser = dbUser;
                LoadUserData(dbUser);
                return dbUser;
            }
        }

        return null;
    }

    private void LoadUserData(User user)
    {
        if (!Database.IsAvailable) return;

        // Load persisted challenges
        var dbChallenges = _repository.GetUserChallenges(user.UserId);
        user.Challenges.Clear();
        foreach (var uc in dbChallenges)
        {
            user.Challenges.Add(uc);
        }

        // Load today's tracking if available
        var dt = _repository.GetDailyTracking(user.UserId, DateTime.Today);
        if (dt != null)
        {
            _todayTracking[user.UserId] = dt;
        }
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public IReadOnlyList<Challenge> GetChallenges()
    {
        return _challenges;
    }

    public bool JoinChallenge(string challengeId)
    {
        if (CurrentUser is null) return false;

        var joined = CurrentUser.JoinChallenge(challengeId);
        if (joined)
        {
            var uc = CurrentUser.Challenges.FirstOrDefault(c => c.ChallengeId == challengeId && !c.IsCompleted);
            if (uc != null)
            {
                _repository.UpsertUserChallenge(uc);
            }
        }
        return joined;
    }

    public bool UpdateChallengeProgress(string challengeId, float progress)
    {
        if (CurrentUser is null) return false;
        var uc = CurrentUser.Challenges.FirstOrDefault(c => c.ChallengeId == challengeId);
        if (uc is null) return false;

        uc.UpdateProgress(progress);
        _repository.UpsertUserChallenge(uc);

        if (uc.IsCompleted)
        {
            var catalog = _challenges.FirstOrDefault(c => c.ChallengeId == challengeId);
            if (catalog != null)
            {
                UpdateUserPoints(CurrentUser.TotalPoints + catalog.RewardPoints);
            }
        }
        return true;
    }

    public void UpdateUserPoints(int points)
    {
        if (CurrentUser is null) return;
        CurrentUser.SetTotalPoints(points);
        _repository.UpdateUserPoints(CurrentUser.UserId, points);
    }

    public void SaveDailyTracking(DailyTracking tracking)
    {
        if (CurrentUser is null) return;
        _todayTracking[CurrentUser.UserId] = tracking;
        _repository.UpsertDailyTracking(tracking);
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
            if (Database.IsAvailable)
            {
                tracking = _repository.GetDailyTracking(userId, DateTime.Today);
            }

            if (tracking is null)
            {
                tracking = new DailyTracking(userId, DateTime.Today, 0f);
            }

            _todayTracking[userId] = tracking;
        }

        return tracking;
    }

    public float GetTodayEmission()
    {
        if (CurrentUser is not null)
        {
            var tracking = GetOrCreateTodayTracking();
            return tracking.DailyEmission;
        }

        return 0f;
    }

    public int GetCurrentPoints()
    {
        return CurrentUser?.TotalPoints ?? 0;
    }
}
