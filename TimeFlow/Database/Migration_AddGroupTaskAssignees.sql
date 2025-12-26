-- ====================================================
-- Migration: Add GroupTaskAssignees table for multiple assignees
-- Date: 2024
-- Description: Support multiple assignees per group task
-- ====================================================

-- Create GroupTaskAssignees table (many-to-many relationship)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupTaskAssignees')
BEGIN
    CREATE TABLE GroupTaskAssignees (
        AssignmentId INT PRIMARY KEY IDENTITY(1,1),
        GroupTaskId INT NOT NULL,
        UserId INT NOT NULL,
        AssignedBy INT NULL,
        AssignedAt DATETIME NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT FK_GroupTaskAssignees_GroupTasks FOREIGN KEY (GroupTaskId) 
            REFERENCES GroupTasks(GroupTaskId) ON DELETE CASCADE,
        CONSTRAINT FK_GroupTaskAssignees_Users FOREIGN KEY (UserId) 
            REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_GroupTaskAssignees_AssignedBy FOREIGN KEY (AssignedBy) 
            REFERENCES Users(UserId) ON DELETE NO ACTION,
        CONSTRAINT UQ_GroupTaskAssignees_GroupTaskUser UNIQUE(GroupTaskId, UserId)
    );
    
    CREATE INDEX IX_GroupTaskAssignees_GroupTaskId ON GroupTaskAssignees(GroupTaskId);
    CREATE INDEX IX_GroupTaskAssignees_UserId ON GroupTaskAssignees(UserId);
    
    PRINT 'GroupTaskAssignees table created successfully';
END
ELSE
BEGIN
    PRINT 'GroupTaskAssignees table already exists';
END
GO

