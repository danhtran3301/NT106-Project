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

-- Thêm tài khoản mẫu (admin1 / 123456 - đã mã hóa SHA256)
INSERT INTO Users (Username, Password, Email) VALUES
('admin1', '8d969eef6ecad3c29a3a629280e686cff8ca38c6a3f6f6f6d8da0fdd7e143c0e', 'admin@example.com');
GO

-- Tạo login + user có thể đăng nhập và sử dụng database
CREATE LOGIN myuser WITH PASSWORD = 'YourStrong@Passw0rd';
GO

USE UserDB;
GO

CREATE USER myuser FOR LOGIN myuser;
GO

ALTER ROLE db_owner ADD MEMBER myuser;
GO
