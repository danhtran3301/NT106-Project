using System;

namespace TimeFlow.Models
{
    // Token xac thuc cho user
    public class UserToken
    {
        public int TokenId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;

        public UserToken()
        {
            CreatedAt = DateTime.Now;
            IsRevoked = false;
        }

        // Tao token moi voi thoi gian het han
        public static UserToken CreateToken(int userId, string token, int expirationHours = 24)
        {
            return new UserToken
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(expirationHours),
                IsRevoked = false
            };
        }

        // Kiem tra token con hieu luc khong
        public bool IsValid => !IsRevoked && DateTime.Now < ExpiresAt;

        // Kiem tra token het han chua
        public bool IsExpired => DateTime.Now >= ExpiresAt;

        // Thu hoi token
        public void Revoke()
        {
            IsRevoked = true;
            RevokedAt = DateTime.Now;
        }

        // Thoi gian con lai den khi het han
        public TimeSpan TimeUntilExpiration => ExpiresAt - DateTime.Now;

        // So gio con lai den khi het han
        public int HoursUntilExpiration => (int)TimeUntilExpiration.TotalHours;
    }
}
