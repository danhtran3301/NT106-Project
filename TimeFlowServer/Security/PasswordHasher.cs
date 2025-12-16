using System.Security.Cryptography;
using System.Text;

namespace TimeFlowServer.Security
{
    // Xu ly hash password va xac thuc su dung SHA256
    public static class PasswordHasher
    {
        // Hash password dang plain text su dung SHA256
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // Xac thuc password plain text voi hash
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            string computedHash = HashPassword(password);
            return computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
