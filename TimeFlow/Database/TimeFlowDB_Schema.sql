-- ====================================================
-- TimeFlow Database Schema
-- Version: 1.0
-- Description: Complete database schema for TimeFlow Task Management System
-- ====================================================

USE master;
GO

-- Drop database if exists (for development only)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TimeFlowDB')
BEGIN
    ALTER DATABASE TimeFlowDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TimeFlowDB;
END
GO

-- Create database
CREATE DATABASE TimeFlowDB;
GO

-- CREATE LOGIN myuser WITH PASSWORD = 'YourStrong@Passw0rd';
-- GO

USE TimeFlowDB;
CREATE USER myuser FOR LOGIN myuser;
GO

ALTER ROLE db_owner ADD MEMBER myuser;
GO

-- ====================================================
-- 1. USERS TABLE
-- ====================================================
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NULL,
    AvatarUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME NULL,
    
    CONSTRAINT CK_Username CHECK (LEN(Username) >= 5 AND LEN(Username) <= 50),
    CONSTRAINT CK_Email CHECK (Email LIKE '%_@__%.__%')
);
GO

-- Index for faster login queries
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
GO

-- ====================================================
-- 2. CATEGORIES TABLE
-- ====================================================
CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(50) NOT NULL UNIQUE,
    Color NVARCHAR(7) NOT NULL, -- Hex color format: #RRGGBB
    IconName NVARCHAR(50) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsDefault BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT CK_Color CHECK (Color LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
);
GO

-- Insert default categories
INSERT INTO Categories (CategoryName, Color, IconName, IsDefault) VALUES
(N'Work', '#3B82F6', 'briefcase', 1),
(N'Personal', '#10B981', 'user', 1),
(N'Study', '#F59E0B', 'book', 1),
(N'Health', '#EF4444', 'heart', 1),
(N'Shopping', '#8B5CF6', 'shopping-cart', 1),
(N'Other', '#6B7280', 'folder', 1);
GO

-- ====================================================
-- 3. TASKS TABLE
-- ====================================================
CREATE TABLE Tasks (
    TaskId INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    DueDate DATETIME NULL,
    Priority INT NOT NULL DEFAULT 2, -- 1=Low, 2=Medium, 3=High
    Status INT NOT NULL DEFAULT 1, -- 1=Pending, 2=InProgress, 3=Completed, 4=Cancelled
    CategoryId INT NULL,
    CreatedBy INT NOT NULL,
    IsGroupTask BIT NOT NULL DEFAULT 0,
    CompletedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    
    CONSTRAINT FK_Tasks_Categories FOREIGN KEY (CategoryId) 
        REFERENCES Categories(CategoryId) ON DELETE SET NULL,
    CONSTRAINT FK_Tasks_CreatedBy FOREIGN KEY (CreatedBy) 
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT CK_Priority CHECK (Priority IN (1, 2, 3)),
    CONSTRAINT CK_Status CHECK (Status IN (1, 2, 3, 4)),
    CONSTRAINT CK_Title CHECK (LEN(TRIM(Title)) > 0)
);
GO

-- Indexes for faster queries
CREATE INDEX IX_Tasks_CreatedBy ON Tasks(CreatedBy);
CREATE INDEX IX_Tasks_DueDate ON Tasks(DueDate);
CREATE INDEX IX_Tasks_Status ON Tasks(Status);
CREATE INDEX IX_Tasks_IsGroupTask ON Tasks(IsGroupTask);
GO

-- ====================================================
-- 4. GROUPS TABLE
-- ====================================================
CREATE TABLE Groups (
    GroupId INT PRIMARY KEY IDENTITY(1,1),
    GroupName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_Groups_CreatedBy FOREIGN KEY (CreatedBy) 
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_GroupName CHECK (LEN(TRIM(GroupName)) > 0)
);
GO

CREATE INDEX IX_Groups_CreatedBy ON Groups(CreatedBy);
GO

-- ====================================================
-- 5. GROUP_MEMBERS TABLE
-- ====================================================
CREATE TABLE GroupMembers (
    GroupMemberId INT PRIMARY KEY IDENTITY(1,1),
    GroupId INT NOT NULL,
    UserId INT NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Member', -- 'Admin' or 'Member'
    JoinedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_GroupMembers_Groups FOREIGN KEY (GroupId) 
        REFERENCES Groups(GroupId) ON DELETE CASCADE,
    CONSTRAINT FK_GroupMembers_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT CK_Role CHECK (Role IN ('Admin', 'Member')),
    CONSTRAINT UQ_GroupMembers_GroupUser UNIQUE(GroupId, UserId)
);
GO

