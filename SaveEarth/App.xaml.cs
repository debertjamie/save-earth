using System.IO;
using System.Windows;
using SaveEarth.Models;
using SaveEarth.Services;

namespace SaveEarth;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Database.Initialize();

        if (e.Args.Length > 0)
        {
            switch (e.Args[0])
            {
                case "--test-persistence":
                    RunPersistenceTest();
                    Shutdown();
                    return;
                case "--test-session-2":
                    RunSession2Test();
                    Shutdown();
                    return;
                case "--test-offline":
                    RunOfflineTest();
                    Shutdown();
                    return;
            }
        }
    }

    private static void RunPersistenceTest()
    {
        Console.WriteLine("[TEST 1] Testing Database Persistence...");
        if (!Database.IsAvailable)
        {
            Console.WriteLine($"[FAIL] Database is not available: {Database.LastError}");
            Environment.Exit(1);
            return;
        }

        var email = $"h10_{Guid.NewGuid():N}@saveearth.local";
        var tempFile = Path.Combine(Path.GetTempPath(), "saveearth_h10_test_user.txt");
        File.WriteAllText(tempFile, email);

        // 1. Register User
        if (!AppState.Current.RegisterUser("Tester H10", email, "Secret123!"))
        {
            Console.WriteLine($"[FAIL] RegisterUser failed: {AppState.Current.LastRegisterError}");
            Environment.Exit(1);
            return;
        }

        var user = AppState.Current.Login(email, "Secret123!");
        if (user is null)
        {
            Console.WriteLine("[FAIL] Login after register failed.");
            Environment.Exit(1);
            return;
        }

        // 2. Join Challenge
        if (!AppState.Current.JoinChallenge("public-transport"))
        {
            Console.WriteLine("[FAIL] JoinChallenge failed.");
            Environment.Exit(1);
            return;
        }

        // 3. Save Daily Tracking
        var tracking = AppState.Current.GetOrCreateTodayTracking();
        tracking.EditLog("driving", 25.5f, "Commute to campus");
        new EmissionCalculator().CalculateDailyEmission(tracking);
        AppState.Current.SaveDailyTracking(tracking);

        // 4. Verify in DB via repository directly
        var repo = new SaveEarthRepository();
        var dbUser = repo.GetUserByEmail(email);
        if (dbUser is null)
        {
            Console.WriteLine("[FAIL] User not found in PostgreSQL.");
            Environment.Exit(1);
            return;
        }

        var dbChallenges = repo.GetUserChallenges(user.UserId);
        if (!dbChallenges.Any(c => c.ChallengeId == "public-transport" && !c.IsCompleted))
        {
            Console.WriteLine("[FAIL] User challenge not found in PostgreSQL.");
            Environment.Exit(1);
            return;
        }

        var dbTracking = repo.GetDailyTracking(user.UserId, DateTime.Today);
        if (dbTracking is null || Math.Abs(dbTracking.ActivityValue - 25.5f) > 0.01f)
        {
            Console.WriteLine("[FAIL] Daily tracking not persisted in PostgreSQL.");
            Environment.Exit(1);
            return;
        }

        Console.WriteLine($"[SUCCESS] User registered, challenge joined, and daily tracking persisted to PostgreSQL for {email}.");
        Environment.Exit(0);
    }

    private static void RunSession2Test()
    {
        Console.WriteLine("[TEST 2] Testing Session 2 (fresh app restart)...");
        if (!Database.IsAvailable)
        {
            Console.WriteLine($"[FAIL] Database is not available: {Database.LastError}");
            Environment.Exit(1);
            return;
        }

        var tempFile = Path.Combine(Path.GetTempPath(), "saveearth_h10_test_user.txt");
        if (!File.Exists(tempFile))
        {
            Console.WriteLine("[FAIL] Test user file not found. Run --test-persistence first.");
            Environment.Exit(1);
            return;
        }

        var email = File.ReadAllText(tempFile).Trim();

        // Fresh login from DB
        var user = AppState.Current.Login(email, "Secret123!");
        if (user is null)
        {
            Console.WriteLine($"[FAIL] Session 2 login failed for {email}.");
            Environment.Exit(1);
            return;
        }

        // Verify active challenge loaded from DB
        var activeChallenge = user.Challenges.FirstOrDefault(c => c.ChallengeId == "public-transport" && !c.IsCompleted);
        if (activeChallenge is null)
        {
            Console.WriteLine("[FAIL] Active challenge 'public-transport' was not restored from DB.");
            Environment.Exit(1);
            return;
        }

        // Verify today's tracking loaded from DB
        var tracking = AppState.Current.GetOrCreateTodayTracking();
        if (tracking is null || tracking.ActivityType != "driving" || Math.Abs(tracking.ActivityValue - 25.5f) > 0.01f)
        {
            Console.WriteLine($"[FAIL] Daily tracking not restored from DB. Found Type={tracking?.ActivityType}, Value={tracking?.ActivityValue}");
            Environment.Exit(1);
            return;
        }

        Console.WriteLine($"[SUCCESS] Session 2 restored user {email}, active challenge '{activeChallenge.ChallengeId}', and daily emission {tracking.DailyEmission:0.##} kg CO2.");
        Environment.Exit(0);
    }

    private static void RunOfflineTest()
    {
        Console.WriteLine("[TEST 3] Testing Offline Mode...");
        if (Database.IsAvailable)
        {
            Console.WriteLine("[FAIL] Database is unexpectedly available during offline test.");
            Environment.Exit(1);
            return;
        }

        var email = "offline_user@saveearth.local";
        if (!AppState.Current.RegisterUser("Offline User", email, "Offline123!"))
        {
            Console.WriteLine($"[FAIL] Offline registration failed: {AppState.Current.LastRegisterError}");
            Environment.Exit(1);
            return;
        }

        var user = AppState.Current.Login(email, "Offline123!");
        if (user is null)
        {
            Console.WriteLine("[FAIL] Offline login failed.");
            Environment.Exit(1);
            return;
        }

        if (!AppState.Current.JoinChallenge("meatless-lunch"))
        {
            Console.WriteLine("[FAIL] Offline join challenge failed.");
            Environment.Exit(1);
            return;
        }

        var tracking = AppState.Current.GetOrCreateTodayTracking();
        tracking.EditLog("electricity", 1000f, "AC usage");
        AppState.Current.SaveDailyTracking(tracking);

        Console.WriteLine("[SUCCESS] Offline mode operated completely in-memory without error or crash.");
        Environment.Exit(0);
    }
}
