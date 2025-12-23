# TimeFlow Models Documentation

## T?ng Quan

Th? m?c **Models** ch?a các class ??i di?n cho entities trong database và DTOs (Data Transfer Objects) ?? truy?n d? li?u.

---

## C?u Trúc Th? M?c

```
Models/
??? User.cs                    # User entity
??? Category.cs                # Category entity
??? TaskItem.cs               # Task entity
??? Group.cs                  # Group entity
??? GroupMember.cs            # GroupMember entity
??? GroupTask.cs              # GroupTask entity
??? Comment.cs                # Comment entity
??? ActivityLog.cs            # ActivityLog entity
??? UserToken.cs              # UserToken entity
??? DTOs/
    ??? DataTransferObjects.cs # All DTOs
```

---

## Entity Models

### 1. User.cs

**Mô t?:** ??i di?n cho ng??i dùng trong h? th?ng

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| UserId | int | Primary key |
| Username | string | Tên ??ng nh?p (unique) |
| Email | string | Email (unique) |
| PasswordHash | string | Password ?ã hash |
| FullName | string? | H? tên ??y ?? |
| AvatarUrl | string? | URL ?nh ??i di?n |
| CreatedAt | DateTime | Ngày t?o |
| UpdatedAt | DateTime? | Ngày c?p nh?t |
| IsActive | bool | Tr?ng thái kích ho?t |
| LastLoginAt | DateTime? | L?n ??ng nh?p cu?i |

**Navigation Properties:**
- `CreatedTasks` - Danh sách tasks do user t?o
- `CreatedGroups` - Danh sách groups do user t?o
- `GroupMemberships` - Danh sách nhóm user tham gia
- `Comments` - Danh sách comments c?a user
- `Tokens` - Danh sách tokens c?a user

**Helper Properties:**
- `DisplayName` - Tr? v? FullName n?u có, không thì Username
- `HasAvatar` - Check xem có avatar không

---

### 2. Category.cs

