using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

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

        public MessageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 1. Gửi tin nhắn cá nhân
        public void AddMessage(string sender, string receiver, string content)
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
        public void AddGroupMessage(string sender, int groupId, string content)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Messages (SenderUsername, GroupId, Content, Timestamp) VALUES (@s, @g, @c, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", sender);
                    cmd.Parameters.AddWithValue("@g", groupId);
                    cmd.Parameters.AddWithValue("@c", content);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public List<MessageData> GetHistory(string user1, string user2)
        {
            var list = new List<MessageData>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
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
        public List<MessageData> GetGroupHistory(int groupId)
        {
            var list = new List<MessageData>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"SELECT SenderUsername, GroupId, Content, Timestamp 
                                 FROM Messages 
                                 WHERE GroupId = @g
                                 ORDER BY Timestamp ASC";

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