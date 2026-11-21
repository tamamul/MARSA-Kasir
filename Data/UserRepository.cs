using System.Security.Cryptography;
using MarsaKasir.Models;
namespace MarsaKasir.Data;

public static class UserRepository
{
    static byte[] Hash(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);

    public static void EnsureDefaultAdmin()
    {
        long count;
        using (var c = Db.Open())
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM users";
            count = (long)cmd.ExecuteScalar()!;
        }
        if (count == 0) Create("admin", "admin", "admin");
    }

    public static void Create(string username, string password, string role)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO users(username,salt,hash,role) VALUES($u,$s,$h,$r)";
        cmd.Parameters.AddWithValue("$u", username);
        cmd.Parameters.AddWithValue("$s", Convert.ToBase64String(salt));
        cmd.Parameters.AddWithValue("$h", Convert.ToBase64String(Hash(password, salt)));
        cmd.Parameters.AddWithValue("$r", role);
        cmd.ExecuteNonQuery();
    }

    public static User? Authenticate(string username, string password)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT id,username,salt,hash,role FROM users WHERE username=$u AND active=1";
        cmd.Parameters.AddWithValue("$u", username);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        var expected = Convert.FromBase64String(r.GetString(3));
        var actual = Hash(password, Convert.FromBase64String(r.GetString(2)));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual)) return null;
        return new User { Id = r.GetInt64(0), Username = r.GetString(1), Role = r.GetString(4) };
    }

    public static List<User> List()
    {
        var list = new List<User>();
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT id,username,role,active FROM users ORDER BY username";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new User
            {
                Id = r.GetInt64(0), Username = r.GetString(1),
                Role = r.GetString(2), Active = r.GetInt32(3) == 1
            });
        return list;
    }

    public static void ResetPassword(long id, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE users SET salt=$s, hash=$h WHERE id=$id";
        cmd.Parameters.AddWithValue("$s", Convert.ToBase64String(salt));
        cmd.Parameters.AddWithValue("$h", Convert.ToBase64String(Hash(password, salt)));
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public static void SetActive(long id, bool active)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE users SET active=$a WHERE id=$id";
        cmd.Parameters.AddWithValue("$a", active ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}