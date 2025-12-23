using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TimeFlow.Data.Configuration;
using Serilog;

namespace TimeFlow.Data.Repositories
{
    public class ContactRepository
    {
        private readonly string _connectionString;

        public ContactRepository()
        {
            _connectionString = DbConfig.GetConnectionString();
        }

        /// <summary>
        /// Thêm một người dùng vào danh bạ của người dùng hiện tại.
        /// </summary>
        /// <param name="userId">ID người dùng chủ (người thêm)</param>
        /// <param name="contactUserId">ID người dùng được thêm (người bạn)</param>
        /// <returns>True nếu thêm thành công, False nếu đã tồn tại hoặc lỗi.</returns>
        public bool AddContact(int userId, int contactUserId)
        {
            // Không cho phép tự thêm chính mình
            if (userId == contactUserId) return false;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Kiểm tra xem đã tồn tại chưa
                    string checkQuery = "SELECT COUNT(1) FROM Contacts WHERE UserId = @UserId AND ContactUserId = @ContactUserId";
                    using (var checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        checkCmd.Parameters.AddWithValue("@ContactUserId", contactUserId);
                        int exists = (int)checkCmd.ExecuteScalar();

                        if (exists > 0) return false; // Đã có trong danh bạ
                    }

                    // Thêm mới
                    string insertQuery = @"
                        INSERT INTO Contacts (UserId, ContactUserId, CreatedAt) 
                        VALUES (@UserId, @ContactUserId, @CreatedAt)";

                    using (var insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@UserId", userId);
                        insertCmd.Parameters.AddWithValue("@ContactUserId", contactUserId);
                        insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        int rows = insertCmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error adding contact: {userId} -> {contactUserId}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách Username của bạn bè trong danh bạ.
        /// </summary>
        /// <param name="userId">ID người dùng cần lấy danh bạ</param>
        /// <returns>Danh sách các username (List string)</returns>
        public List<string> GetContactUsernames(int userId)
        {
            var contacts = new List<string>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Join bảng Contacts với Users để lấy Username
                    string query = @"
                        SELECT u.Username 
                        FROM Contacts c
                        INNER JOIN Users u ON c.ContactUserId = u.UserId
                        WHERE c.UserId = @UserId
                        ORDER BY u.Username ASC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                contacts.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error getting contacts for user {userId}");
            }

            return contacts;
        }

        /// <summary>
        /// Xóa một liên hệ khỏi danh bạ.
        /// </summary>
        public bool RemoveContact(int userId, int contactUserId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Contacts WHERE UserId = @UserId AND ContactUserId = @ContactUserId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@ContactUserId", contactUserId);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error removing contact: {userId} -> {contactUserId}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem người B có trong danh bạ người A không.
        /// </summary>
        public bool IsContact(int userId, int targetUserId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(1) FROM Contacts WHERE UserId = @UserId AND ContactUserId = @TargetUserId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@TargetUserId", targetUserId);
                        return (int)cmd.ExecuteScalar() > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}