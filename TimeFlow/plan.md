📋 KẾ HOẠCH PHÁT TRIỂN - TASK MANAGEMENT SYSTEM
🎯 TỔNG QUAN
Hiện trạng dự án:
•	✅ Authentication System hoàn chỉnh (Login/Register/Auto-login với JWT)
•	✅ Database: UserDB với bảng Users
•	✅ Single-Window Navigation đã implement
•	✅ UI Forms: FormGiaoDien, FormThemTask, FormTaskDetail (chỉ có giao diện)
•	❌ Task Management Logic: Chưa có
•	❌ Group Management: Chưa có
•	❌ Database Schema cho Tasks/Groups: Chưa có
---
🎯 YÊU CẦU 1: TẠO CẤU TRÚC TASK (CÁ NHÂN & NHÓM)
📂 Files cần tạo mới:
1.1. Database Schema
File: database_tasks_schema.sql (tạo mới trong root)
•	Tạo bảng Tasks (TaskId, Title, Description, DueDate, Priority, Status, CreatedBy, CreatedAt, UpdatedAt)
•	Tạo bảng Groups (GroupId, GroupName, CreatedBy, CreatedAt)
•	Tạo bảng GroupMembers (GroupId, UserId, Role, JoinedAt)
•	Tạo bảng GroupTasks (TaskId, GroupId, AssignedTo)
•	Tạo bảng Categories (CategoryId, CategoryName, Color)
•	Tạo quan hệ Foreign Keys giữa các bảng
Nhiệm vụ:
•	Define các cột với data types phù hợp
•	Tạo indexes cho performance
•	Tạo constraints (NOT NULL, UNIQUE, CHECK)
•	Insert sample data cho testing
---
1.2. Model Classes
Folder: TimeFlow/Models/ (tạo mới)
File: Models/Task.cs (tạo mới)
•	Class Task với properties: TaskId, Title, Description, DueDate, Priority, Status, CreatedBy, IsGroupTask, GroupId, AssignedTo, Category, CreatedAt, UpdatedAt
•	Enums: TaskPriority (Low, Medium, High), TaskStatus (Pending, InProgress, Completed)
File: Models/Group.cs (tạo mới)
•	Class Group: GroupId, GroupName, CreatedBy, MemberCount, CreatedAt
•	Class GroupMember: GroupId, UserId, Username, Email, Role (Admin/Member), JoinedAt
File: Models/Category.cs (tạo mới)
•	Class Category: CategoryId, CategoryName, Color, Icon
Nhiệm vụ:
•	Define properties với proper data types
•	Implement INotifyPropertyChanged nếu cần binding
•	Add validation attributes (Required, StringLength, Range)
•	Override ToString() cho debugging
---
1.3. Data Access Layer
Folder: TimeFlow/DataAccess/ (tạo mới)
File: DataAccess/DatabaseHelper.cs (tạo mới)
•	Static class chứa connection string
•	Method GetConnection() để tạo SqlConnection
•	Method ExecuteQuery<T>() generic cho SELECT
•	Method ExecuteNonQuery() cho INSERT/UPDATE/DELETE
•	Error handling và logging
File: DataAccess/TaskRepository.cs (tạo mới)
•	Methods:
•	GetUserTasks(int userId) → List<Task>
•	GetGroupTasks(int groupId) → List<Task>
•	CreateTask(Task task) → bool
•	UpdateTask(Task task) → bool
•	DeleteTask(int taskId) → bool
•	GetTaskById(int taskId) → Task
•	GetTasksByDate(DateTime date) → List<Task>
•	GetTasksByStatus(TaskStatus status) → List<Task>
File: DataAccess/GroupRepository.cs (tạo mới)
•	Methods:
•	GetUserGroups(int userId) → List<Group>
•	CreateGroup(Group group) → int (GroupId)
•	UpdateGroup(Group group) → bool
•	DeleteGroup(int groupId) → bool
•	AddMemberToGroup(int groupId, int userId, string role) → bool
•	RemoveMemberFromGroup(int groupId, int userId) → bool
•	GetGroupMembers(int groupId) → List<GroupMember>
•	IsUserInGroup(int userId, int groupId) → bool
File: DataAccess/CategoryRepository.cs (tạo mới)
•	Methods:
•	GetAllCategories() → List<Category>
•	CreateCategory(Category category) → int
•	UpdateCategory(Category category) → bool
•	DeleteCategory(int categoryId) → bool
Nhiệm vụ:
•	Implement CRUD operations với SQL parameterized queries
•	Add transaction support cho complex operations
•	Error handling và logging
•	Return meaningful error messages
---
1.4. Business Logic Layer
Folder: TimeFlow/Services/ (tạo mới)
File: Services/TaskService.cs (tạo mới)
•	Methods:
•	ValidateTask(Task task) → ValidationResult
•	CanUserEditTask(int userId, int taskId) → bool
•	CanUserDeleteTask(int userId, int taskId) → bool
•	GetTasksForCalendar(int userId, DateTime startDate, DateTime endDate) → List<Task>
•	GetOverdueTasks(int userId) → List<Task>
•	GetUpcomingTasks(int userId, int days) → List<Task>
File: Services/GroupService.cs (tạo mới)
•	Methods:
•	ValidateGroup(Group group) → ValidationResult
•	CanUserManageGroup(int userId, int groupId) → bool
•	GetUserRole(int userId, int groupId) → string
•	AssignTaskToMember(int taskId, int userId, int assignedBy) → bool
File: Services/SessionManager.cs (tạo mới)
•	Properties: CurrentUserId, CurrentUsername, CurrentEmail, IsAuthenticated
•	Methods:
•	Initialize(string username, string email, string token)
•	Clear()
•	RefreshSession()
Nhiệm vụ:
•	Implement business rules validation
•	Add authorization checks
•	Implement caching nếu cần
•	Add logging cho audit trail
---
📝 Files cần sửa:
1.5. FormGiaoDien.cs
Thay đổi:
•	Integrate SessionManager để lưu user context
•	Load tasks cho calendar view
•	Update calendar cells với task indicators
•	Add task count badges
Nhiệm vụ:
•	Khởi tạo SessionManager khi form load
•	Fetch và display user tasks
•	Implement calendar click để show tasks của ngày đó
•	Add refresh mechanism
---
1.6. Program.cs
Thay đổi:
•	Change entry point từ FormTaskDetail về FormDangNhap
•	Initialize SessionManager globally
•	Setup exception handling
Nhiệm vụ:
•	Application.Run(new FormDangNhap())
•	Add global exception handler
•	Initialize logging system
---
🎯 YÊU CẦU 2: THÊM TASK CHO CÁ NHÂN
📂 Files cần sửa:
2.1. FormThemTask.cs & Designer.cs
Thay đổi:
•	Implement form logic để create personal task
•	Add validation cho input fields
•	Connect với TaskRepository
•	Show success/error messages
Nhiệm vụ:
•	Bind controls với Task model
•	Implement buttonSave_Click():
•	Validate inputs (required fields, date validation, etc.)
•	Create Task object
•	Set CreatedBy = SessionManager.CurrentUserId
•	Set IsGroupTask = false
•	Call TaskRepository.CreateTask()
•	Show success message
•	Navigate back to FormGiaoDien hoặc refresh task list
•	Implement buttonCancel_Click(): Close form
•	Add date/time pickers với proper validation
•	Add category dropdown với data từ CategoryRepository
•	Add priority selection (ComboBox/RadioButtons)
Controls cần xử lý:
•	textBox1 (Task Name) → Required, max length
•	comboBox1 (Category) → Load từ DB
•	dateTimePicker1 (Due Date) → Không được quá khứ
•	textBox2 (Time) → Format validation
•	comboBox2 (Frequency) → Daily/Weekly/Monthly/Once
•	richTextBox1 (Description) → Optional
---
2.2. TaskList.cs & Designer.cs
Thay đổi:
•	Uncomment code hiện có
•	Fix namespace issues
•	Update AppColors references to TimeFlow.UI.Components.AppColors
•	Implement data loading từ TaskRepository
•	Add event handlers cho task items
Nhiệm vụ:
•	Load tasks từ TaskRepository.GetUserTasks(SessionManager.CurrentUserId)
•	Display tasks trong FlowLayoutPanel với custom rendering
•	Implement click on task item → Navigate to FormTaskDetail
•	Add filters: All/Today/This Week/Overdue
•	Add search functionality
•	Add sort options (by date, priority, status)
•	Implement real-time task count update
---
2.3. FormTaskDetail.cs & Designer.cs
Thay đổi:
•	Convert từ dummy data sang real data
•	Load task detail từ database
•	Implement edit/delete functionality
•	Add comments system
Nhiệm vụ:
•	Add constructor: FormTaskDetail(int taskId)
•	Load task data từ TaskRepository.GetTaskById(taskId)
•	Display task details (title, description, status, priority, due date, assignees)
•	Implement Edit button:
•	Open FormThemTask với edit mode
•	Pre-fill form với task data
•	Implement Delete button:
•	Confirm dialog
•	Call TaskRepository.DeleteTask()
•	Navigate back
•	Implement Change Status button
•	Add comments section (future enhancement)
•	Add activity log (future enhancement)
---
2.4. FormGiaoDien.cs
Thay đổi:
•	Enable "Your Task" button
•	Update Button1_Click để navigate đến TaskList
•	Load và display task count
Nhiệm vụ:
•	Uncomment navigation code: NavigateToForm<TaskList>()
•	Add badge với task count
•	Update calendar với task indicators
---
🎯 YÊU CẦU 3: QUẢN LÝ NHÓM & TASK NHÓM
📂 Files cần tạo mới:
3.1. FormQuanLyNhom.cs & Designer.cs
File: TimeFlow/Groups/FormQuanLyNhom.cs (tạo mới)
Chức năng:
•	Hiển thị danh sách groups của user
•	Create new group
•	Edit/Delete group
•	View group members
•	Manage group settings
UI Components:
•	FlowLayoutPanel cho danh sách groups
•	Button "Create Group"
•	Context menu cho mỗi group (Edit/Delete/Settings)
•	Group card hiển thị: GroupName, MemberCount, TaskCount
Nhiệm vụ:
•	Load groups từ GroupRepository.GetUserGroups()
•	Implement CreateGroup dialog
•	Implement group member list
•	Add/Remove members
•	Assign roles (Admin/Member)
---
3.2. FormThemThanhVien.cs & Designer.cs
File: TimeFlow/Groups/FormThemThanhVien.cs (tạo mới)
Chức năng:
•	Search users để add vào group
•	Display search results
•	Select user và assign role
•	Send invitation (future: notification system)
UI Components:
•	TextBox search by username/email
•	ListBox/DataGridView hiển thị results
•	ComboBox chọn role
•	Button Add/Cancel
Nhiệm vụ:
•	Implement user search trong database
•	Validate user không phải member rồi
•	Call GroupRepository.AddMemberToGroup()
•	Show success message
---
3.3. FormChonNhom.cs & Designer.cs
File: TimeFlow/Groups/FormChonNhom.cs (tạo mới)
Chức năng:
•	Chọn group khi create group task
•	Display user's groups
•	Return selected GroupId
UI Components:
•	ListBox/ComboBox hiển thị groups
•	Button Select/Cancel
Nhiệm vụ:
•	Load groups từ GroupRepository.GetUserGroups()
•	Return GroupId khi select
•	Close form
---
📝 Files cần sửa:
3.4. FormThemTask.cs
Thay đổi:
•	Add option "Personal Task" vs "Group Task"
•	Nếu Group Task: Show group selection
•	Nếu Group Task: Show member selection để assign
Nhiệm vụ:
•	Add RadioButton/CheckBox "Is Group Task"
•	Khi check "Group Task":
•	Show ComboBox chọn group
•	Load groups từ GroupRepository.GetUserGroups()
•	Show ComboBox chọn assignee (group members)
•	Load members từ GroupRepository.GetGroupMembers(groupId)
•	Khi save:
•	Set task.IsGroupTask = true
•	Set task.GroupId = selectedGroupId
•	Set task.AssignedTo = selectedUserId
•	Insert vào GroupTasks table
---
3.5. FormGiaoDien.cs
Thay đổi:
•	Update "Group" button để navigate đến FormQuanLyNhom (thay vì ChatForm)
•	Hoặc: Tách thành 2 buttons riêng: "Groups" và "Chat"
Nhiệm vụ:
•	Add new navigation method cho FormQuanLyNhom
•	Update button2 click handler
•	Load group task count
---
3.6. FormTaskDetail.cs
Thay đổi:
•	Detect nếu là group task
•	Display group info và assigned member
•	Only allow edit/delete nếu user có permission
Nhiệm vụ:
•	Check task.IsGroupTask
•	Load group info nếu là group task
•	Display assignee info
•	Check permission trước khi edit/delete:
•	Personal task: chỉ creator mới edit/delete được
•	Group task: creator hoặc group admin mới edit/delete được
---
3.7. TaskList.cs
Thay đổi:
•	Add tab/filter để view: "My Tasks" vs "Group Tasks"
•	Display assignee info cho group tasks
Nhiệm vụ:
•	Add TabControl hoặc filter buttons
•	Load cả personal và group tasks
•	Display assignee name cho group tasks
•	Color-code personal vs group tasks
---
📊 DATABASE SCHEMA SUMMARY
-- Users table (đã có)
Users (UserId, Username, Password, Email)

