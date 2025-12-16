-- ====================================================
-- TimeFlow Database Verification & Debug Script
-- Chay script nay de kiem tra database setup
-- ====================================================

USE TimeFlowDB;
GO

PRINT '=================================================';
PRINT 'CHECKING DATABASE SETUP';
PRINT '=================================================';
PRINT '';

-- ====================================================
-- 1. KIEM TRA USERS TABLE
-- ====================================================
PRINT '1. CHECKING USERS TABLE:';
PRINT '-------------------------------------------------';

IF OBJECT_ID('Users', 'U') IS NOT NULL
BEGIN
    PRINT '? Users table exists';
    
    DECLARE @UserCount INT;
    SELECT @UserCount = COUNT(*) FROM Users;
    PRINT '  Total users: ' + CAST(@UserCount AS VARCHAR);
    
    IF @UserCount > 0
    BEGIN
        PRINT '';
        PRINT '  Users list:';
        SELECT 
            UserId,
            Username,
            Email,
            IsActive,
            CreatedAt
        FROM Users;
        
        -- Check password hash
        PRINT '';
        PRINT '  Password hashes:';
        SELECT 
            Username,
            PasswordHash,
            LEN(PasswordHash) AS HashLength
        FROM Users;
    END
    ELSE
    BEGIN
        PRINT '  ? No users found! Run TestData.sql';
    END
END
ELSE
BEGIN
    PRINT '? Users table does NOT exist!';
    PRINT '  Please run TimeFlowDB_Schema.sql first';
END
PRINT '';

-- ====================================================
-- 2. KIEM TRA CATEGORIES TABLE
-- ====================================================
PRINT '2. CHECKING CATEGORIES TABLE:';
PRINT '-------------------------------------------------';

IF OBJECT_ID('Categories', 'U') IS NOT NULL
BEGIN
    PRINT '? Categories table exists';
    
    DECLARE @CategoryCount INT;
    SELECT @CategoryCount = COUNT(*) FROM Categories;
    PRINT '  Total categories: ' + CAST(@CategoryCount AS VARCHAR);
    
    IF @CategoryCount > 0
    BEGIN
        PRINT '';
        PRINT '  Categories list:';
        SELECT CategoryId, CategoryName, Color, IsDefault FROM Categories;
    END
    ELSE
    BEGIN
        PRINT '  ? No categories found!';
    END
END
ELSE
BEGIN
    PRINT '? Categories table does NOT exist!';
END
PRINT '';

-- ====================================================
-- 3. KIEM TRA TASKS TABLE
-- ====================================================
PRINT '3. CHECKING TASKS TABLE:';
PRINT '-------------------------------------------------';

IF OBJECT_ID('Tasks', 'U') IS NOT NULL
BEGIN
    PRINT '? Tasks table exists';
    
    DECLARE @TaskCount INT;
    SELECT @TaskCount = COUNT(*) FROM Tasks;
    PRINT '  Total tasks: ' + CAST(@TaskCount AS VARCHAR);
    
    IF @TaskCount > 0
    BEGIN
        PRINT '';
        PRINT '  Tasks summary:';
        SELECT 
            Status,
            COUNT(*) AS Count,
            CASE Status
                WHEN 1 THEN 'Pending'
                WHEN 2 THEN 'In Progress'
                WHEN 3 THEN 'Completed'
                WHEN 4 THEN 'Cancelled'
            END AS StatusName
        FROM Tasks
        GROUP BY Status;
    END
    ELSE
    BEGIN
        PRINT '  ? No tasks found! Run TestData.sql';
    END
END
ELSE
BEGIN
    PRINT '? Tasks table does NOT exist!';
END
PRINT '';

-- ====================================================
-- 4. KIEM TRA GROUPS TABLE
-- ====================================================
PRINT '4. CHECKING GROUPS TABLE:';
PRINT '-------------------------------------------------';

IF OBJECT_ID('Groups', 'U') IS NOT NULL
BEGIN
    PRINT '? Groups table exists';
    
    DECLARE @GroupCount INT;
    SELECT @GroupCount = COUNT(*) FROM Groups;
    PRINT '  Total groups: ' + CAST(@GroupCount AS VARCHAR);
    
    IF @GroupCount > 0
    BEGIN
        PRINT '';
        SELECT GroupId, GroupName, IsActive FROM Groups;
    END
END
ELSE
BEGIN
    PRINT '? Groups table does NOT exist!';
END
PRINT '';

