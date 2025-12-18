using System;

namespace TimeFlow.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderUsername { get; set; }   // Người gửi
        public string ReceiverUsername { get; set; } // Người nhận
        public string Content { get; set; }          // Nội dung
        public DateTime Timestamp { get; set; }      // Thời gian gửi
        public bool IsRead { get; set; } = false;    // Đã xem chưa (tùy chọn)
    }
}