CREATE INDEX IX_GroupMembers_GroupId ON GroupMembers(GroupId);
CREATE INDEX IX_GroupMembers_UserId ON GroupMembers(UserId);
GO

-- ====================================================
-- 6. GROUP_TASKS TABLE
-- ====================================================
CREATE TABLE GroupTasks (
    GroupTaskId INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    GroupId INT NOT NULL,
    AssignedTo INT NULL, -- NULL means not assigned yet
    AssignedAt DATETIME NULL,
    AssignedBy INT NULL,
    
    CONSTRAINT FK_GroupTasks_Tasks FOREIGN KEY (TaskId) 
        REFERENCES Tasks(TaskId) ON DELETE CASCADE,
    CONSTRAINT FK_GroupTasks_Groups FOREIGN KEY (GroupId) 
        REFERENCES Groups(GroupId) ON DELETE NO ACTION,
    CONSTRAINT FK_GroupTasks_AssignedTo FOREIGN KEY (AssignedTo) 
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_GroupTasks_AssignedBy FOREIGN KEY (AssignedBy) 
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT UQ_GroupTasks_TaskGroup UNIQUE(TaskId, GroupId)
);
GO

CREATE INDEX IX_GroupTasks_TaskId ON GroupTasks(TaskId);
CREATE INDEX IX_GroupTasks_GroupId ON GroupTasks(GroupId);
CREATE INDEX IX_GroupTasks_AssignedTo ON GroupTasks(AssignedTo);
GO

-- ====================================================
-- 7. COMMENTS TABLE (Optional - for task discussion)
-- ====================================================
CREATE TABLE Comments (
    CommentId INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsEdited BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_Comments_Tasks FOREIGN KEY (TaskId) 
        REFERENCES Tasks(TaskId) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT CK_CommentText CHECK (LEN(TRIM(CommentText)) > 0)
);
GO

CREATE INDEX IX_Comments_TaskId ON Comments(TaskId);
CREATE INDEX IX_Comments_UserId ON Comments(UserId);
GO