-- Tasks table (mới)
Tasks (
  TaskId INT PRIMARY KEY IDENTITY,
  Title NVARCHAR(200) NOT NULL,
  Description NVARCHAR(MAX),
  DueDate DATETIME,
  Priority INT (1=Low, 2=Medium, 3=High),
  Status INT (1=Pending, 2=InProgress, 3=Completed),
  CategoryId INT,
  CreatedBy INT FK → Users.UserId,
  IsGroupTask BIT DEFAULT 0,
  CreatedAt DATETIME DEFAULT GETDATE(),
  UpdatedAt DATETIME
)

-- Groups table (mới)
Groups (
  GroupId INT PRIMARY KEY IDENTITY,
  GroupName NVARCHAR(100) NOT NULL,
  CreatedBy INT FK → Users.UserId,
  CreatedAt DATETIME DEFAULT GETDATE()
)

-- GroupMembers table (mới)
GroupMembers (
  GroupMemberId INT PRIMARY KEY IDENTITY,
  GroupId INT FK → Groups.GroupId,
  UserId INT FK → Users.UserId,
  Role NVARCHAR(20) (Admin/Member),
  JoinedAt DATETIME DEFAULT GETDATE(),
  UNIQUE(GroupId, UserId)
)

-- GroupTasks table (mới)
GroupTasks (
  GroupTaskId INT PRIMARY KEY IDENTITY,
  TaskId INT FK → Tasks.TaskId,
  GroupId INT FK → Groups.GroupId,
  AssignedTo INT FK → Users.UserId (nullable),
  AssignedAt DATETIME
)

