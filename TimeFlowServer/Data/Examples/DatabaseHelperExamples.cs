using System;
using Microsoft.Data.SqlClient;
using System.Data;
using TimeFlow.Data;
using TimeFlow.Data.Configuration;

namespace TimeFlow.Data.Examples
{
    // Vi du su dung DatabaseHelper
    public static class DatabaseHelperExamples
    {
        // ================== BASIC USAGE ==================
        
        // Vi du 1: Khoi tao DatabaseHelper
        public static void Example1_InitializeHelper()
        {
            // Cach 1: Dung connection string mac dinh
            var db = new DatabaseHelper();

            // Cach 2: Truyen connection string tu ben ngoai
            var customDb = new DatabaseHelper(
                "Data Source=localhost;Initial Catalog=TimeFlowDB;Integrated Security=True;TrustServerCertificate=True",
                commandTimeout: 60
            );

            // Cach 3: Dung tu DbConfig
            var configDb = new DatabaseHelper(
                DbConfig.GetConnectionString(),
                DbConfig.GetCommandTimeout()
            );
        }

        // Vi du 2: Test connection
        public static void Example2_TestConnection()
        {
            var db = new DatabaseHelper();
            
            try
            {
                if (db.TestConnection())
                {
                    Console.WriteLine("Ket noi thanh cong!");
                }
            }
            catch (DatabaseException ex)
            {
                Console.WriteLine($"Loi ket noi: {ex.Message}");
            }
        }

        // ================== SELECT QUERIES ==================

        // Vi du 3: ExecuteQuery - Lay danh sach users
        public static void Example3_ExecuteQuery()
        {
            var db = new DatabaseHelper();
            
            var query = "SELECT UserId, Username, Email FROM Users WHERE IsActive = @isActive";
            var parameters = new[]
            {
                new SqlParameter("@isActive", true)
            };

            DataTable result = db.ExecuteQuery(query, parameters);

            foreach (DataRow row in result.Rows)
            {
                Console.WriteLine($"User: {row["Username"]}, Email: {row["Email"]}");
            }
        }

        // Vi du 4: ExecuteScalar - Dem so luong users
        public static void Example4_ExecuteScalar()
        {
            var db = new DatabaseHelper();
            
            var query = "SELECT COUNT(*) FROM Users WHERE IsActive = @isActive";
            var parameters = new[]
            {
                new SqlParameter("@isActive", true)
            };

            var count = db.ExecuteScalar(query, parameters);
            Console.WriteLine($"So luong users active: {count}");
        }

        // Vi du 5: Kiem tra ton tai
        public static void Example5_CheckExists()
        {
            var db = new DatabaseHelper();
            
            var exists = db.Exists(
                tableName: "Users",
                whereClause: "Username = @username",
                parameters: new[] { new SqlParameter("@username", "john_doe") }
            );

            Console.WriteLine(exists ? "User ton tai" : "User khong ton tai");
        }

        // ================== INSERT/UPDATE/DELETE ==================

        // Vi du 6: INSERT - Them user moi
        public static void Example6_Insert()
        {
            var db = new DatabaseHelper();
            
            var query = @"INSERT INTO Users (Username, Email, PasswordHash, FullName, IsActive, CreatedAt) 
                          VALUES (@username, @email, @password, @fullname, @active, GETDATE())";
            
            var parameters = new[]
            {
                new SqlParameter("@username", "john_doe"),
                new SqlParameter("@email", "john@example.com"),
                new SqlParameter("@password", "hashed_password_here"),
                new SqlParameter("@fullname", "John Doe"),
                new SqlParameter("@active", true)
            };

            int rowsAffected = db.ExecuteNonQuery(query, parameters);
            Console.WriteLine($"Da them {rowsAffected} user moi");
        }

        // Vi du 7: INSERT va lay ID vua them
        public static void Example7_InsertAndGetId()
        {
            var db = new DatabaseHelper();
            
            var query = @"INSERT INTO Tasks (Title, Description, Priority, Status, CreatedBy, CreatedAt) 
                          VALUES (@title, @desc, @priority, @status, @createdBy, GETDATE())";
            
            var parameters = new[]
            {
                new SqlParameter("@title", "Hoan thanh bao cao"),
                new SqlParameter("@desc", "Bao cao hang tuan"),
                new SqlParameter("@priority", 2), // Medium
                new SqlParameter("@status", 1),   // Pending
                new SqlParameter("@createdBy", 1)
            };

            int newTaskId = db.ExecuteInsertAndGetId(query, parameters);
            Console.WriteLine($"Task moi co ID: {newTaskId}");
        }

