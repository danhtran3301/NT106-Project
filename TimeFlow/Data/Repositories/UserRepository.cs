using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly Users table
    public class UserRepository : BaseRepository
    {
        public UserRepository() : base() { }
        public UserRepository(DatabaseHelper db) : base(db) { }

        // ================== MAPPING ==================

        // Chuyen DataRow thanh User object
        private User MapToUser(DataRow row)
        {
            return new User
            {
                UserId = GetValue<int>(row, "UserId"),
                Username = GetValue<string>(row, "Username", string.Empty),
                Email = GetValue<string>(row, "Email", string.Empty),
                PasswordHash = GetValue<string>(row, "PasswordHash", string.Empty),
                FullName = GetString(row, "FullName"),
                AvatarUrl = GetString(row, "AvatarUrl"),
                CreatedAt = GetValue<DateTime>(row, "CreatedAt"),
                UpdatedAt = GetNullableValue<DateTime>(row, "UpdatedAt"),
                IsActive = GetValue<bool>(row, "IsActive", true),
                LastLoginAt = GetNullableValue<DateTime>(row, "LastLoginAt")
            };
        }


        // Lay user theo ID
        public User? GetById(int userId)
        {
            var query = "SELECT * FROM Users WHERE UserId = @id";
            var parameters = CreateParameters(("@id", userId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToUser(row) : null;
        }

        // Lay user theo username
        public User? GetByUsername(string username)
        {
            var query = "SELECT * FROM Users WHERE Username = @username";
            var parameters = CreateParameters(("@username", username));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToUser(row) : null;
        }

        // Lay user theo email
        public User? GetByEmail(string email)
        {
            var query = "SELECT * FROM Users WHERE Email = @email";
            var parameters = CreateParameters(("@email", email));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToUser(row) : null;
        }

        // Validate login va tra ve User neu thanh cong
        public User? ValidateLogin(string username, string passwordHash)
        {
            var query = @"SELECT * FROM Users 
                         WHERE Username = @username 
                         AND PasswordHash = @password 
                         AND IsActive = 1";
            
            var parameters = CreateParameters(
                ("@username", username),
                ("@password", passwordHash)
            );
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToUser(row) : null;
        }

        // Lay tat ca users active
        public List<User> GetAllActive()
        {
            var query = "SELECT * FROM Users WHERE IsActive = 1 ORDER BY Username";
            var rows = GetRows(query);
            
            var users = new List<User>();
            foreach (DataRow row in rows)
            {
                users.Add(MapToUser(row));
            }
            return users;
        }

        // Tim kiem users theo keyword (username hoac email)
        public List<User> Search(string keyword)
        {
            var query = @"SELECT * FROM Users 
                         WHERE IsActive = 1 
                         AND (Username LIKE @keyword OR Email LIKE @keyword)
                         ORDER BY Username";
            
            var parameters = CreateParameters(("@keyword", $"%{keyword}%"));
            var rows = GetRows(query, parameters);
            
            var users = new List<User>();
            foreach (DataRow row in rows)
            {
                users.Add(MapToUser(row));
            }
            return users;
        }

        // Tao user moi
        public int Create(User user)
        {
            var query = @"INSERT INTO Users 
                         (Username, Email, PasswordHash, FullName, AvatarUrl, IsActive, CreatedAt)
                         VALUES 
                         (@username, @email, @password, @fullname, @avatar, @active, GETDATE())";
            
            var parameters = CreateParameters(
                ("@username", user.Username),
                ("@email", user.Email),
                ("@password", user.PasswordHash),
                ("@fullname", user.FullName),
                ("@avatar", user.AvatarUrl),
                ("@active", user.IsActive)
            );
            
            return InsertAndGetId(query, parameters);
        }



        // Cap nhat thong tin user
        public bool Update(User user)
        {
            var query = @"UPDATE Users SET 
                         Username = @username,
                         Email = @email,
                         PasswordHash = @password,
                         FullName = @fullname,
                         AvatarUrl = @avatar,
                         IsActive = @active,
                         UpdatedAt = GETDATE()
                         WHERE UserId = @id";
            
            var parameters = CreateParameters(
                ("@id", user.UserId),
                ("@username", user.Username),
                ("@email", user.Email),
                ("@password", user.PasswordHash),
                ("@fullname", user.FullName),
                ("@avatar", user.AvatarUrl),
                ("@active", user.IsActive)
            );
            
            return Execute(query, parameters) > 0;
        }

        // Cap nhat mat khau
        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            var query = @"UPDATE Users SET 
                         PasswordHash = @password,
                         UpdatedAt = GETDATE()
                         WHERE UserId = @id";
            
            var parameters = CreateParameters(
                ("@id", userId),
                ("@password", newPasswordHash)
            );
            
            return Execute(query, parameters) > 0;
        }

        // Cap nhat thoi gian login gan nhat
        public bool UpdateLastLogin(int userId)
        {
            var query = "UPDATE Users SET LastLoginAt = GETDATE() WHERE UserId = @id";
            var parameters = CreateParameters(("@id", userId));
            
            return Execute(query, parameters) > 0;
        }

        // Cap nhat trang thai active
        public bool UpdateActiveStatus(int userId, bool isActive)
        {
            var query = @"UPDATE Users SET 
                         IsActive = @active,
                         UpdatedAt = GETDATE()
                         WHERE UserId = @id";
            
            var parameters = CreateParameters(
                ("@id", userId),
                ("@active", isActive)
            );
            
            return Execute(query, parameters) > 0;
        }



        // Xoa user (soft delete - set IsActive = false)
        public bool SoftDelete(int userId)
        {
            return UpdateActiveStatus(userId, false);
        }

        // Xoa user (hard delete - xoa thuc su)
        public bool Delete(int userId)
        {
            var query = "DELETE FROM Users WHERE UserId = @id";
            var parameters = CreateParameters(("@id", userId));
            
            return Execute(query, parameters) > 0;
        }

        // ================== CHECK OPERATIONS ==================

        // Kiem tra username da ton tai chua
        public bool UsernameExists(string username)
        {
            return Exists("Users", "Username = @username", 
                CreateParameters(("@username", username)));
        }

        // Kiem tra email da ton tai chua
        public bool EmailExists(string email)
        {
            return Exists("Users", "Email = @email", 
                CreateParameters(("@email", email)));
        }

        // Kiem tra username hoac email da ton tai chua (dung cho register)
        public bool UsernameOrEmailExists(string username, string email)
        {
            return Exists("Users", "Username = @username OR Email = @email", 
                CreateParameters(
                    ("@username", username),
                    ("@email", email)
                ));
        }


        // Dem so luong users
        public int CountUsers(bool? activeOnly = null)
        {
            var query = activeOnly.HasValue
                ? "SELECT COUNT(*) FROM Users WHERE IsActive = @active"
                : "SELECT COUNT(*) FROM Users";
            
            var parameters = activeOnly.HasValue
                ? CreateParameters(("@active", activeOnly.Value))
                : null;
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
