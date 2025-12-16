# TimeFlow Database Setup Guide

## Yêu cầu

**Option 1: SQL Server trên Windows**
- SQL Server 2019 trở lên (hoặc SQL Server Express)
- SQL Server Management Studio (SSMS) hoặc Azure Data Studio

**Option 2: SQL Server trên Docker (Recommended cho Development)}
- Docker Desktop (Windows/Mac/Linux)
- SQL Server 2022 Container

---

## Setup với Docker (Recommended)

### Bước 0: Cài đặt Docker Desktop

1. Download Docker Desktop: [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
2. Cài đặt và khởi động Docker Desktop
3. Verify Docker đang chạy:
   ```bash
   docker --version
   ```

### Bước 1: Chạy SQL Server Container

```bash
# Pull và run SQL Server 2022 container
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name timeflow-sqlserver --hostname timeflow-sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

**Giải thích parameters:**
- `-e "ACCEPT_EULA=Y"` - Chấp nhận license
- `-e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd"` - Set SA password
- `-p 1433:1433` - Map port 1433 (host:container)
- `--name timeflow-sqlserver` - Tên container
- `-d` - Run ở background
- `mcr.microsoft.com/mssql/server:2022-latest` - Image SQL Server 2022

### Bước 2: Verify Container đang chạy

```bash
# Check container status
docker ps

# Expected output:
# CONTAINER ID   IMAGE                                        STATUS
# xxxxx          mcr.microsoft.com/mssql/server:2022-latest   Up X minutes
```

### Bước 3: Connect đến SQL Server

**Connection String:**
```
Server=localhost,1433;Database=master;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

**Với Azure Data Studio / SSMS:**
- Server: `localhost,1433`
- Authentication: SQL Server Authentication
- Login: `sa`
- Password: `YourStrong@Passw0rd`

### Bước 4: Tạo SQL User cho TimeFlow

```sql
-- Connect to master database và run:
CREATE LOGIN myuser WITH PASSWORD = 'YourStrong@Passw0rd';
GO
```

### Docker Commands Hữu Ích

```bash
# Stop container
docker stop timeflow-sqlserver

# Start container (sau khi stop)
docker start timeflow-sqlserver

# Xem logs
docker logs timeflow-sqlserver

# Xóa container (cẩn thận - mất hết data!)
docker rm -f timeflow-sqlserver

# Execute command trong container
docker exec -it timeflow-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd
```

### Lưu ý về Data Persistence

Container mặc định **KHÔNG lưu data** khi xóa. Để persist data:

```bash
# Run với volume để lưu data
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  --name timeflow-sqlserver \
  -v timeflow-data:/var/opt/mssql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

---

## Cách Setup Database

### Bước 1: Tạo Database Schema

Chạy file `TimeFlowDB_Schema.sql` trong SSMS hoặc Azure Data Studio:

1. Mở SSMS/Azure Data Studio và connect đến SQL Server
   - Nếu dùng Docker: Server = `localhost,1433`
2. Mở file `TimeFlowDB_Schema.sql`
3. Click **Execute** (F5)

Script sẽ:
- Tạo database `TimeFlowDB`
- Tạo 9 tables
- Tạo 3 views
- Tạo 4 stored procedures
- Tạo 1 function
- Tạo 2 triggers
- Insert 6 default categories

### Bước 2: Tạo SQL User và Grant Permissions

```sql
-- Run in master database
USE master;
GO

-- Create login if not exists
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'myuser')
BEGIN
    CREATE LOGIN myuser WITH PASSWORD = 'YourStrong@Passw0rd';
END
GO

-- Switch to TimeFlowDB
USE TimeFlowDB;
GO

-- Create user and grant permissions
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'myuser')
BEGIN
    CREATE USER myuser FOR LOGIN myuser;
END
GO

-- Grant full permissions
ALTER ROLE db_owner ADD MEMBER myuser;
GO

PRINT 'User "myuser" created and granted db_owner role';
GO
```

### Bước 3: Insert Test Data

Chạy file `TestData.sql` trong SSMS/Azure Data Studio:

1. Mở file `TestData.sql`
2. Click **Execute** (F5)

Script sẽ:
- **Tự động xóa dữ liệu cũ** (nếu có)
- Reset identity seeds về 0
- Tạo 3 users với password: `Test@1234`
- Tạo 8 personal tasks
- Tạo 4 group tasks
- Tạo 2 groups với members
- Tạo 4 comments
- Tạo 7 activity logs

---

## Xóa Dữ Liệu Test

### Option 1: Chạy lại TestData.sql
TestData.sql tự động xóa dữ liệu cũ trước khi insert mới.

### Option 2: Dùng CleanData.sql (Xóa sạch hoàn toàn)
```sql
-- Chỉ xóa data, giữ nguyên database structure
TimeFlow/Database/CleanData.sql
```

Script sẽ:
- Xóa TÂT CẢ users
- Xóa TÂT CẢ tasks
- Xóa TÂT CẢ groups
- Xóa TÂT CẢ comments & activity logs
- Reset identity seeds về 0
- **GIỮ NGUYÊN** 6 default categories

---

## Test Accounts

### 1. Admin Account

```
Username: admin
Password: Test@1234
Email: admin@timeflow.com
```

**Dữ liệu:**
- ✅ 6 tasks cá nhân:
  - 1 overdue (High priority)
  - 2 due today (Medium priority)
  - 1 completed (High priority)
  - 2 upcoming tasks
- ✅ Admin của Development Team
- ✅ Admin của QA Team
- ✅ Nhiều activity logs

**Dùng để test:**
- Dashboard với nhiều tasks
- Task filtering (overdue, today, priority)
- Group management
- Task assignment

---

### 2. Test User Account

```
Username: testuser
Password: Test@1234
Email: test@timeflow.com
```

**Dữ liệu:**
- ✅ 2 tasks cá nhân (shopping, study)
- ✅ Member của Development Team
- ✅ Creator/Admin của QA Team
- ✅ 1 group task assigned (authentication)

**Dùng để test:**
- Normal user workflow
- Group task assignment
- Comment trên tasks

---

### 3. Demo User Account

```
Username: demouser
Password: Test@1234
Email: demo@timeflow.com
```

**Dữ liệu:**
- ✅ Không có tasks cá nhân (clean slate)
- ✅ Member của Development Team
- ✅ Member của QA Team
- ✅ 1 group task assigned (UI design)

**Dùng để test:**
- New user experience
- Group collaboration
- Task receiving

---

## Database Connection String

### Docker / SQL Server với SQL Authentication:
```
Server=localhost,1433;Database=TimeFlowDB;User Id=myuser;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Integrated Security=False;
```

### Windows với SQL Server Authentication:
```
Data Source=localhost;Initial Catalog=TimeFlowDB;User ID=myuser;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

### Windows Authentication (chỉ Windows):
```
Data Source=localhost;Initial Catalog=TimeFlowDB;Integrated Security=True;TrustServerCertificate=True
```

Connection string này đã được config trong:
- `TimeFlow/Data/Configuration/DbConfig.cs`
- `TimeFlow/Data/DatabaseHelper.cs`

---

## Verify Database Setup

Chạy các queries sau để verify:

```sql
-- Check users
SELECT UserId, Username, Email, IsActive FROM Users;

-- Check categories
SELECT CategoryId, CategoryName, Color, IsDefault FROM Categories;

-- Check tasks
SELECT TaskId, Title, Priority, Status, IsGroupTask, CreatedBy 
FROM Tasks 
ORDER BY CreatedAt DESC;

-- Check groups
SELECT g.GroupId, g.GroupName, COUNT(gm.GroupMemberId) AS MemberCount
FROM Groups g
LEFT JOIN GroupMembers gm ON g.GroupId = gm.GroupId AND gm.IsActive = 1
GROUP BY g.GroupId, g.GroupName;

-- Check admin user dashboard
EXEC sp_GetUserAllTasks @UserId = 1; -- Admin user
```

**Expected results:**
- 3 users
- 6 categories
- 12 tasks (8 personal + 4 group)
- 2 groups
- 7 group members
- 4 comments

---

## Database Scripts Overview

| Script | Purpose | When to Use |
|--------|---------|-------------|
| **TimeFlowDB_Schema.sql** | Tạo database structure | Lần đầu setup hoặc rebuild database |
| **TestData.sql** | Insert test data (auto-clean old data) | Thường xuyên - reset về data mẫu |
| **CleanData.sql** | Xóa sạch TẤT CẢ data | Khi muốn bắt đầu với database rỗng |
| **DebugCheck.sql** | Kiểm tra database health | Khi gặp lỗi, troubleshooting |
| **VerifyPasswordHash.sql** | Kiểm tra password hash | Khi login fail |

---

## Password Hash

Tất cả users dùng password: `Test@1234`

SHA256 Hash: `849f1575ccfbf3a4d6cf00e6c5641b7fd4da2ed3e212c2d79ba9161a5a432ff0`

Code hash trong C#:
```csharp
private string HashPassword(string password)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytes) 
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
```

---

## Troubleshooting

### Lỗi: Cannot connect to Docker SQL Server

**Check:**
```bash
# 1. Docker Desktop đang chạy
docker ps

