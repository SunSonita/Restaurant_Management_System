using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Resturant_Management.Data
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Hashes a plaintext password using SHA-256 with a standard salt.
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return "";

            // Use SHA256 with project salt
            const string salt = "RestoMgmt_2026_SaltKey#99";
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Verifies a password against the stored password hash.
        /// Supports legacy plaintext passwords from initial seed and auto-upgrades them.
        /// </summary>
        public static bool VerifyPassword(string inputPassword, string storedHash, int userId = 0)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            string computedHash = HashPassword(inputPassword);

            // 1. Direct match with computed hash
            if (string.Equals(storedHash, computedHash, StringComparison.OrdinalIgnoreCase))
                return true;

            // 2. Legacy seed plaintext compatibility: "admin123", "manager123", "cashier123", "admin"
            if (string.Equals(storedHash, inputPassword, StringComparison.Ordinal) ||
                (storedHash == "REPLACE_WITH_HASH" && (inputPassword == "admin123" || inputPassword == "admin")))
            {
                // Auto-upgrade stored password in database to secure hash
                if (userId > 0)
                {
                    try
                    {
                        DbHelper.ExecuteNonQuery(
                            "UPDATE dbo.APP_USER SET PasswordHash = @Hash WHERE UserID = @UID",
                            new SqlParameter("@Hash", computedHash),
                            new SqlParameter("@UID", userId));
                    }
                    catch { }
                }
                return true;
            }

            return false;
        }
    }
}
