-- ====================================================
-- TimeFlow Test Data Script
-- Tao du lieu mau de test he thong
-- ====================================================

USE TimeFlowDB;
GO

PRINT '=================================================';
PRINT 'CLEANING UP OLD TEST DATA';
PRINT '=================================================';
PRINT '';

-- ====================================================
-- XOA DU LIEU CU (neu co)
-- Phai xoa theo thu tu de tranh loi foreign key constraint
-- ====================================================

-- Buoc 1: Xoa ActivityLog (khong co foreign key reference den no)
IF EXISTS (SELECT 1 FROM ActivityLog)
BEGIN
    PRINT 'Deleting ActivityLog records...';
    DELETE FROM ActivityLog;
    PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END

-- Buoc 2: Xoa Comments (khong co foreign key reference den no)
IF EXISTS (SELECT 1 FROM Comments)
BEGIN
    PRINT 'Deleting Comments records...';
    DELETE FROM Comments;
    PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END

-- Buoc 3: Xoa UserTokens (khong co foreign key reference den no)
IF EXISTS (SELECT 1 FROM UserTokens)
BEGIN
    PRINT 'Deleting UserTokens records...';
    DELETE FROM UserTokens;
    PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END

-- Buoc 4: Xoa GroupTasks (phu thuoc vao Tasks va Groups)
IF EXISTS (SELECT 1 FROM GroupTasks)
BEGIN
    PRINT 'Deleting GroupTasks records...';
    DELETE FROM GroupTasks;
    PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END

-- Buoc 5: Xoa GroupMembers (phu thuoc vao Groups)
IF EXISTS (SELECT 1 FROM GroupMembers)
BEGIN
    PRINT 'Deleting GroupMembers records...';
    DELETE FROM GroupMembers;
    PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END

-- Buoc 6: Xoa Groups (phu thuoc vao Users qua CreatedBy)
IF EXISTS (SELECT 1 FROM Groups)
BEGIN
    PRINT 'Deleting Groups records...';
    DELETE FROM Groups;
    PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END

-- Buoc 7: Xoa Tasks (phu thuoc vao Users va Categories)
IF EXISTS (SELECT 1 FROM Tasks)
BEGIN
    PRINT 'Deleting Tasks records...';
    DELETE FROM Tasks;
    PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END

-- Buoc 8: Xoa Users (khong xoa neu co du lieu phu thuoc)
IF EXISTS (SELECT 1 FROM Users)
BEGIN
    PRINT 'Deleting Users records...';
    DELETE FROM Users;
    PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' records';
END

-- Buoc 9: KHONG xoa Categories vi la du lieu mac dinh cua he thong
PRINT '';
PRINT 'Note: Categories NOT deleted (system default data)';
PRINT '';

-- Reset identity seeds de ID bat dau tu 1
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Tasks', RESEED, 0);
DBCC CHECKIDENT ('Groups', RESEED, 0);
DBCC CHECKIDENT ('GroupMembers', RESEED, 0);
DBCC CHECKIDENT ('GroupTasks', RESEED, 0);
DBCC CHECKIDENT ('Comments', RESEED, 0);
DBCC CHECKIDENT ('ActivityLog', RESEED, 0);
DBCC CHECKIDENT ('UserTokens', RESEED, 0);

PRINT 'Identity seeds reset to 0';
PRINT '';
PRINT '✓ Cleanup completed!';
PRINT '';
PRINT '=================================================';
PRINT 'INSERTING NEW TEST DATA';
PRINT '=================================================';
PRINT '';

-- ====================================================
-- 1. TAO USERS MAU
-- ====================================================
-- Password cho tat ca users: Test@1234
-- SHA256 hash cua "Test@1234": 849f1575ccfbf3a4d6cf00e6c5641b7fd4da2ed3e212c2d79ba9161a5a432ff0

DECLARE @PasswordHash NVARCHAR(255) = '849f1575ccfbf3a4d6cf00e6c5641b7fd4da2ed3e212c2d79ba9161a5a432ff0';