# 2. Container đang chạy
docker ps | grep timeflow-sqlserver

# 3. Xem logs nếu container crashed
docker logs timeflow-sqlserver

# 4. Restart container
docker restart timeflow-sqlserver
```

### Lỗi: Database 'TimeFlowDB' already exists

**Solution 1:** Xóa database cũ trước:
```sql
USE master;
ALTER DATABASE TimeFlowDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE TimeFlowDB;
```

**Solution 2:** Giữ database, chỉ reset data:
```sql
-- Chạy CleanData.sql để xóa hết data
-- Sau đó chạy TestData.sql để insert lại
```

### Lỗi: Cannot connect to SQL Server

**Check:**
1. SQL Server service đang chạy (hoặc Docker container)
2. TCP/IP protocol enabled (SQL Server Configuration Manager)
3. Firewall không block port 1433
4. Với Docker: `docker ps` để check container status

### Lỗi: Login failed for user

**Docker / SQL Auth:**
- Check password: `YourStrong@Passw0rd`
- Verify user exists: Run create user script ở Bước 2

**Windows:**
- Check SQL Server authentication mode (Mixed Mode)
- User enabled và có permissions

### Lỗi: Foreign Key constraint khi xóa data

**Solution:**
Dùng `CleanData.sql` - script này xóa theo đúng thứ tự foreign key.

---

## Testing Workflow

### Test 1: Login
```
1. Chạy Server (FormMenuTCP)
2. Start Server trên port 1010
3. Chạy Client
4. Login với: admin / Test@1234
5. Verify: User info hiển thị đúng
```

### Test 2: View Tasks
```
1. Login với admin
2. View dashboard
3. Verify: 6 tasks hiển thị
4. Check: 1 task overdue (màu đỏ)
5. Check: 2 tasks today (màu vàng)
```

### Test 3: Group Tasks
```
1. Login với testuser
2. View groups
3. Verify: 2 groups (Dev Team, QA Team)
4. Click Dev Team
5. Verify: 3 group tasks
6. Check: 1 task assigned to testuser
```

### Test 4: Create New Task
```
1. Login với demouser (clean account)
2. Create new personal task
3. Verify: Task xuất hiện trong list
4. Check: ActivityLog ghi lại
```

---

## Next Steps

Sau khi setup database xong:

1. ✅ Chạy `DebugCheck.sql` để verify
2. ✅ Test login với 3 accounts
3. ✅ Verify data hiển thị đúng
4. ✅ Test CRUD operations
5. ✅ Test group functionality
6. ✅ Test comments & activity logs

---

## Reset Database to Fresh State

**Quick Reset (Keep structure, new data):**
```
1. Run TestData.sql (auto-cleans old data)
```

**Full Reset (Clean everything):**
```
1. Run CleanData.sql
2. Run TestData.sql
```

**Complete Rebuild:**
```
1. Drop database in SSMS
2. Run TimeFlowDB_Schema.sql
3. Run TestData.sql
```

**Docker Fresh Start:**
```bash
# Xóa container và data
docker rm -f timeflow-sqlserver

# Tạo container mới
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name timeflow-sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Sau đó run Schema + TestData scripts
```

---

**Version:** 1.2  
**Last Updated:** 2025  
**Status:** Ready for Testing ✅  
**Docker Support:** ✅ Added