        // Vi du 8: UPDATE
        public static void Example8_Update()
        {
            var db = new DatabaseHelper();
            
            var query = @"UPDATE Tasks 
                          SET Status = @status, UpdatedAt = GETDATE() 
                          WHERE TaskId = @taskId";
            
            var parameters = new[]
            {
                new SqlParameter("@status", 3), // Completed
                new SqlParameter("@taskId", 1)
            };

            int rowsAffected = db.ExecuteNonQuery(query, parameters);
            Console.WriteLine($"Da cap nhat {rowsAffected} task");
        }

        // Vi du 9: DELETE
        public static void Example9_Delete()
        {
            var db = new DatabaseHelper();
            
            var query = "DELETE FROM Comments WHERE CommentId = @commentId";
            var parameters = new[] { new SqlParameter("@commentId", 5) };

            int rowsAffected = db.ExecuteNonQuery(query, parameters);
            Console.WriteLine($"Da xoa {rowsAffected} comment");
        }

        // ================== TRANSACTIONS ==================

        // Vi du 10: Transaction co ban (manual)
        public static void Example10_ManualTransaction()
        {
            var db = new DatabaseHelper();
            
            using var conn = new SqlConnection(db.GetConnectionString());
            conn.Open();
            
            using var transaction = db.BeginTransaction(conn);
            
            try
            {
                // Buoc 1: Tao task
                var insertTask = @"INSERT INTO Tasks (Title, CreatedBy, CreatedAt) 
                                   VALUES (@title, @createdBy, GETDATE()); 
                                   SELECT SCOPE_IDENTITY();";
                
                using var cmd1 = new SqlCommand(insertTask, conn, transaction);
                cmd1.Parameters.AddWithValue("@title", "Task moi");
                cmd1.Parameters.AddWithValue("@createdBy", 1);
                int taskId = Convert.ToInt32(cmd1.ExecuteScalar());

                // Buoc 2: Tao activity log
                var insertLog = @"INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt) 
                                  VALUES (@taskId, @userId, @type, @desc, GETDATE())";
                
                using var cmd2 = new SqlCommand(insertLog, conn, transaction);
                cmd2.Parameters.AddWithValue("@taskId", taskId);
                cmd2.Parameters.AddWithValue("@userId", 1);
                cmd2.Parameters.AddWithValue("@type", "Created");
                cmd2.Parameters.AddWithValue("@desc", "Task created");
                cmd2.ExecuteNonQuery();

                // Commit neu thanh cong
                db.CommitTransaction(transaction);
                Console.WriteLine("Transaction thanh cong!");
            }
            catch (Exception ex)
            {
                // Rollback neu co loi
                db.RollbackTransaction(transaction);
                Console.WriteLine($"Transaction that bai: {ex.Message}");
            }
        }

        // Vi du 11: Transaction tu dong
        public static void Example11_AutoTransaction()
        {
            var db = new DatabaseHelper();
            
            try
            {
                var newTaskId = db.ExecuteInTransaction((conn, transaction) =>
                {
                    // Buoc 1: Insert task
                    var query1 = @"INSERT INTO Tasks (Title, CreatedBy, CreatedAt) 
                                   VALUES (@title, @userId, GETDATE()); 
                                   SELECT SCOPE_IDENTITY();";
                    
                    using var cmd1 = new SqlCommand(query1, conn, transaction);
                    cmd1.Parameters.AddWithValue("@title", "Task trong transaction");
                    cmd1.Parameters.AddWithValue("@userId", 1);
                    int taskId = Convert.ToInt32(cmd1.ExecuteScalar());

                    // Buoc 2: Insert log
                    var query2 = @"INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt) 
                                   VALUES (@taskId, @userId, @type, @desc, GETDATE())";
                    
                    using var cmd2 = new SqlCommand(query2, conn, transaction);
                    cmd2.Parameters.AddWithValue("@taskId", taskId);
                    cmd2.Parameters.AddWithValue("@userId", 1);
                    cmd2.Parameters.AddWithValue("@type", "Created");
                    cmd2.Parameters.AddWithValue("@desc", "Created task");
                    cmd2.ExecuteNonQuery();

                    return taskId; // Tra ve taskId
                });

                Console.WriteLine($"Task moi co ID: {newTaskId}");
            }
            catch (DatabaseException ex)
            {
                Console.WriteLine($"Loi: {ex.Message}");
            }
        }

