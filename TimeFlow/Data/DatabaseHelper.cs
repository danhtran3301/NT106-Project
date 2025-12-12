using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace TimeFlow.Data
{
    // Quan ly ket noi va thuc thi cac truy van database
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private readonly int _commandTimeout;

        // Constructor mac dinh voi connection string hardcode (development)
        public DatabaseHelper()
        {
            // SQL Server Authentication (IMPORTANT: Persist Security Info=False de bao mat)
            _connectionString = "Server=localhost;Database=TimeFlowDB;User Id=myuser;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Integrated Security=False;";
            _commandTimeout = 30; // 30 giay timeout
        }

        // Constructor cho phep truyen connection string tu ben ngoai
        public DatabaseHelper(string connectionString, int commandTimeout = 30)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string khong duoc rong", nameof(connectionString));

            _connectionString = connectionString;
            _commandTimeout = commandTimeout;
        }

        // Tao connection moi
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Kiem tra ket noi database
        public bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Khong the ket noi den database", ex);
            }
        }

        // Lay connection string hien tai (de debug)
        public string GetConnectionString() => _connectionString;

        // ================== EXECUTE NON-QUERY ==================
        // Thuc thi INSERT, UPDATE, DELETE - tra ve so dong bi anh huong
        public int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();

                using var cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = _commandTimeout;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new DatabaseException($"Loi khi thuc thi query: {query}", ex);
            }
        }

        // ================== EXECUTE QUERY ==================
        // Thuc thi SELECT - tra ve DataTable
        public DataTable ExecuteQuery(string query, SqlParameter[]? parameters = null)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();

                using var cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = _commandTimeout;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                using var adapter = new SqlDataAdapter(cmd);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                return dataTable;
            }
            catch (SqlException ex)
            {
                throw new DatabaseException($"Loi khi thuc thi query: {query}", ex);
            }
        }

        // ================== EXECUTE SCALAR ==================
        // Thuc thi SELECT COUNT, MAX, MIN, SUM... - tra ve 1 gia tri duy nhat
        public object? ExecuteScalar(string query, SqlParameter[]? parameters = null)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();

                using var cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = _commandTimeout;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw new DatabaseException($"Loi khi thuc thi query: {query}", ex);
            }
        }

        // ================== TRANSACTION SUPPORT ==================
        // Bat dau transaction
        public SqlTransaction BeginTransaction(SqlConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection.BeginTransaction();
        }

        // Commit transaction
        public void CommitTransaction(SqlTransaction transaction)
        {
            try
            {
                transaction?.Commit();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Loi khi commit transaction", ex);
            }
        }

        // Rollback transaction
        public void RollbackTransaction(SqlTransaction transaction)
        {
            try
            {
                transaction?.Rollback();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Loi khi rollback transaction", ex);
            }
        }

        // ================== TRANSACTION HELPER ==================
        // Execute voi transaction tu dong
        public T ExecuteInTransaction<T>(Func<SqlConnection, SqlTransaction, T> action)
        {
            using var conn = GetConnection();
            conn.Open();

            using var transaction = conn.BeginTransaction();
            try
            {
                var result = action(conn, transaction);
                transaction.Commit();
                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new DatabaseException("Loi trong transaction", ex);
            }
        }

        // Execute non-query voi transaction
        public int ExecuteNonQueryWithTransaction(string query, SqlParameter[]? parameters = null)
        {
            return ExecuteInTransaction((conn, transaction) =>
            {
                using var cmd = new SqlCommand(query, conn, transaction);
                cmd.CommandTimeout = _commandTimeout;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteNonQuery();
            });
        }

        // ================== STORED PROCEDURE ==================
        // Thuc thi stored procedure
        public DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[]? parameters = null)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();

                using var cmd = new SqlCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = _commandTimeout;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                using var adapter = new SqlDataAdapter(cmd);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                return dataTable;
            }
            catch (SqlException ex)
            {
                throw new DatabaseException($"Loi khi thuc thi stored procedure: {procedureName}", ex);
            }
        }

        // Thuc thi stored procedure tra ve scalar
        public object? ExecuteStoredProcedureScalar(string procedureName, SqlParameter[]? parameters = null)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();

                using var cmd = new SqlCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = _commandTimeout;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw new DatabaseException($"Loi khi thuc thi stored procedure: {procedureName}", ex);
            }
        }

        // ================== HELPER METHODS ==================
        // Kiem tra ton tai record
        public bool Exists(string tableName, string whereClause, SqlParameter[]? parameters = null)
        {
            var query = $"SELECT COUNT(*) FROM {tableName} WHERE {whereClause}";
            var count = ExecuteScalar(query, parameters);
            return count != null && Convert.ToInt32(count) > 0;
        }

        // Lay gia tri identity/auto-increment vua insert
        public int GetLastInsertedId()
        {
            var result = ExecuteScalar("SELECT SCOPE_IDENTITY()");
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Execute query va lay ID vua insert (combo)
        public int ExecuteInsertAndGetId(string query, SqlParameter[]? parameters = null)
        {
            var queryWithId = query + "; SELECT SCOPE_IDENTITY();";
            var result = ExecuteScalar(queryWithId, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }

    // ================== CUSTOM EXCEPTION ==================
    // Exception rieng cho database errors
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
