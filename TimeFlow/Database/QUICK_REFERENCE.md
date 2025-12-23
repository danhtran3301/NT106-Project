# ?? TimeFlow Test Accounts - Quick Reference

## ?? Test Accounts Overview

| Username | Password | Role | Tasks | Groups | Best For |
|----------|----------|------|-------|--------|----------|
| **admin** | Test@1234 | Power User | 6 personal<br>Leader of 2 groups | Dev Team (Admin)<br>QA Team (Admin) | Full feature testing<br>Dashboard testing<br>Group management |
| **testuser** | Test@1234 | Regular User | 2 personal<br>1 assigned group task | Dev Team (Member)<br>QA Team (Admin) | Normal workflow<br>Task assignment<br>Collaboration |
| **demouser** | Test@1234 | New User | 0 personal<br>1 assigned group task | Dev Team (Member)<br>QA Team (Member) | Clean slate testing<br>First-time user experience |

---

## ?? Login Credentials

### Admin Account (Full Features)
```
Username: admin
Password: Test@1234
```
**Has:** 6 tasks (1 overdue, 2 today, 1 completed), 2 groups as admin

### Test User (Normal User)
```
Username: testuser
Password: Test@1234
```
**Has:** 2 personal tasks, member of 2 groups, 1 group task assigned

### Demo User (Clean Start)
```
Username: demouser
Password: Test@1234
```
**Has:** No personal tasks, member of 2 groups, 1 group task assigned

---

## ?? Database Statistics

```
? Users: 3
? Categories: 6 (Work, Personal, Study, Health, Shopping, Other)
? Personal Tasks: 8
? Group Tasks: 4
? Groups: 2 (Development Team, QA Team)
? Comments: 4
? Activity Logs: 7
```

---

## ?? Task Status Distribution (Admin)

| Status | Count | Priority Distribution |
|--------|-------|----------------------|
| ? Pending | 3 | ?? High: 1<br>?? Medium: 1<br>?? Low: 1 |
| ?? In Progress | 2 | ?? Medium: 2 |
| ? Completed | 1 | ?? High: 1 |

**Special Cases:**
- ?? **1 Overdue Task**: "Complete Project Report" (2 days late)
- ? **2 Due Today**: "Review Code Pull Requests", "Gym Workout"

---

## ?? Group Structure

### Development Team
- **Admin:** admin
- **Members:** testuser, demouser
- **Tasks:** 3 (2 assigned, 1 unassigned)

### QA Team
- **Admin:** testuser, admin
- **Members:** demouser
- **Tasks:** 1 (assigned to admin)

---

## ?? Test Scenarios

### Scenario 1: Login & Dashboard
```
1. Login: admin / Test@1234
2. Expected: 
   - Welcome message v?i fullname "Administrator"
   - Dashboard hi?n th? 6 tasks
   - 1 task màu ?? (overdue)
   - 2 tasks màu vàng (due today)
```

### Scenario 2: Task Filtering
```
1. Login: admin
2. Filter: High Priority
3. Expected: 2 tasks (1 pending overdue, 1 completed)
```

### Scenario 3: Group Collaboration
```
1. Login: testuser
2. Navigate to Development Team
3. Expected: See 3 group tasks
4. Check: 1 task assigned to you ("Implement User Authentication")
```

### Scenario 4: Comments
```
1. Login: admin
2. Open: "Implement User Authentication" task
3. Expected: See 3 comments from different users
```

### Scenario 5: Activity Log
```
1. Login: admin
2. View Activity Log
3. Expected: See logs for:
   - Task created
   - Status changed
   - Tasks assigned
   - Comments added
```

---

## ?? Password Reset (If Needed)

N?u c?n reset password v? `Test@1234`:

```sql
UPDATE Users 
SET PasswordHash = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'
WHERE Username IN ('admin', 'testuser', 'demouser');
```

---

## ?? Quick SQL Queries

### Check user tasks:
```sql
SELECT Title, Priority, Status, DueDate 
FROM Tasks 
WHERE CreatedBy = 1 -- admin
ORDER BY DueDate;
```

### Check group members:
```sql
EXEC sp_GetGroupMembers @GroupId = 1; -- Development Team
```

### Check all tasks for user:
```sql
EXEC sp_GetUserAllTasks @UserId = 1; -- admin
```

### View activity log:
```sql
SELECT TOP 10 
    u.Username, 
    al.ActivityType, 
    al.ActivityDescription, 
    al.CreatedAt
FROM ActivityLog al
INNER JOIN Users u ON al.UserId = u.UserId
ORDER BY al.CreatedAt DESC;
```

---

## ? Pre-Flight Checklist

Tr??c khi test, verify:

- [ ] SQL Server ?ang ch?y
- [ ] Database `TimeFlowDB` t?n t?i
- [ ] 3 users t?n t?i v?i password ?úng
- [ ] 6 categories t?n t?i
- [ ] 12 tasks t?ng c?ng (8 personal + 4 group)
- [ ] 2 groups v?i members
- [ ] TCP Server ?ang ch?y trên port 8080

---

**Pro Tip:** Dùng `admin` account ?? test ??y ?? features, `testuser` ?? test normal workflow, và `demouser` ?? test new user experience! ??
