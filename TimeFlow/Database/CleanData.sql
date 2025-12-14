-- ====================================================
-- TimeFlow - Clean All Test Data Script
-- Xoa TAT CA du lieu test de bat dau lai tu dau
-- ====================================================

USE TimeFlowDB;
GO

PRINT '=================================================';
PRINT 'WARNING: CLEANING ALL TEST DATA';
PRINT '=================================================';
PRINT '';
PRINT 'This will DELETE ALL data from:';
PRINT '  - Users';
PRINT '  - Tasks';
PRINT '  - Groups';
PRINT '  - GroupMembers';
PRINT '  - GroupTasks';
PRINT '  - Comments';
PRINT '  - ActivityLog';
PRINT '  - UserTokens';
PRINT '';
PRINT 'Categories will NOT be deleted (system default)';
PRINT '';
PRINT 'Starting cleanup in 3 seconds...';
WAITFOR DELAY '00:00:03';
GO

-- ====================================================
-- XOA DU LIEU THEO THU TU (FOREIGN KEY SAFE)
-- ====================================================

-- Buoc 1: Xoa ActivityLog
PRINT 'Step 1/8: Deleting ActivityLog...';
DELETE FROM ActivityLog;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
PRINT '';

-- Buoc 2: Xoa Comments
PRINT 'Step 2/8: Deleting Comments...';
DELETE FROM Comments;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
PRINT '';

-- Buoc 3: Xoa UserTokens
PRINT 'Step 3/8: Deleting UserTokens...';
DELETE FROM UserTokens;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
PRINT '';

-- Buoc 4: Xoa GroupTasks
PRINT 'Step 4/8: Deleting GroupTasks...';
DELETE FROM GroupTasks;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
PRINT '';

-- Buoc 5: Xoa GroupMembers
PRINT 'Step 5/8: Deleting GroupMembers...';
DELETE FROM GroupMembers;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
PRINT '';

-- Buoc 6: Xoa Groups
PRINT 'Step 6/8: Deleting Groups...';
DELETE FROM Groups;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
PRINT '';

-- Buoc 7: Xoa Tasks
PRINT 'Step 7/8: Deleting Tasks...';
DELETE FROM Tasks;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
PRINT '';

-- Buoc 8: Xoa Users
PRINT 'Step 8/8: Deleting Users...';
DELETE FROM Users;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
PRINT '';

-- ====================================================
-- RESET IDENTITY SEEDS
-- ====================================================
PRINT 'Resetting identity seeds...';

DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Tasks', RESEED, 0);
DBCC CHECKIDENT ('Groups', RESEED, 0);
DBCC CHECKIDENT ('GroupMembers', RESEED, 0);
DBCC CHECKIDENT ('GroupTasks', RESEED, 0);
DBCC CHECKIDENT ('Comments', RESEED, 0);
DBCC CHECKIDENT ('ActivityLog', RESEED, 0);
DBCC CHECKIDENT ('UserTokens', RESEED, 0);

PRINT '  All identity seeds reset to 0';
PRINT '';

-- ====================================================
-- VERIFY CLEANUP
-- ====================================================
PRINT '=================================================';
PRINT 'VERIFICATION';
PRINT '=================================================';
PRINT '';

DECLARE @UserCount INT = (SELECT COUNT(*) FROM Users);
DECLARE @TaskCount INT = (SELECT COUNT(*) FROM Tasks);
DECLARE @GroupCount INT = (SELECT COUNT(*) FROM Groups);
DECLARE @GroupMemberCount INT = (SELECT COUNT(*) FROM GroupMembers);
DECLARE @GroupTaskCount INT = (SELECT COUNT(*) FROM GroupTasks);
DECLARE @CommentCount INT = (SELECT COUNT(*) FROM Comments);
DECLARE @ActivityCount INT = (SELECT COUNT(*) FROM ActivityLog);
DECLARE @TokenCount INT = (SELECT COUNT(*) FROM UserTokens);
DECLARE @CategoryCount INT = (SELECT COUNT(*) FROM Categories);

PRINT 'Current record counts:';
PRINT '  Users: ' + CAST(@UserCount AS VARCHAR);
PRINT '  Tasks: ' + CAST(@TaskCount AS VARCHAR);
PRINT '  Groups: ' + CAST(@GroupCount AS VARCHAR);
PRINT '  GroupMembers: ' + CAST(@GroupMemberCount AS VARCHAR);
PRINT '  GroupTasks: ' + CAST(@GroupTaskCount AS VARCHAR);
PRINT '  Comments: ' + CAST(@CommentCount AS VARCHAR);
PRINT '  ActivityLog: ' + CAST(@ActivityCount AS VARCHAR);
PRINT '  UserTokens: ' + CAST(@TokenCount AS VARCHAR);
PRINT '  Categories: ' + CAST(@CategoryCount AS VARCHAR) + ' (not deleted)';
PRINT '';

IF @UserCount = 0 AND @TaskCount = 0 AND @GroupCount = 0
BEGIN
    PRINT '??? CLEANUP SUCCESSFUL! ???';
    PRINT '';
    PRINT 'All test data has been removed.';
    PRINT 'Database is now clean and ready for fresh data.';
    PRINT '';
    PRINT 'Next steps:';
    PRINT '  1. Run TestData.sql to insert new test data';
    PRINT '  2. Or start using the application with real data';
END
ELSE
BEGIN
    PRINT '??? CLEANUP INCOMPLETE ???';
    PRINT '';
    PRINT 'Some records still remain. Please check for errors.';
END

PRINT '=================================================';
GO