        // ================== STORED PROCEDURES ==================

        // Vi du 12: Execute stored procedure
        public static void Example12_StoredProcedure()
        {
            var db = new DatabaseHelper();
            
            // Goi SP: sp_GetUserAllTasks
            var parameters = new[]
            {
                new SqlParameter("@UserId", 1),
                new SqlParameter("@Status", 2) // InProgress
            };

            DataTable tasks = db.ExecuteStoredProcedure("sp_GetUserAllTasks", parameters);

            foreach (DataRow row in tasks.Rows)
            {
                Console.WriteLine($"Task: {row["Title"]}, Due: {row["DueDate"]}");
            }
        }

        // Vi du 13: Stored procedure tra ve scalar
        public static void Example13_StoredProcedureScalar()
        {
            var db = new DatabaseHelper();
            
            var parameters = new[]
            {
                new SqlParameter("@UserId", 1)
            };

            // Goi function: fn_GetOverdueTasksCount
            var count = db.ExecuteStoredProcedureScalar("SELECT dbo.fn_GetOverdueTasksCount(@UserId)", parameters);
            Console.WriteLine($"So task qua han: {count}");
        }

        // ================== COMPLEX EXAMPLE ==================

        // Vi du 14: Tao task voi group va log (full workflow)
        public static void Example14_ComplexWorkflow()
        {
            var db = new DatabaseHelper();

            try
            {
                var newTaskId = db.ExecuteInTransaction((conn, transaction) =>
                {
                    // 1. Tao task
                    var insertTask = @"
                        INSERT INTO Tasks (Title, Description, Priority, Status, IsGroupTask, CreatedBy, CreatedAt)
                        VALUES (@title, @desc, @priority, @status, @isGroup, @userId, GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    using var cmd1 = new SqlCommand(insertTask, conn, transaction);
                    cmd1.Parameters.AddWithValue("@title", "Group project");
                    cmd1.Parameters.AddWithValue("@desc", "Complete the project");
                    cmd1.Parameters.AddWithValue("@priority", 3);
                    cmd1.Parameters.AddWithValue("@status", 1);
                    cmd1.Parameters.AddWithValue("@isGroup", true);
                    cmd1.Parameters.AddWithValue("@userId", 1);
                    int taskId = Convert.ToInt32(cmd1.ExecuteScalar());

                    // 2. Gan task vao group
                    var insertGroupTask = @"
                        INSERT INTO GroupTasks (TaskId, GroupId, AssignedTo, AssignedBy, AssignedAt)
                        VALUES (@taskId, @groupId, @assignedTo, @assignedBy, GETDATE())";

                    using var cmd2 = new SqlCommand(insertGroupTask, conn, transaction);
                    cmd2.Parameters.AddWithValue("@taskId", taskId);
                    cmd2.Parameters.AddWithValue("@groupId", 1);
                    cmd2.Parameters.AddWithValue("@assignedTo", 2);
                    cmd2.Parameters.AddWithValue("@assignedBy", 1);
                    cmd2.ExecuteNonQuery();

                    // 3. Ghi log
                    var insertLog = @"
                        INSERT INTO ActivityLog (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
                        VALUES (@taskId, @userId, @type, @desc, GETDATE())";

                    using var cmd3 = new SqlCommand(insertLog, conn, transaction);
                    cmd3.Parameters.AddWithValue("@taskId", taskId);
                    cmd3.Parameters.AddWithValue("@userId", 1);
                    cmd3.Parameters.AddWithValue("@type", "Created");
                    cmd3.Parameters.AddWithValue("@desc", "Created group task and assigned to user 2");
                    cmd3.ExecuteNonQuery();

                    return taskId;
                });

                Console.WriteLine($"Tao group task thanh cong! ID: {newTaskId}");
            }
            catch (DatabaseException ex)
            {
                Console.WriteLine($"Loi tao task: {ex.Message}");
            }
        }
    }
}