-- ====================================================
-- 5. TEST LOGIN QUERY
-- ====================================================
PRINT '5. TESTING LOGIN QUERY:';
PRINT '-------------------------------------------------';

DECLARE @TestUsername NVARCHAR(50) = 'admin';
DECLARE @TestPasswordHash NVARCHAR(255) = '849f1575ccfbf3a4d6cf00e6c5641b7fd4da2ed3e212c2d79ba9161a5a432ff0'; -- Test@1234

PRINT 'Testing login for username: ' + @TestUsername;
PRINT 'Using password hash: ' + @TestPasswordHash;
PRINT '';

-- Query that UserRepository.ValidateLogin() uses
SELECT 
    UserId,
    Username,
    Email,
    FullName,
    IsActive,
    CreatedAt
FROM Users 
WHERE Username = @TestUsername 
  AND PasswordHash = @TestPasswordHash 
  AND IsActive = 1;

DECLARE @LoginResult INT;
SELECT @LoginResult = COUNT(*) 
FROM Users 
WHERE Username = @TestUsername 
  AND PasswordHash = @TestPasswordHash 
  AND IsActive = 1;

IF @LoginResult > 0
    PRINT '? Login query successful! User found.';
ELSE
BEGIN
    PRINT '? Login query failed! User not found.';
    PRINT '';
    PRINT 'Checking if user exists with different hash:';
    
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @TestUsername)
    BEGIN
        PRINT '  User exists but password hash does not match!';
        PRINT '  Expected hash: ' + @TestPasswordHash;
        
        SELECT 
            '  Actual hash: ' + PasswordHash AS Info
        FROM Users 
        WHERE Username = @TestUsername;
        
        PRINT '';
        PRINT '  Fix: Run this UPDATE statement:';
        PRINT '  UPDATE Users SET PasswordHash = ''' + @TestPasswordHash + ''' WHERE Username = ''' + @TestUsername + ''';';
    END
    ELSE
    BEGIN
        PRINT '  User does not exist at all!';
        PRINT '  Run TestData.sql to create test users.';
    END
END
PRINT '';

-- ====================================================
-- 6. CHECK CONNECTION FROM C#
-- ====================================================
PRINT '6. CONNECTION STRING VERIFICATION:';
PRINT '-------------------------------------------------';
PRINT 'C# uses this connection string:';
PRINT 'Data Source=localhost;Initial Catalog=TimeFlowDB;Integrated Security=True;TrustServerCertificate=True';
PRINT '';
PRINT 'Current database: ' + DB_NAME();
PRINT 'Current server: ' + @@SERVERNAME;
PRINT '';

-- ====================================================
-- 7. SUMMARY
-- ====================================================
PRINT '=================================================';
PRINT 'SUMMARY';
PRINT '=================================================';

DECLARE @Issues INT = 0;

IF OBJECT_ID('Users', 'U') IS NULL 
BEGIN
    PRINT '? Users table missing';
    SET @Issues = @Issues + 1;
END

IF (SELECT COUNT(*) FROM Users) = 0
BEGIN
    PRINT '? No users in database';
    SET @Issues = @Issues + 1;
END

IF (SELECT COUNT(*) FROM Categories) < 6
BEGIN
    PRINT '? Missing default categories';
    SET @Issues = @Issues + 1;
END

-- Check login test
IF NOT EXISTS (
    SELECT 1 FROM Users 
    WHERE Username = 'admin' 
    AND PasswordHash = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'
    AND IsActive = 1
)
BEGIN
    PRINT '? Admin user login will FAIL';
    SET @Issues = @Issues + 1;
END

IF @Issues = 0
BEGIN
    PRINT '';
    PRINT '??? ALL CHECKS PASSED! ???';
    PRINT '';
    PRINT 'Database is ready for testing!';
    PRINT '';
    PRINT 'Test accounts:';
    PRINT '  Username: admin,     Password: Test@1234';
    PRINT '  Username: testuser,  Password: Test@1234';
    PRINT '  Username: demouser,  Password: Test@1234';
END
ELSE
BEGIN
    PRINT '';
    PRINT '??? FOUND ' + CAST(@Issues AS VARCHAR) + ' ISSUE(S) ???';
    PRINT '';
    PRINT 'Please fix the issues above before testing.';
    PRINT '';
    PRINT 'Quick fix:';
    PRINT '  1. Run TimeFlowDB_Schema.sql (if tables missing)';
    PRINT '  2. Run TestData.sql (if data missing)';
END

PRINT '=================================================';
GO