-- User 1: Admin user - co nhieu tasks va groups
INSERT INTO Users (Username, Email, PasswordHash, FullName, IsActive, CreatedAt)
VALUES 
(N'admin', N'admin@timeflow.com', @PasswordHash, N'Administrator', 1, GETDATE());

DECLARE @AdminUserId INT = SCOPE_IDENTITY();

-- User 2: Test user - co tasks ca nhan
INSERT INTO Users (Username, Email, PasswordHash, FullName, IsActive, CreatedAt)
VALUES 
(N'testuser', N'test@timeflow.com', @PasswordHash, N'Test User', 1, GETDATE());

DECLARE @TestUserId INT = SCOPE_IDENTITY();

-- User 3: Demo user - tham gia nhom
INSERT INTO Users (Username, Email, PasswordHash, FullName, IsActive, CreatedAt)
VALUES 
(N'demouser', N'demo@timeflow.com', @PasswordHash, N'Demo User', 1, GETDATE());

DECLARE @DemoUserId INT = SCOPE_IDENTITY();

PRINT 'Created 3 users:';
PRINT '  - Username: admin, Password: Test@1234';
PRINT '  - Username: testuser, Password: Test@1234';
PRINT '  - Username: demouser, Password: Test@1234';
PRINT '';

-- ====================================================
-- 2. TAO TASKS CA NHAN CHO ADMIN USER
-- ====================================================

-- Task 1: Pending - High Priority - Overdue
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Complete Project Report', 
 N'Write comprehensive report for Q4 project deliverables',
 DATEADD(day, -2, GETDATE()), -- Qua han 2 ngay
 3, -- High
 1, -- Pending
 1, -- Work category
 @AdminUserId,
 0,
 DATEADD(day, -10, GETDATE()));

-- Task 2: In Progress - Medium Priority - Due today
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Review Code Pull Requests',
 N'Review and merge pending PRs from team members',
 CAST(GETDATE() AS DATE), -- Hom nay
 2, -- Medium
 2, -- In Progress
 1, -- Work
 @AdminUserId,
 0,
 DATEADD(day, -5, GETDATE()));

-- Task 3: Pending - Low Priority - Next week
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Plan Team Building Activity',
 N'Organize team building event for next month',
 DATEADD(day, 7, GETDATE()),
 1, -- Low
 1, -- Pending
 2, -- Personal
 @AdminUserId,
 0,
 DATEADD(day, -3, GETDATE()));

-- Task 4: Completed - High Priority
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt, CompletedAt)
VALUES 
(N'Database Schema Design',
 N'Design and implement database schema for TimeFlow',
 DATEADD(day, -1, GETDATE()),
 3, -- High
 3, -- Completed
 1, -- Work
 @AdminUserId,
 0,
 DATEADD(day, -15, GETDATE()),
 DATEADD(day, -2, GETDATE()));

-- Task 5: Study task
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Learn ASP.NET Core',
 N'Complete ASP.NET Core tutorial on Microsoft Learn',
 DATEADD(day, 14, GETDATE()),
 2, -- Medium
 1, -- Pending
 3, -- Study
 @AdminUserId,
 0,
 DATEADD(day, -1, GETDATE()));

-- Task 6: Health task
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Gym Workout',
 N'Leg day workout session at gym',
 CAST(GETDATE() AS DATE),
 2, -- Medium
 2, -- In Progress
 4, -- Health
 @AdminUserId,
 0,
 GETDATE());

PRINT 'Created 6 personal tasks for admin user';
PRINT '';

-- ====================================================
-- 3. TAO TASKS CA NHAN CHO TEST USER
-- ====================================================

INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Buy Groceries',
 N'Buy milk, eggs, bread, vegetables',
 DATEADD(day, 1, GETDATE()),
 2, -- Medium
 1, -- Pending
 5, -- Shopping
 @TestUserId,
 0,
 GETDATE());

INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Finish Homework',
 N'Complete math and physics assignments',
 DATEADD(day, 2, GETDATE()),
 3, -- High
 1, -- Pending
 3, -- Study
 @TestUserId,
 0,
 GETDATE());

PRINT 'Created 2 personal tasks for testuser';
PRINT '';

-- ====================================================
-- 4. TAO GROUPS
-- ====================================================

-- Group 1: Development Team (admin la creator)
INSERT INTO Groups (GroupName, Description, CreatedBy, IsActive, CreatedAt)
VALUES 
(N'Development Team',
 N'Main development team for TimeFlow project',
 @AdminUserId,
 1,
 DATEADD(day, -20, GETDATE()));

DECLARE @DevGroupId INT = SCOPE_IDENTITY();

-- Group 2: QA Team (testuser la creator)
INSERT INTO Groups (GroupName, Description, CreatedBy, IsActive, CreatedAt)
VALUES 
(N'QA Team',
 N'Quality Assurance and Testing team',
 @TestUserId,
 1,
 DATEADD(day, -15, GETDATE()));

DECLARE @QAGroupId INT = SCOPE_IDENTITY();

PRINT 'Created 2 groups: Development Team, QA Team';
PRINT '';

-- ====================================================
-- 5. THEM MEMBERS VAO GROUPS
-- ====================================================
-- Luu y: Trigger trg_Groups_AutoAddCreator da tu dong them creator vao GroupMembers

-- Them demouser vao Development Team
INSERT INTO GroupMembers (GroupId, UserId, Role, JoinedAt, IsActive)
VALUES (@DevGroupId, @DemoUserId, 'Member', DATEADD(day, -18, GETDATE()), 1);

-- Them testuser vao Development Team
INSERT INTO GroupMembers (GroupId, UserId, Role, JoinedAt, IsActive)
VALUES (@DevGroupId, @TestUserId, 'Member', DATEADD(day, -15, GETDATE()), 1);

-- Them admin vao QA Team
INSERT INTO GroupMembers (GroupId, UserId, Role, JoinedAt, IsActive)
VALUES (@QAGroupId, @AdminUserId, 'Admin', DATEADD(day, -14, GETDATE()), 1);

-- Them demouser vao QA Team
INSERT INTO GroupMembers (GroupId, UserId, Role, JoinedAt, IsActive)
VALUES (@QAGroupId, @DemoUserId, 'Member', DATEADD(day, -10, GETDATE()), 1);

PRINT 'Added members to groups';
PRINT '';

-- ====================================================
-- 6. TAO GROUP TASKS
-- ====================================================

-- Group Task 1: Development Team - Assigned to testuser
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Implement User Authentication',
 N'Implement JWT authentication and authorization',
 DATEADD(day, 5, GETDATE()),
 3, -- High
 2, -- In Progress
 1, -- Work
 @AdminUserId,
 1, -- Group task
 DATEADD(day, -5, GETDATE()));

DECLARE @GroupTask1Id INT = SCOPE_IDENTITY();

INSERT INTO GroupTasks (TaskId, GroupId, AssignedTo, AssignedBy, AssignedAt)
VALUES (@GroupTask1Id, @DevGroupId, @TestUserId, @AdminUserId, DATEADD(day, -5, GETDATE()));

-- Group Task 2: Development Team - Assigned to demouser
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Design UI Components',
 N'Design reusable UI components for the application',
 DATEADD(day, 10, GETDATE()),
 2, -- Medium
 1, -- Pending
 1, -- Work
 @AdminUserId,
 1,
 DATEADD(day, -3, GETDATE()));

DECLARE @GroupTask2Id INT = SCOPE_IDENTITY();

INSERT INTO GroupTasks (TaskId, GroupId, AssignedTo, AssignedBy, AssignedAt)
VALUES (@GroupTask2Id, @DevGroupId, @DemoUserId, @AdminUserId, DATEADD(day, -3, GETDATE()));

