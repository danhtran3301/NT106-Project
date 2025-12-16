# TimeFlow Database Documentation

## Tổng Quan Hệ Thống

**Database Name:** TimeFlowDB  
**DBMS:** Microsoft SQL Server  
**Version:** 1.0  
**Charset:** Unicode (NVARCHAR)  
**Ngày tạo:** 2025

---

## Mục Lục

1. [Sơ Đồ ERD](#sơ-đồ-erd)
2. [Danh Sách Bảng](#danh-sách-bảng)
3. [Chi Tiết Các Bảng](#chi-tiết-các-bảng)
4. [Quan Hệ Giữa Các Bảng](#quan-hệ-giữa-các-bảng)
5. [Views](#views)
6. [Stored Procedures](#stored-procedures)
7. [Triggers](#triggers)
8. [Indexes](#indexes)

---

## Sơ Đồ ERD

```text
+-------------+         +---------------+         +-------------+
|   Users     |-------->|    Tasks      |<--------|  Categories |
+-------------+         +---------------+         +-------------+
      |                       |                         
      |                       |                         
      +---------------------------------------+      
      |                       |               |      
      |                       |               |      
+-------------+         +---------------+   +---------------+
|   Groups    |-------->|  GroupTasks   |   |   Comments    |
+-------------+         +---------------+   +---------------+
      |                       
      |                       
      |                       
+-------------+         
|GroupMembers |         
+-------------+         
```

---

## Danh Sách Bảng

| STT | Tên Bảng | Mô Tả | Số Cột |
|-----|----------|-------|--------|
| 1 | Users | Quản lý thông tin người dùng | 10 |
| 2 | Categories | Phân loại công việc | 6 |
| 3 | Tasks | Lưu trữ công việc | 12 |
| 4 | Groups | Quản lý nhóm làm việc | 6 |
| 5 | GroupMembers | Thành viên trong nhóm | 6 |
| 6 | GroupTasks | Công việc của nhóm | 6 |
| 7 | Comments | Bình luận trên công việc | 6 |
| 8 | ActivityLog | Lịch sử hoạt động | 5 |
| 9 | UserTokens | Quản lý phiên đăng nhập | 7 |

---

## Chi Tiết Các Bảng

### 1. Bảng Users (Người Dùng)

**Mô tả:** Lưu trữ thông tin tài khoản người dùng trong hệ thống

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| UserId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã định danh duy nhất |
| Username | NVARCHAR(50) | NOT NULL, UNIQUE | Tên đăng nhập (5-50 ký tự) |
| Email | NVARCHAR(100) | NOT NULL, UNIQUE | Email người dùng |
| PasswordHash | NVARCHAR(255) | NOT NULL | Mật khẩu đã mã hóa |
| FullName | NVARCHAR(100) | NULL | Họ và tên đầy đủ |
| AvatarUrl | NVARCHAR(500) | NULL | Đường dẫn ảnh đại diện |
| CreatedAt | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo tài khoản |
| UpdatedAt | DATETIME | NULL | Thời gian cập nhật cuối |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Trạng thái kích hoạt |
| LastLoginAt | DATETIME | NULL | Lần đăng nhập gần nhất |

**Constraints:**
- `CK_Username`: Độ dài username từ 5-50 ký tự
- `CK_Email`: Email phải có định dạng hợp lệ (@domain.ext)

**Indexes:**
- `IX_Users_Username`: Index trên Username
- `IX_Users_Email`: Index trên Email

---

### 2. Bảng Categories (Danh Mục)

**Mô tả:** Phân loại công việc theo chủ đề

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| CategoryId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã danh mục |
| CategoryName | NVARCHAR(50) | NOT NULL, UNIQUE | Tên danh mục |
| Color | NVARCHAR(7) | NOT NULL | Mã màu hex (#RRGGBB) |
| IconName | NVARCHAR(50) | NULL | Tên icon hiển thị |
| CreatedAt | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| IsDefault | BIT | NOT NULL, DEFAULT 0 | Danh mục mặc định |

**Constraints:**
- `CK_Color`: Màu phải theo format hex (#[0-9A-Fa-f]{6})

**Dữ liệu mặc định:**

| CategoryName | Color | IconName |
|--------------|-------|----------|
| Work | #3B82F6 | briefcase |
| Personal | #10B981 | user |
| Study | #F59E0B | book |
| Health | #EF4444 | heart |
| Shopping | #8B5CF6 | shopping-cart |
| Other | #6B7280 | folder |

---

### 3. Bảng Tasks (Công Việc)

**Mô tả:** Lưu trữ thông tin công việc (cá nhân và nhóm)

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| TaskId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã công việc |
| Title | NVARCHAR(200) | NOT NULL | Tiêu đề công việc |
| Description | NVARCHAR(MAX) | NULL | Mô tả chi tiết |
| DueDate | DATETIME | NULL | Hạn hoàn thành |
| Priority | INT | NOT NULL, DEFAULT 2 | Độ ưu tiên (1=Low, 2=Medium, 3=High) |
| Status | INT | NOT NULL, DEFAULT 1 | Trạng thái (1=Pending, 2=InProgress, 3=Completed, 4=Cancelled) |
| CategoryId | INT | NULL, FK to Categories | Danh mục công việc |
| CreatedBy | INT | NOT NULL, FK to Users | Người tạo |
| IsGroupTask | BIT | NOT NULL, DEFAULT 0 | Công việc nhóm hay cá nhân |
| CompletedAt | DATETIME | NULL | Thời gian hoàn thành |
| CreatedAt | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| UpdatedAt | DATETIME | NULL | Thời gian cập nhật |

**Constraints:**
- `CK_Priority`: Priority IN (1, 2, 3)
- `CK_Status`: Status IN (1, 2, 3, 4)
- `CK_Title`: Tiêu đề không được rỗng

**Foreign Keys:**
- `FK_Tasks_Categories`: CategoryId to Categories(CategoryId) ON DELETE SET NULL
- `FK_Tasks_CreatedBy`: CreatedBy to Users(UserId) ON DELETE CASCADE

**Indexes:**
- `IX_Tasks_CreatedBy`: Index trên CreatedBy
- `IX_Tasks_DueDate`: Index trên DueDate
- `IX_Tasks_Status`: Index trên Status
- `IX_Tasks_IsGroupTask`: Index trên IsGroupTask

**Giá trị Priority:**

| Giá Trị | Mô Tả | Màu Hiển Thị |
|---------|-------|--------------|
| 1 | Low (Thấp) | Green (#10B981) |
| 2 | Medium (Trung bình) | Orange (#F97316) |
| 3 | High (Cao) | Red (#EF4444) |

**Giá trị Status:**

| Giá Trị | Mô Tả | Màu Hiển Thị |
|---------|-------|--------------|
| 1 | Pending (Chờ xử lý) | Yellow (#F59E0B) |
| 2 | In Progress (Đang thực hiện) | Blue (#3B82F6) |
| 3 | Completed (Hoàn thành) | Green (#10B981) |
| 4 | Cancelled (Đã hủy) | Gray (#6B7280) |

---

### 4. Bảng Groups (Nhóm)

**Mô tả:** Quản lý các nhóm làm việc

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| GroupId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã nhóm |
| GroupName | NVARCHAR(100) | NOT NULL | Tên nhóm |
| Description | NVARCHAR(500) | NULL | Mô tả nhóm |
| CreatedBy | INT | NOT NULL, FK to Users | Người tạo nhóm |
| CreatedAt | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| UpdatedAt | DATETIME | NULL | Thời gian cập nhật |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Constraints:**
- `CK_GroupName`: Tên nhóm không được rỗng

**Foreign Keys:**
- `FK_Groups_CreatedBy`: CreatedBy to Users(UserId) ON DELETE NO ACTION

**Indexes:**
- `IX_Groups_CreatedBy`: Index trên CreatedBy

---

### 5. Bảng GroupMembers (Thành Viên Nhóm)

**Mô tả:** Quản lý thành viên trong các nhóm

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| GroupMemberId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã thành viên |
| GroupId | INT | NOT NULL, FK to Groups | Mã nhóm |
| UserId | INT | NOT NULL, FK to Users | Mã người dùng |
| Role | NVARCHAR(20) | NOT NULL, DEFAULT 'Member' | Vai trò (Admin/Member) |
| JoinedAt | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tham gia |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Constraints:**
- `CK_Role`: Role IN ('Admin', 'Member')
- `UQ_GroupMembers_GroupUser`: UNIQUE(GroupId, UserId) - Mỗi user chỉ có 1 role trong 1 nhóm

**Foreign Keys:**
- `FK_GroupMembers_Groups`: GroupId to Groups(GroupId) ON DELETE CASCADE
- `FK_GroupMembers_Users`: UserId to Users(UserId) ON DELETE CASCADE

**Indexes:**
- `IX_GroupMembers_GroupId`: Index trên GroupId
- `IX_GroupMembers_UserId`: Index trên UserId

**Giá trị Role:**

| Giá Trị | Quyền Hạn |
|---------|-----------|
| Admin | Quản lý nhóm, thêm/xóa thành viên, assign tasks |
| Member | Xem và thực hiện tasks được giao |

---

### 6. Bảng GroupTasks (Công Việc Nhóm)

**Mô tả:** Liên kết công việc với nhóm và phân công

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| GroupTaskId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã công việc nhóm |
| TaskId | INT | NOT NULL, FK to Tasks | Mã công việc |
| GroupId | INT | NOT NULL, FK to Groups | Mã nhóm |
| AssignedTo | INT | NULL, FK to Users | Người được giao việc |
| AssignedAt | DATETIME | NULL | Thời gian phân công |
| AssignedBy | INT | NULL, FK to Users | Người phân công |

**Constraints:**
- `UQ_GroupTasks_TaskGroup`: UNIQUE(TaskId, GroupId) - Mỗi task chỉ thuộc 1 nhóm

**Foreign Keys:**
- `FK_GroupTasks_Tasks`: TaskId to Tasks(TaskId) ON DELETE CASCADE
- `FK_GroupTasks_Groups`: GroupId to Groups(GroupId) ON DELETE NO ACTION
- `FK_GroupTasks_AssignedTo`: AssignedTo to Users(UserId) ON DELETE NO ACTION
- `FK_GroupTasks_AssignedBy`: AssignedBy to Users(UserId) ON DELETE NO ACTION

**Indexes:**
- `IX_GroupTasks_TaskId`: Index trên TaskId
- `IX_GroupTasks_GroupId`: Index trên GroupId
- `IX_GroupTasks_AssignedTo`: Index trên AssignedTo

---

### 7. Bảng Comments (Bình Luận)

**Mô tả:** Bình luận và thảo luận trên công việc

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| CommentId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã bình luận |
| TaskId | INT | NOT NULL, FK to Tasks | Mã công việc |
| UserId | INT | NOT NULL, FK to Users | Người bình luận |
| CommentText | NVARCHAR(MAX) | NOT NULL | Nội dung bình luận |
| CreatedAt | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| UpdatedAt | DATETIME | NULL | Thời gian chỉnh sửa |
| IsEdited | BIT | NOT NULL, DEFAULT 0 | Đã chỉnh sửa hay chưa |

**Constraints:**
- `CK_CommentText`: Nội dung không được rỗng

**Foreign Keys:**
- `FK_Comments_Tasks`: TaskId to Tasks(TaskId) ON DELETE CASCADE
- `FK_Comments_Users`: UserId to Users(UserId) ON DELETE NO ACTION

**Indexes:**
- `IX_Comments_TaskId`: Index trên TaskId
- `IX_Comments_UserId`: Index trên UserId

---

### 8. Bảng ActivityLog (Lịch Sử Hoạt Động)

**Mô tả:** Ghi lại các thay đổi và hoạt động trong hệ thống

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| LogId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã log |
| TaskId | INT | NULL, FK to Tasks | Mã công việc liên quan |
| UserId | INT | NOT NULL, FK to Users | Người thực hiện |
| ActivityType | NVARCHAR(50) | NOT NULL | Loại hoạt động |
| ActivityDescription | NVARCHAR(500) | NOT NULL | Mô tả chi tiết |
| CreatedAt | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian |

**Foreign Keys:**
- `FK_ActivityLog_Tasks`: TaskId to Tasks(TaskId) ON DELETE CASCADE
- `FK_ActivityLog_Users`: UserId to Users(UserId) ON DELETE NO ACTION

**Indexes:**
- `IX_ActivityLog_TaskId`: Index trên TaskId
- `IX_ActivityLog_UserId`: Index trên UserId
- `IX_ActivityLog_CreatedAt`: Index trên CreatedAt

**Loại ActivityType:**

| ActivityType | Mô Tả |
|--------------|-------|
| Created | Tạo mới công việc |
| Updated | Cập nhật thông tin |
| StatusChanged | Thay đổi trạng thái |
| Assigned | Phân công cho người khác |
| Completed | Hoàn thành công việc |
| Commented | Thêm bình luận |
| Deleted | Xóa công việc |

---

### 9. Bảng UserTokens (Token Xác Thực)

**Mô tả:** Quản lý JWT tokens và sessions

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
|---------|--------------|-----------|-------|
| TokenId | INT | PRIMARY KEY, IDENTITY(1,1) | Mã token |
| UserId | INT | NOT NULL, FK to Users | Mã người dùng |
| Token | NVARCHAR(500) | NOT NULL, UNIQUE | JWT token string |
| CreatedAt | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| ExpiresAt | DATETIME | NOT NULL | Thời gian hết hạn |
| IsRevoked | BIT | NOT NULL, DEFAULT 0 | Token đã bị thu hồi |
| RevokedAt | DATETIME | NULL | Thời gian thu hồi |

**Foreign Keys:**
- `FK_UserTokens_Users`: UserId to Users(UserId) ON DELETE CASCADE

**Indexes:**
- `IX_UserTokens_UserId`: Index trên UserId
- `IX_UserTokens_Token`: Index trên Token
- `IX_UserTokens_ExpiresAt`: Index trên ExpiresAt

---

## Quan Hệ Giữa Các Bảng

### Quan Hệ 1-N (One-to-Many)

| Bảng Cha | Bảng Con | Quan Hệ | Mô Tả |
|----------|----------|---------|-------|
| Users | Tasks | 1-N | Một user tạo nhiều tasks |
| Users | Groups | 1-N | Một user tạo nhiều groups |
| Users | Comments | 1-N | Một user có nhiều comments |
| Users | ActivityLog | 1-N | Một user có nhiều activities |
| Users | UserTokens | 1-N | Một user có nhiều tokens |
| Categories | Tasks | 1-N | Một category có nhiều tasks |
| Groups | GroupMembers | 1-N | Một group có nhiều members |
| Groups | GroupTasks | 1-N | Một group có nhiều tasks |
| Tasks | Comments | 1-N | Một task có nhiều comments |
| Tasks | ActivityLog | 1-N | Một task có nhiều logs |
| Tasks | GroupTasks | 1-1 | Một task có thể thuộc một group |

### Quan Hệ N-N (Many-to-Many)

| Bảng 1 | Bảng Trung Gian | Bảng 2 | Mô Tả |
|--------|----------------|--------|-------|
| Users | GroupMembers | Groups | Nhiều users thuộc nhiều groups |
| Groups | GroupTasks | Tasks | Nhiều groups có nhiều tasks |

---

## Views

### 1. vw_UserTasksSummary

**Mục đích:** Thống kê số lượng tasks theo trạng thái của từng user

**Cột trả về:**

| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| UserId | INT | Mã người dùng |
| Username | NVARCHAR(50) | Tên đăng nhập |
| PendingTasks | INT | Số task đang chờ |
| InProgressTasks | INT | Số task đang làm |
| CompletedTasks | INT | Số task hoàn thành |
| TotalTasks | INT | Tổng số task |

**Sử dụng:**
```sql
SELECT * FROM vw_UserTasksSummary WHERE UserId = 1;
```

---

### 2. vw_GroupTasksWithMembers

**Mục đích:** Hiển thị thông tin chi tiết công việc nhóm và người được giao

**Cột trả về:**

| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| GroupTaskId | INT | Mã group task |
| TaskId | INT | Mã task |
| Title | NVARCHAR(200) | Tiêu đề task |
| Description | NVARCHAR(MAX) | Mô tả |
| DueDate | DATETIME | Hạn hoàn thành |
| Priority | INT | Độ ưu tiên |
| Status | INT | Trạng thái |
| GroupId | INT | Mã nhóm |
| GroupName | NVARCHAR(100) | Tên nhóm |
| AssignedUserId | INT | ID người được giao |
| AssignedUsername | NVARCHAR(50) | Tên người được giao |
| CreatedByUsername | NVARCHAR(50) | Người tạo task |
| AssignedAt | DATETIME | Thời gian phân công |

**Sử dụng:**
```sql
SELECT * FROM vw_GroupTasksWithMembers WHERE GroupId = 1;
```

---

### 3. vw_UserGroups

**Mục đích:** Danh sách nhóm mà user tham gia

**Cột trả về:**

| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| UserId | INT | Mã người dùng |
| Username | NVARCHAR(50) | Tên đăng nhập |
| GroupId | INT | Mã nhóm |
| GroupName | NVARCHAR(100) | Tên nhóm |
| Description | NVARCHAR(500) | Mô tả nhóm |
| Role | NVARCHAR(20) | Vai trò trong nhóm |
| JoinedAt | DATETIME | Thời gian tham gia |
| MemberCount | INT | Số thành viên |

**Sử dụng:**
```sql
SELECT * FROM vw_UserGroups WHERE UserId = 1;
```

---

## Stored Procedures

### 1. sp_GetUserAllTasks

**Mục đích:** Lấy tất cả tasks của user (personal + assigned group tasks)

**Parameters:**

| Tên | Kiểu | Mô Tả |
|-----|------|-------|
| @UserId | INT | Mã người dùng |
| @Status | INT (Optional) | Lọc theo trạng thái |
| @Priority | INT (Optional) | Lọc theo độ ưu tiên |

**Sử dụng:**
```sql
EXEC sp_GetUserAllTasks @UserId = 1, @Status = 2;
EXEC sp_GetUserAllTasks @UserId = 1, @Priority = 3;
```

---

### 2. sp_GetGroupMembers

**Mục đích:** Lấy danh sách thành viên trong nhóm

**Parameters:**

| Tên | Kiểu | Mô Tả |
|-----|------|-------|
| @GroupId | INT | Mã nhóm |

**Sử dụng:**
```sql
EXEC sp_GetGroupMembers @GroupId = 1;
```

---

### 3. sp_CreateTask

**Mục đích:** Tạo task mới (có thể là personal hoặc group task)

**Parameters:**

| Tên | Kiểu | Mô Tả |
|-----|------|-------|
| @Title | NVARCHAR(200) | Tiêu đề |
| @Description | NVARCHAR(MAX) | Mô tả |
| @DueDate | DATETIME | Hạn hoàn thành |
| @Priority | INT | Độ ưu tiên (1-3) |
| @Status | INT | Trạng thái (1-4) |
| @CategoryId | INT | Mã danh mục |
| @CreatedBy | INT | Người tạo |
| @IsGroupTask | BIT | Task nhóm? |
| @GroupId | INT (Optional) | Mã nhóm |
| @AssignedTo | INT (Optional) | Người được giao |
| @TaskId | INT OUTPUT | Trả về TaskId mới |

**Sử dụng:**
```sql
DECLARE @NewTaskId INT;
EXEC sp_CreateTask 
    @Title = N'Complete report', 
    @Description = N'Quarterly report', 
    @DueDate = '2025-12-31',
    @Priority = 3,
    @Status = 1,
    @CategoryId = 1,
    @CreatedBy = 1,
    @IsGroupTask = 0,
    @TaskId = @NewTaskId OUTPUT;
SELECT @NewTaskId;
```

---

### 4. sp_UpdateTaskStatus

**Mục đích:** Cập nhật trạng thái task và ghi log

**Parameters:**

| Tên | Kiểu | Mô Tả |
|-----|------|-------|
| @TaskId | INT | Mã task |
| @Status | INT | Trạng thái mới |
| @UserId | INT | Người thực hiện |

**Sử dụng:**
```sql
EXEC sp_UpdateTaskStatus @TaskId = 1, @Status = 3, @UserId = 1;
```

---

## Triggers

### 1. trg_Tasks_UpdatedAt

**Bảng:** Tasks  
**Event:** AFTER UPDATE  
**Chức năng:** Tự động cập nhật cột UpdatedAt khi có thay đổi

---

### 2. trg_Groups_AutoAddCreator

**Bảng:** Groups  
**Event:** AFTER INSERT  
**Chức năng:** Tự động thêm người tạo nhóm vào GroupMembers với role = 'Admin'

---

## Indexes

### Performance Indexes

| Bảng | Index | Cột | Mục đích |
|------|-------|-----|----------|
| Users | IX_Users_Username | Username | Tăng tốc login |
| Users | IX_Users_Email | Email | Tìm kiếm user |
| Tasks | IX_Tasks_CreatedBy | CreatedBy | Lấy tasks của user |
| Tasks | IX_Tasks_DueDate | DueDate | Sắp xếp theo deadline |
| Tasks | IX_Tasks_Status | Status | Lọc theo trạng thái |
| Tasks | IX_Tasks_IsGroupTask | IsGroupTask | Phân biệt task type |
| GroupMembers | IX_GroupMembers_GroupId | GroupId | Lấy members của group |
| GroupMembers | IX_GroupMembers_UserId | UserId | Lấy groups của user |
| GroupTasks | IX_GroupTasks_TaskId | TaskId | Tìm group của task |
| GroupTasks | IX_GroupTasks_GroupId | GroupId | Lấy tasks của group |
| GroupTasks | IX_GroupTasks_AssignedTo | AssignedTo | Tìm tasks được assign |
| Comments | IX_Comments_TaskId | TaskId | Lấy comments của task |
| ActivityLog | IX_ActivityLog_TaskId | TaskId | Lịch sử của task |
| ActivityLog | IX_ActivityLog_CreatedAt | CreatedAt | Sắp xếp theo thời gian |
| UserTokens | IX_UserTokens_Token | Token | Xác thực token |
| UserTokens | IX_UserTokens_ExpiresAt | ExpiresAt | Kiểm tra token hết hạn |

---

## Bảo Mật

### 1. Ràng Buộc Dữ Liệu (Constraints)

- **Username:** 5-50 ký tự, không ký tự đặc biệt
- **Email:** Định dạng email hợp lệ
- **Password:** Lưu dưới dạng hash (PasswordHash)
- **Color:** Định dạng hex color (#RRGGBB)
- **Priority:** Chỉ nhận giá trị 1, 2, 3
- **Status:** Chỉ nhận giá trị 1, 2, 3, 4
- **Role:** Chỉ nhận 'Admin' hoặc 'Member'

### 2. Foreign Key Policies

| Relationship | ON DELETE | Lý Do |
|--------------|-----------|-------|
| Tasks to Users (CreatedBy) | CASCADE | Xóa user thì xóa tasks của user đó |
| Tasks to Categories | SET NULL | Xóa category không ảnh hưởng tasks |
| GroupMembers to Groups | CASCADE | Xóa group thì xóa members |
| GroupMembers to Users | CASCADE | Xóa user thì xóa khỏi groups |
| GroupTasks to Tasks | CASCADE | Xóa task thì xóa group task |
| GroupTasks to Groups | NO ACTION | Không cho xóa group nếu còn tasks |
| Comments to Tasks | CASCADE | Xóa task thì xóa comments |

### 3. Unique Constraints

- **Users:** Username, Email (không trùng lặp)
- **Categories:** CategoryName (không trùng tên)
- **GroupMembers:** (GroupId, UserId) - Mỗi user chỉ 1 role/group
- **GroupTasks:** (TaskId, GroupId) - Mỗi task chỉ thuộc 1 group
- **UserTokens:** Token (mỗi token là duy nhất)

---

## Quy Trình Nghiệp Vụ

### 1. Đăng Ký Người Dùng

```text
1. Insert vào Users (Username, Email, PasswordHash)
2. Kiểm tra Username và Email chưa tồn tại (UNIQUE constraint)
3. Hash password trước khi lưu
4. IsActive = 1, CreatedAt = GETDATE()
```

### 2. Tạo Task Cá Nhân

```text
1. Insert vào Tasks (Title, Description, DueDate, Priority, Status, CategoryId, CreatedBy)
2. IsGroupTask = 0
3. Insert vào ActivityLog (ActivityType = 'Created')
4. Trigger tự động set CreatedAt
```

### 3. Tạo Task Nhóm

```text
1. Insert vào Tasks (IsGroupTask = 1)
2. Insert vào GroupTasks (TaskId, GroupId, AssignedTo)
3. Insert vào ActivityLog (ActivityType = 'Created' và 'Assigned')
4. Kiểm tra AssignedTo có phải member của GroupId
```

### 4. Tạo Nhóm

```text
1. Insert vào Groups (GroupName, CreatedBy)
2. Trigger tự động insert vào GroupMembers (UserId = CreatedBy, Role = 'Admin')
```

### 5. Thêm Thành Viên Vào Nhóm

```text
1. Kiểm tra UserId tồn tại
2. Kiểm tra chưa là member (UNIQUE constraint)
3. Insert vào GroupMembers (GroupId, UserId, Role)
4. JoinedAt = GETDATE()
```

---

## Tối Ưu Hóa

### 1. Query Optimization

- Sử dụng indexes trên các cột thường xuyên filter/join
- Views để tránh viết lại queries phức tạp
- Stored procedures để tái sử dụng logic

### 2. Data Integrity

- Foreign keys đảm bảo tính toàn vẹn dữ liệu
- Constraints kiểm tra dữ liệu hợp lệ
- Unique constraints tránh duplicate

### 3. Performance

- Index trên Username, Email cho login nhanh
- Index trên DueDate, Status cho filtering
- Index trên foreign keys cho joins

---

## Mở Rộng Tương Lai

### 1. Tính năng có thể thêm

- **Attachments:** Bảng lưu file đính kèm cho tasks
- **Notifications:** Bảng thông báo cho users
- **TaskTags:** Bảng tags cho tasks (N-N relationship)
- **Reminders:** Bảng nhắc nhở deadline
- **TaskHistory:** Bảng lưu lịch sử thay đổi chi tiết

### 2. Cải tiến hiệu suất

- Partitioning cho bảng ActivityLog (theo CreatedAt)
- Archive data cũ vào bảng riêng
- Caching thông tin user thường dùng
- Compression cho bảng Comments

---

## Thống Kê Database

| Chỉ Số | Giá Trị |
|--------|---------|
| Tổng số bảng | 9 |
| Tổng số Views | 3 |
| Tổng số Stored Procedures | 4 |
| Tổng số Functions | 1 |
| Tổng số Triggers | 2 |
| Tổng số Foreign Keys | 17 |
| Tổng số Indexes | 20 |
| Tổng số Constraints | 15 |

---

