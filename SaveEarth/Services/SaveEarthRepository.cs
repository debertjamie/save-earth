using Npgsql;
using SaveEarth.Models;

namespace SaveEarth.Services;

/// <summary>
/// All DB queries for SaveEarth. Every method is safe: returns a neutral
/// value when the DB is unavailable or an NpgsqlException occurs.
/// All queries use parameterized commands.
/// </summary>
public class SaveEarthRepository
{
    // ── Users ─────────────────────────────────────────────────────────────────

    public bool TryAddUser(User user)
    {
        if (!Database.IsAvailable) return false;
        try
        {
            using var conn = Database.Open();
            const string sql = """
                INSERT INTO users (user_id, name, email, password_hash, total_points)
                VALUES (@id, @name, @email, @hash, @pts)
                ON CONFLICT DO NOTHING;
                """;
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id",    user.UserId);
            cmd.Parameters.AddWithValue("name",  user.Name);
            cmd.Parameters.AddWithValue("email", user.Email.ToLowerInvariant());
            cmd.Parameters.AddWithValue("hash",  user.PasswordHash);
            cmd.Parameters.AddWithValue("pts",   user.TotalPoints);
            return cmd.ExecuteNonQuery() == 1;
        }
        catch (NpgsqlException) { return false; }
    }

    public User? GetUserByEmail(string email)
    {
        if (!Database.IsAvailable) return null;
        try
        {
            using var conn = Database.Open();
            const string sql = "SELECT user_id, name, email, password_hash, total_points FROM users WHERE email = @email;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("email", email.Trim().ToLowerInvariant());
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return User.FromStorage(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4));
        }
        catch (NpgsqlException) { return null; }
    }

    public void UpdateUserPoints(string userId, int points)
    {
        if (!Database.IsAvailable) return;
        try
        {
            using var conn = Database.Open();
            const string sql = "UPDATE users SET total_points = @pts WHERE user_id = @id;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("pts", points);
            cmd.Parameters.AddWithValue("id",  userId);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException) { /* silent */ }
    }

    // ── UserChallenges ────────────────────────────────────────────────────────

    public void UpsertUserChallenge(UserChallenge uc)
    {
        if (!Database.IsAvailable) return;
        try
        {
            using var conn = Database.Open();
            const string sql = """
                INSERT INTO user_challenges (user_id, challenge_id, start_date, end_date, status, progress, total_points)
                VALUES (@uid, @cid, @start, @end, @status, @prog, @pts)
                ON CONFLICT (user_id, challenge_id) DO UPDATE
                    SET start_date   = EXCLUDED.start_date,
                        end_date     = EXCLUDED.end_date,
                        status       = EXCLUDED.status,
                        progress     = EXCLUDED.progress,
                        total_points = EXCLUDED.total_points;
                """;
            using var cmd = new NpgsqlCommand(sql, conn);
            var startUtc = uc.StartDate.Kind == DateTimeKind.Utc ? uc.StartDate : DateTime.SpecifyKind(uc.StartDate, DateTimeKind.Utc);
            var endUtc = uc.EndDate.Kind == DateTimeKind.Utc ? uc.EndDate : DateTime.SpecifyKind(uc.EndDate, DateTimeKind.Utc);
            cmd.Parameters.AddWithValue("uid",    uc.UserId);
            cmd.Parameters.AddWithValue("cid",    uc.ChallengeId);
            cmd.Parameters.AddWithValue("start",  startUtc);
            cmd.Parameters.AddWithValue("end",    endUtc);
            cmd.Parameters.AddWithValue("status", uc.Status.ToString());
            cmd.Parameters.AddWithValue("prog",   uc.Progress);
            cmd.Parameters.AddWithValue("pts",    uc.TotalPoints);
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException) { /* silent */ }
    }

    public List<UserChallenge> GetUserChallenges(string userId)
    {
        var list = new List<UserChallenge>();
        if (!Database.IsAvailable) return list;
        try
        {
            using var conn = Database.Open();
            const string sql = "SELECT challenge_id, start_date, end_date, status, progress, total_points FROM user_challenges WHERE user_id = @uid;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var statusStr = reader.GetString(3);
                var status = Enum.TryParse<ChallengeStatus>(statusStr, out var s) ? s : ChallengeStatus.InProgress;
                var uc = UserChallenge.FromStorage(
                    userId,
                    reader.GetString(0),
                    reader.GetDateTime(1),
                    reader.GetDateTime(2),
                    status,
                    reader.GetFloat(4),
                    reader.GetInt32(5));
                list.Add(uc);
            }
        }
        catch (NpgsqlException) { /* return whatever we have */ }
        return list;
    }

    // ── DailyTracking ─────────────────────────────────────────────────────────

    public void UpsertDailyTracking(DailyTracking dt)
    {
        if (!Database.IsAvailable) return;
        try
        {
            using var conn = Database.Open();
            const string sql = """
                INSERT INTO daily_tracking (user_id, date, daily_emission, activity_type, activity_value, notes)
                VALUES (@uid, @date, @emission, @atype, @aval, @notes)
                ON CONFLICT (user_id, date) DO UPDATE
                    SET daily_emission = EXCLUDED.daily_emission,
                        activity_type  = EXCLUDED.activity_type,
                        activity_value = EXCLUDED.activity_value,
                        notes          = EXCLUDED.notes;
                """;
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid",      dt.UserId);
            cmd.Parameters.AddWithValue("date",     dt.Date.Date);
            cmd.Parameters.AddWithValue("emission", dt.DailyEmission);
            cmd.Parameters.Add(new NpgsqlParameter("atype", NpgsqlTypes.NpgsqlDbType.Text) { Value = string.IsNullOrEmpty(dt.ActivityType) ? DBNull.Value : dt.ActivityType });
            cmd.Parameters.AddWithValue("aval",     dt.ActivityValue);
            cmd.Parameters.Add(new NpgsqlParameter("notes", NpgsqlTypes.NpgsqlDbType.Text) { Value = string.IsNullOrEmpty(dt.Notes) ? DBNull.Value : dt.Notes });
            cmd.ExecuteNonQuery();
        }
        catch (NpgsqlException) { /* silent */ }
    }

    public DailyTracking? GetDailyTracking(string userId, DateTime date)
    {
        if (!Database.IsAvailable) return null;
        try
        {
            using var conn = Database.Open();
            const string sql = "SELECT daily_emission, activity_type, activity_value, notes FROM daily_tracking WHERE user_id = @uid AND date = @date;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid",  userId);
            cmd.Parameters.AddWithValue("date", date.Date);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            var dt = DailyTracking.FromStorage(
                userId,
                date,
                reader.GetFloat(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.GetFloat(2),
                reader.IsDBNull(3) ? null : reader.GetString(3));
            return dt;
        }
        catch (NpgsqlException) { return null; }
    }
}
