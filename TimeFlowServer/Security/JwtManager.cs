using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TimeFlowServer.Security
{
    // Quan ly tao va xac thuc JWT token
    public class JwtManager
    {
        private readonly string _secretKey;

        public JwtManager(string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
                throw new ArgumentException("Secret key must be at least 32 characters long", nameof(secretKey));
            
            _secretKey = secretKey;
        }

        // Tao JWT token cho username voi thoi gian het han
        public string CreateToken(string username, int expirationMinutes = 60)
        {
            var header = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" });
            var payload = JsonSerializer.Serialize(new
            {
                username = username,
                exp = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds(),
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            string encodedHeader = Base64UrlEncode(header);
            string encodedPayload = Base64UrlEncode(payload);
            string signature = HmacSha256($"{encodedHeader}.{encodedPayload}", _secretKey);

            return $"{encodedHeader}.{encodedPayload}.{signature}";
        }

        // Xac thuc JWT token va lay username
        public bool ValidateToken(string token, out string? username)
        {
            username = null;
            
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return false;

                string header = parts[0];
                string payload = parts[1];
                string signature = parts[2];

                // Xac thuc signature
                string expectedSignature = HmacSha256($"{header}.{payload}", _secretKey);
                if (signature != expectedSignature) return false;

                // Giai ma payload
                string payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(payload)));
                using var doc = JsonDocument.Parse(payloadJson);

                // Kiem tra het han
                long exp = doc.RootElement.GetProperty("exp").GetInt64();
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp) return false;

                // Lay username
                username = doc.RootElement.GetProperty("username").GetString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Lay thoi gian het han cua token
        public DateTime? GetTokenExpiration(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return null;

                string payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(parts[1])));
                using var doc = JsonDocument.Parse(payloadJson);

                long exp = doc.RootElement.GetProperty("exp").GetInt64();
                return DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
            }
            catch
            {
                return null;
            }
        }

        private string PadBase64(string base64)
        {
            return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=')
                         .Replace('-', '+')
                         .Replace('_', '/');
        }

        private string Base64UrlEncode(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
                          .Replace("=", "")
                          .Replace('+', '-')
                          .Replace('/', '_');
        }

        private string HmacSha256(string data, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)))
                              .Replace("=", "")
                              .Replace('+', '-')
                              .Replace('/', '_');
            }
        }
    }
}