-- Categories table (mới)
Categories (
  CategoryId INT PRIMARY KEY IDENTITY,
  CategoryName NVARCHAR(50),
  Color NVARCHAR(7) (hex color)
)
---
🔄 WORKFLOW TỔNG QUAN
FormGiaoDien → Click "New Task" 
  → FormThemTask opens
  → User fills form (không check "Group Task")
  → Click Save
  → TaskService validates
  → TaskRepository.CreateTask()
  → Success message
  → Navigate back to FormGiaoDien

Group Task Creation:
FormGiaoDien → Click "New Task"
  → FormThemTask opens
  → User checks "Group Task"
  → Select Group (FormChonNhom hoặc ComboBox)
  → Select Assignee (ComboBox group members)
  → Fill task details
  → Click Save
  → TaskService validates
  → TaskRepository.CreateTask() + Insert GroupTasks
  → Success message
  → Navigate back

Group Management:
FormGiaoDien → Click "Groups"
  → FormQuanLyNhom opens
  → Display user's groups
  → Click "Create Group"
  → Dialog to enter GroupName
  → GroupRepository.CreateGroup()
  → Click "Add Members"
  → FormThemThanhVien opens
  → Search users
  → Select user + role
  → GroupRepository.AddMemberToGroup()

---
📋 IMPLEMENTATION CHECKLIST
Phase 1: Database & Models (Ưu tiên cao)
•	[ ] Tạo database_tasks_schema.sql
•	[ ] Run script để tạo tables
•	[ ] Tạo Models folder và classes
•	[ ] Test models với sample data
Phase 2: Data Access Layer (Ưu tiên cao)
•	[ ] Tạo DatabaseHelper
•	[ ] Implement TaskRepository
•	[ ] Implement GroupRepository
•	[ ] Implement CategoryRepository
•	[ ] Unit test repositories
Phase 3: Business Logic (Ưu tiên trung bình)
•	[ ] Implement TaskService
•	[ ] Implement GroupService
•	[ ] Implement SessionManager
•	[ ] Integration test services
Phase 4: Personal Task (Ưu tiên cao)
•	[ ] Sửa FormThemTask cho personal task
•	[ ] Sửa TaskList để load real data
•	[ ] Sửa FormTaskDetail cho edit/delete
•	[ ] Test end-to-end personal task flow
Phase 5: Group Management (Ưu tiên trung bình)
•	[ ] Tạo FormQuanLyNhom
•	[ ] Tạo FormThemThanhVien
•	[ ] Implement group CRUD
•	[ ] Test group management flow
Phase 6: Group Task (Ưu tiên trung bình)
•	[ ] Update FormThemTask cho group task
•	[ ] Implement assign task to member
•	[ ] Update TaskList để show group tasks
•	[ ] Update FormTaskDetail cho group task
•	[ ] Test end-to-end group task flow
Phase 7: Integration & Polish (Ưu tiên thấp)
•	[ ] Connect FormGiaoDien với task system
•	[ ] Add task indicators to calendar
•	[ ] Add notifications (overdue tasks)
•	[ ] Performance optimization
•	[ ] UI/UX polish
---
🎯 MỨC ĐỘ ƯU TIÊN
🔴 Critical (Làm trước)
1.	Database schema
2.	Model classes
3.	TaskRepository
4.	FormThemTask personal task
5.	SessionManager
🟡 High (Làm tiếp)
6.	TaskList với real data
7.	FormTaskDetail với edit/delete
8.	GroupRepository
9.	FormQuanLyNhom
🟢 Medium (Làm sau)
10.	Group task trong FormThemTask
11.	FormThemThanhVien
12.	Assign task to members
13.	Permission system
🔵 Low (Optional)
14.	Calendar integration
15.	Notifications
16.	Comments system
17.	Activity log
---
