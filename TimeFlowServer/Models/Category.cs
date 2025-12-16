using System;
using System.Drawing;

namespace TimeFlow.Models
{
    // Phan loai cong viec theo chu de
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Color { get; set; } = "#6B7280"; // Mau xam mac dinh
        public string? IconName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDefault { get; set; }

        // Navigation properties
        public List<TaskItem> Tasks { get; set; } = new();

        public Category()
        {
            CreatedAt = DateTime.Now;
            IsDefault = false;
        }

        // Chuyen doi hex color sang System.Drawing.Color
        public Color GetColor()
        {
            try
            {
                return ColorTranslator.FromHtml(Color);
            }
            catch
            {
                return ColorTranslator.FromHtml("#6B7280"); // Mau xam mac dinh
            }
        }

        // Kiem tra color co dung dinh dang hex khong
        public bool IsValidColor()
        {
            return System.Text.RegularExpressions.Regex.IsMatch(Color, @"^#[0-9A-Fa-f]{6}$");
        }
    }
}