-- Group Task 3: Development Team - Unassigned
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Write API Documentation',
 N'Document all REST API endpoints',
 DATEADD(day, 15, GETDATE()),
 1, -- Low
 1, -- Pending
 1, -- Work
 @AdminUserId,
 1,
 DATEADD(day, -1, GETDATE()));

DECLARE @GroupTask3Id INT = SCOPE_IDENTITY();

INSERT INTO GroupTasks (TaskId, GroupId, AssignedTo, AssignedBy, AssignedAt)
VALUES (@GroupTask3Id, @DevGroupId, NULL, NULL, NULL); -- Unassigned

-- Group Task 4: QA Team - Assigned to admin
INSERT INTO Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy, IsGroupTask, CreatedAt)
VALUES 
(N'Create Test Cases',
 N'Create comprehensive test cases for all features',
 DATEADD(day, 7, GETDATE()),
 3, -- High
 1, -- Pending
 1, -- Work
 @TestUserId,
 1,
 DATEADD(day, -2, GETDATE()));

DECLARE @GroupTask4Id INT = SCOPE_IDENTITY();

INSERT INTO GroupTasks (TaskId, GroupId, AssignedTo, AssignedBy, AssignedAt)
VALUES (@GroupTask4Id, @QAGroupId, @AdminUserId, @TestUserId, DATEADD(day, -2, GETDATE()));

PRINT 'Created 4 group tasks (3 for Dev Team, 1 for QA Team)';
PRINT '';

-- ====================================================
-- 7. TAO COMMENTS
-- ====================================================

-- Comments on Group Task 1
INSERT INTO Comments (TaskId, UserId, CommentText, CreatedAt, IsEdited)
VALUES 
(@GroupTask1Id, @AdminUserId, N'Please make sure to follow our security guidelines.', DATEADD(day, -4, GETDATE()), 0);

INSERT INTO Comments (TaskId, UserId, CommentText, CreatedAt, IsEdited)
VALUES 
(@GroupTask1Id, @TestUserId, N'Sure, I will implement OAuth 2.0 as well.', DATEADD(day, -3, GETDATE()), 0);

INSERT INTO Comments (TaskId, UserId, CommentText, CreatedAt, IsEdited)
VALUES 
(@GroupTask1Id, @DemoUserId, N'Let me know if you need help with testing.', DATEADD(day, -2, GETDATE()), 0);

-- Comment on personal task
INSERT INTO Comments (TaskId, UserId, CommentText, CreatedAt, IsEdited)
VALUES 
(1, @AdminUserId, N'Remember to include financial data and charts.', DATEADD(day, -1, GETDATE()), 0);

PRINT 'Created 4 comments on tasks';
PRINT '';

-- ====================================================
-- 8. TAO ACTIVITY LOGS
-- ====================================================

-- Logs for task creation
INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
VALUES 
(1, @AdminUserId, 'Created', N'Task created: Complete Project Report', DATEADD(day, -10, GETDATE()));

INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
VALUES 
(2, @AdminUserId, 'Created', N'Task created: Review Code Pull Requests', DATEADD(day, -5, GETDATE()));

-- Logs for status changes
INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
VALUES 
(2, @AdminUserId, 'StatusChanged', N'Status changed to: In Progress', DATEADD(day, -4, GETDATE()));

INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
VALUES 
(4, @AdminUserId, 'StatusChanged', N'Status changed to: Completed', DATEADD(day, -2, GETDATE()));

-- Logs for group task assignments
INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
VALUES 
(@GroupTask1Id, @AdminUserId, 'Assigned', N'Task assigned to testuser', DATEADD(day, -5, GETDATE()));

INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
VALUES 
(@GroupTask2Id, @AdminUserId, 'Assigned', N'Task assigned to demouser', DATEADD(day, -3, GETDATE()));

-- Logs for comments
INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
VALUES 
(@GroupTask1Id, @AdminUserId, 'Commented', N'Added a comment on task', DATEADD(day, -4, GETDATE()));

