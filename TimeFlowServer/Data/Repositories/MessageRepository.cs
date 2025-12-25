using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TimeFlow.Data.Configuration;

namespace TimeFlow.Data.Repositories
{
    // Class DTO để chứa dữ liệu tin nhắn
    public class MessageData
    {
        public string Sender { get; set; } = "";
        public string Receiver { get; set; } = "";
        public int? GroupId { get; set; }
        public string Content { get; set; } = "";
        public DateTime Time { get; set; }
    }

    public class MessageRepository
    {
        private readonly string _connectionString;
        private DatabaseHelper dbHelper;

        public MessageRepository(string connectionString)
        {
            _connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? DbConfig.GetConnectionString()
                : connectionString;
        }

        public MessageRepository(DatabaseHelper dbHelper)
        {
            this.dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _connectionString = DbConfig.GetConnectionString();
        }

        // 1. Gửi tin nhắn cá nhân (1-1)
        public void AddMessage(string sender, string receiver, string content)
        {
            var connStr = _connectionString ?? DbConfig.GetConnectionString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                // ✅ Khớp với schema: SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage
                string query = @"INSERT INTO Messages 
                    (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, CreatedAt) 
                    VALUES (@s, @r, @c, 0, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", sender);
                    cmd.Parameters.AddWithValue("@r", receiver);
                    cmd.Parameters.AddWithValue("@c", content);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 2. Gửi tin nhắn nhóm
        public void AddGroupMessage(string sender, int groupId, string content)
        {
            var connStr = _connectionString ?? DbConfig.GetConnectionString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                // ✅ Khớp với schema: IsGroupMessage = 1, GroupId có giá trị
                string query = @"INSERT INTO Messages 
                    (SenderUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt) 
                    VALUES (@s, @c, 1, @g, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", sender);
                    cmd.Parameters.AddWithValue("@c", content);
                    cmd.Parameters.AddWithValue("@g", groupId);
                    cmd.Parameters.AddWithValue("@c", content);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 3. Lấy lịch sử chat 1-1 giữa 2 user
        public List<MessageData> GetHistory(string user1, string user2)
        {
            var list = new List<MessageData>();
            var connStr = _connectionString ?? DbConfig.GetConnectionString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT SenderUsername, ReceiverUsername, MessageContent, CreatedAt
                                 FROM Messages 
                                WHERE IsGroupMessage = 0 
                                    AND ((SenderUsername = @u1 AND ReceiverUsername = @u2)
                                    OR (SenderUsername = @u2 AND ReceiverUsername = @u1))
                                ORDER BY CreatedAt ASC";

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
                                Receiver = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Content = reader.GetString(2),
                                Time = reader.GetDateTime(3)
                            });
                        }
                    }
                }
            }
            return list;
        }

        // 4. Lấy lịch sử chat nhóm
        public List<MessageData> GetGroupHistory(int groupId)
        {
            var list = new List<MessageData>();
            var connStr = _connectionString ?? DbConfig.GetConnectionString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT SenderUsername, GroupId, MessageContent, CreatedAt
                                 FROM Messages 
                                WHERE IsGroupMessage = 1 AND GroupId = @g
                                ORDER BY CreatedAt ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@g", groupId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new MessageData
                            {
                                Sender = reader.GetString(0),
                                GroupId = reader.GetInt32(1),
                                Content = reader.GetString(2),
                                Time = reader.GetDateTime(3)
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}