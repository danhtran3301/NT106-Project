using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly Categories table
    public class CategoryRepository : BaseRepository
    {
        public CategoryRepository() : base() { }
        public CategoryRepository(DatabaseHelper db) : base(db) { }

        // ================== MAPPING ==================

        private Category MapToCategory(DataRow row)
        {
            return new Category
            {
                CategoryId = GetValue<int>(row, "CategoryId"),
                CategoryName = GetValue<string>(row, "CategoryName", string.Empty),
                Color = GetValue<string>(row, "Color", "#6B7280"),
                IconName = GetString(row, "IconName"),
                CreatedAt = GetValue<DateTime>(row, "CreatedAt"),
                IsDefault = GetValue<bool>(row, "IsDefault", false)
            };
        }

        // ================== READ OPERATIONS ==================

        // Lay category theo ID
        public Category? GetById(int categoryId)
        {
            var query = "SELECT * FROM Categories WHERE CategoryId = @id";
            var parameters = CreateParameters(("@id", categoryId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToCategory(row) : null;
        }

        // Lay category theo ten
        public Category? GetByName(string categoryName)
        {
            var query = "SELECT * FROM Categories WHERE CategoryName = @name";
            var parameters = CreateParameters(("@name", categoryName));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToCategory(row) : null;
        }

        // Lay tat ca categories
        public List<Category> GetAll()
        {
            var query = "SELECT * FROM Categories ORDER BY IsDefault DESC, CategoryName";
            var rows = GetRows(query);
            
            var categories = new List<Category>();
            foreach (DataRow row in rows)
            {
                categories.Add(MapToCategory(row));
            }
            return categories;
        }

        // Lay cac categories mac dinh
        public List<Category> GetDefaultCategories()
        {
            var query = "SELECT * FROM Categories WHERE IsDefault = 1 ORDER BY CategoryName";
            var rows = GetRows(query);
            
            var categories = new List<Category>();
            foreach (DataRow row in rows)
            {
                categories.Add(MapToCategory(row));
            }
            return categories;
        }

        // ================== CREATE OPERATIONS ==================

        // Tao category moi
        public int Create(Category category)
        {
            var query = @"INSERT INTO Categories 
                         (CategoryName, Color, IconName, IsDefault, CreatedAt)
                         VALUES 
                         (@name, @color, @icon, @default, GETDATE())";
            
            var parameters = CreateParameters(
                ("@name", category.CategoryName),
                ("@color", category.Color),
                ("@icon", category.IconName),
                ("@default", category.IsDefault)
            );
            
            return InsertAndGetId(query, parameters);
        }

        // ================== UPDATE OPERATIONS ==================

        // Cap nhat category
        public bool Update(Category category)
        {
            var query = @"UPDATE Categories SET 
                         CategoryName = @name,
                         Color = @color,
                         IconName = @icon
                         WHERE CategoryId = @id";
            
            var parameters = CreateParameters(
                ("@id", category.CategoryId),
                ("@name", category.CategoryName),
                ("@color", category.Color),
                ("@icon", category.IconName)
            );
            
            return Execute(query, parameters) > 0;
        }

        // ================== DELETE OPERATIONS ==================

        // Xoa category (chi xoa neu khong phai default va khong co tasks)
        public bool Delete(int categoryId)
        {
            // Kiem tra xem co phai default category khong
            var category = GetById(categoryId);
            if (category?.IsDefault == true)
                return false; // Khong cho xoa default categories

            // Kiem tra xem co tasks nao dang dung category nay khong
            var checkQuery = "SELECT COUNT(*) FROM Tasks WHERE CategoryId = @id";
            var checkParams = CreateParameters(("@id", categoryId));
            var taskCount = _db.ExecuteScalar(checkQuery, checkParams);
            
            if (taskCount != null && Convert.ToInt32(taskCount) > 0)
                return false; // Khong cho xoa neu con tasks dang dung

            // Xoa category
            var query = "DELETE FROM Categories WHERE CategoryId = @id";
            var parameters = CreateParameters(("@id", categoryId));
            
            return Execute(query, parameters) > 0;
        }

        // ================== CHECK OPERATIONS ==================

        // Kiem tra ten category da ton tai chua
        public bool CategoryNameExists(string categoryName)
        {
            return Exists("Categories", "CategoryName = @name", 
                CreateParameters(("@name", categoryName)));
        }
    }
}