PRINT 'Created 7 activity log entries';
PRINT '';

-- ====================================================
-- 9. TẠO MESSAGES CHO GROUP CHAT
-- ====================================================

-- Messages trong Development Team
INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'admin', NULL, N'Welcome to the Development Team! Let''s build something amazing.', 1, @DevGroupId, DATEADD(day, -18, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'testuser', NULL, N'Hi everyone! Excited to join the team.', 1, @DevGroupId, DATEADD(day, -17, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'demouser', NULL, N'Hello! Looking forward to working with you all.', 1, @DevGroupId, DATEADD(day, -16, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'admin', NULL, N'I''ve assigned the authentication task to testuser. Please check the requirements.', 1, @DevGroupId, DATEADD(day, -5, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'testuser', NULL, N'Got it! I''ll start working on JWT implementation today.', 1, @DevGroupId, DATEADD(day, -5, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'admin', NULL, N'demouser, can you help with UI components after finishing your current task?', 1, @DevGroupId, DATEADD(day, -3, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'demouser', NULL, N'Sure! I''ll start on that tomorrow.', 1, @DevGroupId, DATEADD(day, -3, GETDATE()), 1);

-- Messages trong QA Team
INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'testuser', NULL, N'Welcome to QA Team! Our goal is to ensure quality in every release.', 1, @QAGroupId, DATEADD(day, -14, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'admin', NULL, N'Thanks for adding me! I''ll help with test case creation.', 1, @QAGroupId, DATEADD(day, -13, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'demouser', NULL, N'Hi! Ready to help with testing.', 1, @QAGroupId, DATEADD(day, -10, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'testuser', NULL, N'admin, I''ve assigned you the test case creation task. Priority is high!', 1, @QAGroupId, DATEADD(day, -2, GETDATE()), 1);

-- Private messages (chat 1-1)
INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'admin', N'testuser', N'Hey, how is the authentication module going?', 0, NULL, DATEADD(day, -4, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'testuser', N'admin', N'Going well! Should be done by tomorrow.', 0, NULL, DATEADD(day, -4, GETDATE()), 1);

INSERT INTO Messages (SenderUsername, ReceiverUsername, MessageContent, IsGroupMessage, GroupId, CreatedAt, IsRead)
VALUES 
(N'admin', N'testuser', N'Great! Let me know if you need any help.', 0, NULL, DATEADD(day, -4, GETDATE()), 1);

PRINT 'Created 15 messages (12 group messages, 3 private messages)';
PRINT '';

-- ====================================================
-- SUMMARY
-- ====================================================

PRINT '=================================================';
PRINT 'Test Data Created Successfully!';
PRINT '=================================================';
PRINT '';
PRINT 'USERS (Password: Test@1234 for all):';
PRINT '  1. Username: admin';
PRINT '     - 6 personal tasks (1 overdue, 2 today, 1 completed)';
PRINT '     - Admin of Development Team';
PRINT '     - Admin of QA Team';
PRINT '';
PRINT '  2. Username: testuser';
PRINT '     - 2 personal tasks';
PRINT '     - Member of Development Team';
PRINT '     - Creator of QA Team';
PRINT '     - 1 group task assigned';
PRINT '';
PRINT '  3. Username: demouser';
PRINT '     - 0 personal tasks';
PRINT '     - Member of Development Team';
PRINT '     - Member of QA Team';
PRINT '     - 1 group task assigned';
PRINT '';
PRINT 'GROUPS:';
PRINT '  1. Development Team (3 members, 3 group tasks)';
PRINT '  2. QA Team (3 members, 1 group task)';
PRINT '';
PRINT 'CATEGORIES (6 default categories already exist):';
PRINT '  Work, Personal, Study, Health, Shopping, Other';
PRINT '';
PRINT '=================================================';
PRINT 'You can now test the application with these accounts';
PRINT '=================================================';
GO
