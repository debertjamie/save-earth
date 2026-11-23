using System.IO;
using System.Text.Json;
using Npgsql;

namespace SaveEarth.Services;

/// <summary>
/// Manages the NpgsqlDataSource and schema creation.
/// Falls back gracefully if the DB is unavailable.
/// </summary>
public static class Database
{
    private static NpgsqlDataSource? _source;

    public static bool IsAvailable { get; private set; }
    public static string LastError { get; private set; } = string.Empty;

    // Seed challenges — same IDs as AppState
    private static readonly (string Id, string Title, string Desc, int Points, int Days, float Target)[] _seedChallenges =
    [
        ("public-transport", "Use Public Transport", "Swap private driving for the bus or train for a day.", 50, 7, 2.4f),
        ("meatless-lunch",   "Meatless Lunch",        "Choose a plant-based lunch instead of meat.",          30, 5, 1.5f),
        ("lights-off",       "Lights Off",             "Turn off lights and appliances you are not using.",    20, 10, 0.8f),
        ("short-showers",    "Shorter Showers",        "Keep showers under five minutes to save energy.",      25, 14, 0.6f),
    ];

    public static void Initialize()
    {
        try
        {
            var connStr = ResolveConnectionString();
            if (connStr is null)
            {
                LastError = "No connection string configured.";
                IsAvailable = false;
                return;
            }

            _source = NpgsqlDataSource.Create(connStr);

            using var conn = _source.OpenConnection();
            ApplySchema(conn);
            SeedChallenges(conn);

            IsAvailable = true;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            LastError = ex.Message;
            _source = null;
        }
    }

    public static NpgsqlConnection Open()
    {
        if (_source is null)
            throw new InvalidOperationException("Database is not available.");
        return _source.OpenConnection();
    }

    // ── Connection string resolution ──────────────────────────────────────────

    private static string? ResolveConnectionString()
    {
        // 1. Environment variable
        var env = Environment.GetEnvironmentVariable("SAVEEARTH_DB");
        if (!string.IsNullOrWhiteSpace(env))
            return env;

        // 2. appsettings.local.json in app base directory
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.local.json");
        if (!File.Exists(path))
            return null;

        try
        {
            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("ConnectionString", out var prop))
                return prop.GetString();
        }
        catch
        {
            // ignore malformed file
        }

        return null;
    }

    // ── Schema ────────────────────────────────────────────────────────────────

    private static void ApplySchema(NpgsqlConnection conn)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS users (
                user_id       text PRIMARY KEY,
                name          text NOT NULL,
                email         text NOT NULL UNIQUE,
                password_hash text NOT NULL,
                role          text NOT NULL DEFAULT 'User',
                total_points  int  NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS challenges (
                challenge_id              text PRIMARY KEY,
                title                     text NOT NULL,
                description               text NOT NULL,
                reward_points             int  NOT NULL DEFAULT 0,
                target_days               int  NOT NULL,
                emission_reduction_target real NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS user_challenges (
                user_id      text REFERENCES users(user_id),
                challenge_id text REFERENCES challenges(challenge_id),
                start_date   timestamptz NOT NULL,
                end_date     timestamptz NOT NULL,
                status       text NOT NULL,
                progress     real NOT NULL DEFAULT 0,
                total_points int  NOT NULL DEFAULT 0,
                PRIMARY KEY (user_id, challenge_id)
            );

            CREATE TABLE IF NOT EXISTS daily_tracking (
                user_id        text REFERENCES users(user_id),
                date           date NOT NULL,
                daily_emission real NOT NULL DEFAULT 0,
                activity_type  text,
                activity_value real NOT NULL DEFAULT 0,
                notes          text,
                PRIMARY KEY (user_id, date)
            );
            """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void SeedChallenges(NpgsqlConnection conn)
    {
        const string sql = """
            INSERT INTO challenges (challenge_id, title, description, reward_points, target_days, emission_reduction_target)
            VALUES (@id, @title, @desc, @points, @days, @target)
            ON CONFLICT DO NOTHING;
            """;

        foreach (var (id, title, desc, points, days, target) in _seedChallenges)
        {
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id",     id);
            cmd.Parameters.AddWithValue("title",  title);
            cmd.Parameters.AddWithValue("desc",   desc);
            cmd.Parameters.AddWithValue("points", points);
            cmd.Parameters.AddWithValue("days",   days);
            cmd.Parameters.AddWithValue("target", target);
            cmd.ExecuteNonQuery();
        }
    }
}