**Mô t?:** Phân lo?i tasks theo ch? ??

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| CategoryId | int | Primary key |
| CategoryName | string | Tên category (unique) |
| Color | string | Mã màu hex (#RRGGBB) |
| IconName | string? | Tên icon |
| CreatedAt | DateTime | Ngày t?o |
| IsDefault | bool | Category m?c ??nh c?a h? th?ng |

**Navigation Properties:**
- `Tasks` - Danh sách tasks thu?c category

**Methods:**
- `GetColor()` - Convert hex string thành System.Drawing.Color
- `IsValidColor()` - Validate format hex color

**Default Categories:**
- Work (#3B82F6 - Blue)
- Personal (#10B981 - Green)
- Study (#F59E0B - Yellow)
- Health (#EF4444 - Red)
- Shopping (#8B5CF6 - Purple)
- Other (#6B7280 - Gray)

---

### 3. TaskItem.cs

**Mô t?:** ??i di?n cho m?t task (cá nhân ho?c nhóm)

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| TaskId | int | Primary key |
| Title | string | Tiêu ?? task |
| Description | string? | Mô t? chi ti?t |
| DueDate | DateTime? | H?n hoàn thành |
| Priority | TaskPriority | ?? ?u tiên (enum) |
| Status | TaskStatus | Tr?ng thái (enum) |
| CategoryId | int? | FK to Category |
| CreatedBy | int | FK to User |
| IsGroupTask | bool | Task nhóm hay cá nhân |
| CompletedAt | DateTime? | Th?i gian hoàn thành |
| CreatedAt | DateTime | Ngày t?o |
| UpdatedAt | DateTime? | Ngày c?p nh?t |

**Enums:**

```csharp
public enum TaskPriority
{
    Low = 1,      // #10B981 Green
    Medium = 2,   // #F97316 Orange
    High = 3      // #EF4444 Red
}

public enum TaskStatus
{
    Pending = 1,      // #F59E0B Yellow
    InProgress = 2,   // #3B82F6 Blue
    Completed = 3,    // #10B981 Green
    Cancelled = 4     // #6B7280 Gray
}
```

**Navigation Properties:**
- `Category` - Category c?a task
- `Creator` - User t?o task
- `Comments` - Danh sách comments
- `ActivityLogs` - L?ch s? ho?t ??ng
- `GroupTask` - Thông tin group task (n?u là group task)

**Helper Properties:**
- `IsOverdue` - Check xem task có quá h?n không
- `DaysUntilDue` - S? ngày còn l?i (âm n?u quá h?n)
- `StatusText` - Text hi?n th? status
- `PriorityText` - Text hi?n th? priority
- `StatusColor` - Hex color cho status
- `PriorityColor` - Hex color cho priority

**Methods:**
- `MarkAsCompleted()` - ?ánh d?u hoàn thành
- `CanEdit(userId)` - Check quy?n edit

---

### 4. Group.cs

**Mô t?:** ??i di?n cho m?t nhóm làm vi?c

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| GroupId | int | Primary key |
| GroupName | string | Tên nhóm |
| Description | string? | Mô t? nhóm |
| CreatedBy | int | FK to User |
| CreatedAt | DateTime | Ngày t?o |
| UpdatedAt | DateTime? | Ngày c?p nh?t |
| IsActive | bool | Tr?ng thái ho?t ??ng |

**Navigation Properties:**
- `Creator` - User t?o group
- `Members` - Danh sách members
- `GroupTasks` - Danh sách tasks c?a group

**Helper Properties:**
- `ActiveMemberCount` - S? members ?ang active
- `TaskCount` - S? tasks trong group

**Methods:**
- `IsMember(userId)` - Check user có là member không
- `IsAdmin(userId)` - Check user có ph?i admin không

---

### 5. GroupMember.cs

**Mô t?:** ??i di?n cho m?t thành viên trong nhóm

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| GroupMemberId | int | Primary key |
| GroupId | int | FK to Group |
| UserId | int | FK to User |
| Role | GroupRole | Vai trò (enum) |
| JoinedAt | DateTime | Ngày tham gia |
| IsActive | bool | Tr?ng thái |

**Enum:**

```csharp
public enum GroupRole
{
    Member,  // Thành viên th??ng
    Admin    // Qu?n tr? viên
}
```

**Navigation Properties:**
- `Group` - Group mà member thu?c v?
- `User` - User thông tin

**Helper Properties:**
- `RoleText` - "Admin" ho?c "Member"
- `IsAdmin` - True n?u là admin
- `MembershipDays` - S? ngày ?ã tham gia

---

### 6. GroupTask.cs

**Mô t?:** Liên k?t task v?i group và phân công

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| GroupTaskId | int | Primary key |
| TaskId | int | FK to Task |
| GroupId | int | FK to Group |
| AssignedTo | int? | FK to User (ng??i ???c giao) |
| AssignedAt | DateTime? | Th?i gian phân công |
| AssignedBy | int? | FK to User (ng??i phân công) |

**Navigation Properties:**
- `Task` - Task thông tin
- `Group` - Group thông tin
- `AssignedUser` - User ???c giao task
- `AssignerUser` - User ?ã phân công

**Helper Properties:**
- `IsAssigned` - Check ?ã assign ch?a
- `AssignedToName` - Tên user ???c assign ho?c "Unassigned"

**Methods:**
- `AssignTo(userId, assignedBy)` - Assign task cho user
- `Unassign()` - H?y assignment

---

### 7. Comment.cs

**Mô t?:** Bình lu?n trên task

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| CommentId | int | Primary key |
| TaskId | int | FK to Task |
| UserId | int | FK to User |
| CommentText | string | N?i dung comment |
| CreatedAt | DateTime | Th?i gian t?o |
| UpdatedAt | DateTime? | Th?i gian s?a |
| IsEdited | bool | ?ã ch?nh s?a ch?a |

**Navigation Properties:**
- `Task` - Task ???c comment
- `User` - User comment

**Helper Properties:**
- `TimeAgo` - Hi?n th? th?i gian d?ng "5 minutes ago"

**Methods:**
- `UpdateText(newText)` - C?p nh?t text và ?ánh d?u edited
- `CanEdit(userId)` - Check quy?n edit

---

### 8. ActivityLog.cs

**Mô t?:** Ghi log các ho?t ??ng trong h? th?ng

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| LogId | int | Primary key |
| TaskId | int? | FK to Task (nullable) |
| UserId | int | FK to User |
| ActivityType | string | Lo?i ho?t ??ng |
| ActivityDescription | string | Mô t? |
| CreatedAt | DateTime | Th?i gian |

**Activity Types:**
- Created
- Updated
- StatusChanged
- Assigned
- Completed
- Commented
- Deleted

**Navigation Properties:**
- `Task` - Task liên quan
- `User` - User th?c hi?n

**Helper Properties:**
- `TimeAgo` - Th?i gian d?ng "5m ago"
- `FormattedDescription` - "Username + Description"

**Static Methods:**
- `Create(userId, taskId, type, description)` - Factory method

---

### 9. UserToken.cs

**Mô t?:** Qu?n lý authentication tokens

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| TokenId | int | Primary key |
| UserId | int | FK to User |
| Token | string | JWT token string |
| CreatedAt | DateTime | Th?i gian t?o |
| ExpiresAt | DateTime | Th?i gian h?t h?n |
| IsRevoked | bool | Token b? thu h?i |
| RevokedAt | DateTime? | Th?i gian thu h?i |

**Navigation Properties:**
- `User` - User s? h?u token

**Helper Properties:**
- `IsValid` - Token còn hi?u l?c
- `IsExpired` - Token ?ã h?t h?n
- `TimeUntilExpiration` - Th?i gian còn l?i
- `HoursUntilExpiration` - S? gi? còn l?i

**Methods:**
- `Revoke()` - Thu h?i token

**Static Methods:**
- `CreateToken(userId, token, hours)` - T?o token m?i

---

## Data Transfer Objects (DTOs)

### Authentication DTOs

#### LoginRequest
```csharp
{
    string Username
    string Password
}
```

#### RegisterRequest
```csharp
{
    string Username
    string Email
    string Password
    string? FullName
}
```

#### AuthResponse
```csharp
{
    bool Success
    string? Token
    int UserId
    string Username
    string Email
    string? Message
}
```

---

### Task DTOs

#### CreateTaskRequest
```csharp
{
    string Title
    string? Description
    DateTime? DueDate
    int Priority              // 1=Low, 2=Medium, 3=High
    int Status                // 1=Pending, 2=InProgress, 3=Completed, 4=Cancelled
    int? CategoryId
    bool IsGroupTask
    int? GroupId
    int? AssignedTo
}
```

#### UpdateTaskRequest
```csharp
{
    int TaskId
    string? Title
    string? Description
    DateTime? DueDate
    int? Priority
    int? Status
    int? CategoryId
}
```

#### TaskSummary
```csharp
{
    int TaskId
    string Title
    DateTime? DueDate
    string Priority          // "Low", "Medium", "High"
    string Status            // "Pending", "In Progress", etc.
    string? CategoryName
    string? CategoryColor
    bool IsGroupTask
    string? GroupName
    int? AssignedTo
    string? AssignedToName
}
```

---

### Group DTOs

#### CreateGroupRequest
```csharp
{
    string GroupName
    string? Description
}
```

#### AddGroupMemberRequest
```csharp
{
    int GroupId
    int UserId
    string Role              // "Admin" or "Member"
}
```

#### GroupMemberInfo
```csharp
{
    int UserId
    string Username
    string? FullName
    string Email
    string Role
    DateTime JoinedAt
}
```

#### GroupInfo
```csharp
{
    int GroupId
    string GroupName
    string? Description
    int MemberCount
    int TaskCount
    string CreatedByUsername
    DateTime CreatedAt
    string UserRole          // Current user's role
}
```

---

### Comment DTOs

#### AddCommentRequest
```csharp
{
    int TaskId
    string CommentText
}
```

#### CommentDisplay
```csharp
{
    int CommentId
    string Username
    string? FullName
    string CommentText
    DateTime CreatedAt
    string TimeAgo
    bool IsEdited
}
```

---

### Statistics DTO

#### UserStatistics
```csharp
{
    int TotalTasks
    int PendingTasks
    int InProgressTasks
    int CompletedTasks
    int OverdueTasks
    int GroupsCount
}
```

---

## S? D?ng Models

### Ví d? 1: T?o User m?i

```csharp
var user = new User
{
    Username = "john_doe",
    Email = "john@example.com",
    PasswordHash = hashedPassword,
    FullName = "John Doe"
};
// CreatedAt và IsActive t? ??ng set trong constructor
```

### Ví d? 2: T?o Task

```csharp
var task = new TaskItem
{
    Title = "Complete project",
    Description = "Finish the TimeFlow project",
    DueDate = DateTime.Now.AddDays(7),
    Priority = TaskPriority.High,
    CategoryId = 1,
    CreatedBy = currentUserId
};
// Status m?c ??nh là Pending, IsGroupTask = false
```

### Ví d? 3: Check Task Overdue

```csharp
if (task.IsOverdue)
{
    MessageBox.Show($"Task is overdue by {Math.Abs(task.DaysUntilDue.Value)} days!");
}
```

### Ví d? 4: T?o Group và Add Member

```csharp
var group = new Group
{
    GroupName = "Development Team",
    Description = "Main development team",
    CreatedBy = currentUserId
};

// Sau khi save group, thêm member
var member = new GroupMember
{
    GroupId = group.GroupId,
    UserId = newMemberId,
    Role = GroupRole.Member
};
```

### Ví d? 5: Assign Task to Group Member

```csharp
var groupTask = new GroupTask
{
    TaskId = task.TaskId,
    GroupId = group.GroupId
};

groupTask.AssignTo(memberId, currentUserId);
```

### Ví d? 6: Add Comment

```csharp
var comment = new Comment
{
    TaskId = task.TaskId,
    UserId = currentUserId,
    CommentText = "This looks good!"
};

// Hi?n th?: "5 minutes ago"
Console.WriteLine(comment.TimeAgo);
```

### Ví d? 7: Log Activity

```csharp
var log = ActivityLog.Create(
    userId: currentUserId,
    taskId: task.TaskId,
    activityType: "Completed",
    description: "marked task as completed"
);
```

---

## Best Practices

### 1. Validation

Luôn validate d? li?u tr??c khi l?u:

```csharp
if (string.IsNullOrWhiteSpace(task.Title))
    throw new ArgumentException("Title is required");

if (task.Priority < 1 || task.Priority > 3)
    throw new ArgumentException("Invalid priority");
```

### 2. Navigation Properties

S? d?ng navigation properties ?? load related data:

```csharp
// Load task v?i category và creator
var task = repository.GetTaskWithRelations(taskId);
Console.WriteLine($"{task.Title} - {task.Category?.CategoryName} - By: {task.Creator.DisplayName}");
```

### 3. Helper Methods

T?n d?ng helper methods ?? ??n gi?n hóa code:

```csharp
// Thay vì:
if (group.Members.Any(m => m.UserId == userId && m.Role == GroupRole.Admin && m.IsActive))
{
    // Admin logic
}

// Dùng:
if (group.IsAdmin(userId))
{
    // Admin logic
}
```

### 4. DTOs cho API/UI

S? d?ng DTOs khi truy?n d? li?u qua layers:

```csharp
// Thay vì tr? v? entity tr?c ti?p
public TaskItem GetTask(int id) { }

// Dùng DTO
public TaskSummary GetTaskSummary(int id) { }
```

---

## Mapping Entity ? DTO

### Task Entity ? TaskSummary DTO

```csharp
public static TaskSummary ToSummary(TaskItem task)
{
    return new TaskSummary
    {
        TaskId = task.TaskId,
        Title = task.Title,
        DueDate = task.DueDate,
        Priority = task.PriorityText,
        Status = task.StatusText,
        CategoryName = task.Category?.CategoryName,
        CategoryColor = task.Category?.Color,
        IsGroupTask = task.IsGroupTask,
        GroupName = task.GroupTask?.Group.GroupName
    };
}
```

### CreateTaskRequest ? TaskItem Entity

```csharp
public static TaskItem FromRequest(CreateTaskRequest request, int userId)
{
    return new TaskItem
    {
        Title = request.Title,
        Description = request.Description,
        DueDate = request.DueDate,
        Priority = (TaskPriority)request.Priority,
        Status = (TaskStatus)request.Status,
        CategoryId = request.CategoryId,
        CreatedBy = userId,
        IsGroupTask = request.IsGroupTask
    };
}
```

---

## Notes

- T?t c? entities ??u có constructor kh?i t?o giá tr? m?c ??nh
- Navigation properties ???c init v?i empty list ?? tránh null reference
- Nullable types (`string?`, `int?`, `DateTime?`) cho phép giá tr? null
- Enums ???c dùng thay cho magic numbers
- Helper properties/methods gi?m code duplication
- DTOs tách bi?t logic UI/API kh?i entities

---

**Version:** 1.0  
**Updated:** 2025
