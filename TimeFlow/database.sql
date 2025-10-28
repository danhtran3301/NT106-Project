-- Tạo database
CREATE DATABASE UserDB;
GO

-- Sử dụng database
USE UserDB;
GO

-- Tạo bảng Users
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Password NVARCHAR(256) NOT NULL,
    Email NVARCHAR(100)
);
GO

-- Thêm dữ liệu mẫu 
INSERT INTO Users (Username, Password, Email) VALUES
('admin1', '8d969eef6ecad3c29a3a629280e686cff8ca38c6a3f6f6f6d8da0fdd7e143c0e', 'admin@example.com');