-- ====================================================
-- 8. ACTIVITY_LOG TABLE (Optional - for tracking changes)
-- ====================================================
CREATE TABLE ActivityLog (
    LogId INT PRIMARY KEY IDENTITY(1,1),
    TaskId INT NULL,
    UserId INT NOT NULL,
    ActivityType NVARCHAR(50) NOT NULL, -- 'Created', 'Updated', 'Completed', 'Assigned', etc.
    ActivityDescription NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_ActivityLog_Tasks FOREIGN KEY (TaskId) 
        REFERENCES Tasks(TaskId) ON DELETE CASCADE,
    CONSTRAINT FK_ActivityLog_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO

CREATE INDEX IX_ActivityLog_TaskId ON ActivityLog(TaskId);
CREATE INDEX IX_ActivityLog_UserId ON ActivityLog(UserId);
CREATE INDEX IX_ActivityLog_CreatedAt ON ActivityLog(CreatedAt);
GO

-- ====================================================
-- 9. USER_TOKENS TABLE (For JWT/Session management)
-- ====================================================
CREATE TABLE UserTokens (
    TokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME NULL,
    
    CONSTRAINT FK_UserTokens_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

CREATE INDEX IX_UserTokens_UserId ON UserTokens(UserId);
CREATE INDEX IX_UserTokens_Token ON UserTokens(Token);
CREATE INDEX IX_UserTokens_ExpiresAt ON UserTokens(ExpiresAt);
GO

-- ====================================================
-- 10. MESSAGES TABLE (For Chat functionality)
-- ====================================================
CREATE TABLE Messages (
    MessageId INT PRIMARY KEY IDENTITY(1,1),
    SenderUsername NVARCHAR(50) NOT NULL,
    ReceiverUsername NVARCHAR(50) NULL, -- NULL for group messages
    MessageContent NVARCHAR(MAX) NOT NULL,
    IsGroupMessage BIT NOT NULL DEFAULT 0,
    GroupId INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsRead BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_Messages_Groups FOREIGN KEY (GroupId) 
        REFERENCES Groups(GroupId) ON DELETE SET NULL,
    CONSTRAINT CK_Messages_Receiver CHECK (
        (IsGroupMessage = 0 AND ReceiverUsername IS NOT NULL) OR 
        (IsGroupMessage = 1 AND GroupId IS NOT NULL)
    )
);
GO

CREATE INDEX IX_Messages_SenderUsername ON Messages(SenderUsername);
CREATE INDEX IX_Messages_ReceiverUsername ON Messages(ReceiverUsername);
CREATE INDEX IX_Messages_GroupId ON Messages(GroupId);
CREATE INDEX IX_Messages_CreatedAt ON Messages(CreatedAt);
GO

-- ====================================================
-- VIEWS
-- ====================================================

-- View: User's Tasks Summary
CREATE VIEW vw_UserTasksSummary AS
SELECT 
    u.UserId,
    u.Username,
    COUNT(CASE WHEN t.Status = 1 THEN 1 END) AS PendingTasks,
    COUNT(CASE WHEN t.Status = 2 THEN 1 END) AS InProgressTasks,
    COUNT(CASE WHEN t.Status = 3 THEN 1 END) AS CompletedTasks,
    COUNT(*) AS TotalTasks
FROM Users u
LEFT JOIN Tasks t ON u.UserId = t.CreatedBy AND t.IsGroupTask = 0
GROUP BY u.UserId, u.Username;
GO

-- View: Group Tasks with Members
CREATE VIEW vw_GroupTasksWithMembers AS
SELECT 
    gt.GroupTaskId,
    t.TaskId,
    t.Title,
    t.Description,
    t.DueDate,
    t.Priority,
    t.Status,
    g.GroupId,
    g.GroupName,
    u_assigned.UserId AS AssignedUserId,
    u_assigned.Username AS AssignedUsername,
    u_created.Username AS CreatedByUsername,
    gt.AssignedAt
FROM GroupTasks gt
INNER JOIN Tasks t ON gt.TaskId = t.TaskId
INNER JOIN Groups g ON gt.GroupId = g.GroupId
LEFT JOIN Users u_assigned ON gt.AssignedTo = u_assigned.UserId
INNER JOIN Users u_created ON t.CreatedBy = u_created.UserId;
GO

-- View: User's Group Memberships
CREATE VIEW vw_UserGroups AS
SELECT 
    gm.UserId,
    u.Username,
    g.GroupId,
    g.GroupName,
    g.Description,
    gm.Role,
    gm.JoinedAt,
    (SELECT COUNT(*) FROM GroupMembers WHERE GroupId = g.GroupId AND IsActive = 1) AS MemberCount
FROM GroupMembers gm
INNER JOIN Users u ON gm.UserId = u.UserId
INNER JOIN Groups g ON gm.GroupId = g.GroupId
WHERE gm.IsActive = 1 AND g.IsActive = 1;
GO

-- ====================================================
-- STORED PROCEDURES
-- ====================================================

-- SP: Get User Tasks (Personal + Assigned Group Tasks)
CREATE PROCEDURE sp_GetUserAllTasks
    @UserId INT,
    @Status INT = NULL,
    @Priority INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.TaskId,
        t.Title,
        t.Description,
        t.DueDate,
        t.Priority,
        t.Status,
        t.IsGroupTask,
        c.CategoryName,
        c.Color AS CategoryColor,
        CASE 
            WHEN t.IsGroupTask = 1 THEN g.GroupName
            ELSE NULL
        END AS GroupName,
        t.CreatedAt,
        t.UpdatedAt
    FROM Tasks t
    LEFT JOIN Categories c ON t.CategoryId = c.CategoryId
    LEFT JOIN GroupTasks gt ON t.TaskId = gt.TaskId
    LEFT JOIN Groups g ON gt.GroupId = g.GroupId
    WHERE (t.CreatedBy = @UserId OR gt.AssignedTo = @UserId)
        AND (@Status IS NULL OR t.Status = @Status)
        AND (@Priority IS NULL OR t.Priority = @Priority)
    ORDER BY 
        CASE WHEN t.DueDate IS NULL THEN 1 ELSE 0 END,
        t.DueDate ASC,
        t.Priority DESC;
END
GO

-- SP: Get Group Members
CREATE PROCEDURE sp_GetGroupMembers
    @GroupId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        gm.GroupMemberId,
        u.UserId,
        u.Username,
        u.Email,
        u.FullName,
        gm.Role,
        gm.JoinedAt
    FROM GroupMembers gm
    INNER JOIN Users u ON gm.UserId = u.UserId
    WHERE gm.GroupId = @GroupId AND gm.IsActive = 1
    ORDER BY 
        CASE WHEN gm.Role = 'Admin' THEN 0 ELSE 1 END,
        gm.JoinedAt ASC;
END
GO

-- SP: Create Task with Activity Log
CREATE PROCEDURE sp_CreateTask
    @Title NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @DueDate DATETIME,
    @Priority INT,
    @Status INT,
    @CategoryId INT,
    @CreatedBy INT,
    @IsGroupTask BIT,
    @GroupId INT = NULL,
    @AssignedTo INT = NULL,
    @TaskId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Insert task
        INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask)
        VALUES (@Title, @Description, @DueDate, @Priority, @Status, @CategoryId, @CreatedBy, @IsGroupTask);
        
        SET @TaskId = SCOPE_IDENTITY();
        
        -- If group task, create group task entry
        IF @IsGroupTask = 1 AND @GroupId IS NOT NULL
        BEGIN
            INSERT INTO GroupTasks (TaskId, GroupId, AssignedTo, AssignedAt, AssignedBy)
            VALUES (@TaskId, @GroupId, @AssignedTo, GETDATE(), @CreatedBy);
        END
        
        -- Log activity
        INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription)
        VALUES (@TaskId, @CreatedBy, 'Created', N'Task created: ' + @Title);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Update Task Status with Activity Log
