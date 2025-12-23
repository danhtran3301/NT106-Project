using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace TimeFlow.Data.Repositories
{
    public class MessageData // Class DTO đơn giản
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
    }

    public class MessageRepository
    {
        private readonly string _connectionString;

        public MessageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Hàm lưu tin nhắn mới
        public void AddMessage(string sender, string receiver, string content)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Messages (SenderUsername, ReceiverUsername, Content, Timestamp) VALUES (@s, @r, @c, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@s", sender);
                        cmd.Parameters.AddWithValue("@r", receiver);
                        cmd.Parameters.AddWithValue("@c", content);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi vào console hoặc file nếu cần
                Console.WriteLine("Error saving message: " + ex.Message);
            }
        }

        // Hàm lấy lịch sử chat giữa 2 người (Dùng cho Bước 2)
        public List<MessageData> GetHistory(string user1, string user2)
        {
            var list = new List<MessageData>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Lấy tin nhắn chiều đi và chiều về, sắp xếp theo thời gian
                    string query = @"SELECT SenderUsername, ReceiverUsername, Content, Timestamp 
                                     FROM Messages 
                                     WHERE (SenderUsername = @u1 AND ReceiverUsername = @u2) 
                                        OR (SenderUsername = @u2 AND ReceiverUsername = @u1)
                                     ORDER BY Timestamp ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u1", user1);
                        cmd.Parameters.AddWithValue("@u2", user2);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new MessageData
                                {
                                    Sender = reader.GetString(0),
                                    Receiver = reader.GetString(1),
                                    Content = reader.GetString(2),
                                    Time = reader.GetDateTime(3)
                                });
                            }
                        }
                    }
                }
            }
            catch { }
            return list;
        }
    }
}