CREATE PROCEDURE sp_UpdateTaskStatus
    @TaskId INT,
    @Status INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @OldStatus INT;
        DECLARE @Title NVARCHAR(200);
        
        SELECT @OldStatus = Status, @Title = Title
        FROM Tasks
        WHERE TaskId = @TaskId;
        
        IF @OldStatus IS NULL
        BEGIN
            RAISERROR('Task not found', 16, 1);
            RETURN;
        END
        
        -- Update task
        UPDATE Tasks
        SET Status = @Status,
            UpdatedAt = GETDATE(),
            CompletedAt = CASE WHEN @Status = 3 THEN GETDATE() ELSE NULL END
        WHERE TaskId = @TaskId;
        
        -- Log activity
        DECLARE @StatusText NVARCHAR(50);
        SET @StatusText = CASE @Status
            WHEN 1 THEN 'Pending'
            WHEN 2 THEN 'In Progress'
            WHEN 3 THEN 'Completed'
            WHEN 4 THEN 'Cancelled'
        END;
        
        INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription)
        VALUES (@TaskId, @UserId, 'StatusChanged', N'Status changed to: ' + @StatusText);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ====================================================
-- FUNCTIONS
-- ====================================================

-- Function: Get overdue tasks count for a user
CREATE FUNCTION fn_GetOverdueTasksCount(@UserId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    
    SELECT @Count = COUNT(*)
    FROM Tasks t
    LEFT JOIN GroupTasks gt ON t.TaskId = gt.TaskId
    WHERE (t.CreatedBy = @UserId OR gt.AssignedTo = @UserId)
        AND t.Status IN (1, 2) -- Pending or InProgress
        AND t.DueDate < GETDATE();
    
    RETURN ISNULL(@Count, 0);
END
GO

-- ====================================================
-- TRIGGERS
-- ====================================================

-- Trigger: Update UpdatedAt on Tasks table
CREATE TRIGGER trg_Tasks_UpdatedAt
ON Tasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Tasks
    SET UpdatedAt = GETDATE()
    FROM Tasks t
    INNER JOIN inserted i ON t.TaskId = i.TaskId;
END
GO

-- Trigger: Auto-add creator as Admin when creating a group
CREATE TRIGGER trg_Groups_AutoAddCreator
ON Groups
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO GroupMembers (GroupId, UserId, Role)
    SELECT GroupId, CreatedBy, 'Admin'
    FROM inserted;
END
GO

-- ====================================================
-- SAMPLE DATA (For testing - comment out in production)
-- ====================================================

-- Insert sample users (Note: These are hashed passwords for testing only)
-- Password for all: Test@1234
INSERT INTO Users (Username, Email, PasswordHash, FullName) VALUES
(N'john_doe', N'john@example.com', N'$2a$11$XYZ...', N'John Doe'),
(N'jane_smith', N'jane@example.com', N'$2a$11$ABC...', N'Jane Smith'),
(N'bob_wilson', N'bob@example.com', N'$2a$11$DEF...', N'Bob Wilson');
GO

-- ====================================================
-- COMPLETION MESSAGE
-- ====================================================

PRINT '=================================================';
PRINT 'TimeFlow Database Schema Created Successfully!';
PRINT '=================================================';
PRINT 'Database Name: TimeFlowDB';
PRINT 'Tables Created: 10';
PRINT 'Views Created: 3';
PRINT 'Stored Procedures Created: 4';
PRINT 'Functions Created: 1';
PRINT 'Triggers Created: 2';
PRINT '=================================================';
GO
