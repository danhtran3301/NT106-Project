using System;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Data.Configuration;

namespace TimeFlow.Data.Repositories
{
    // Base repository cho tat ca repositories
    // Chua cac method chung va DatabaseHelper instance
    public abstract class BaseRepository
    {
        protected readonly DatabaseHelper _db;

        // Constructor khoi tao DatabaseHelper
        protected BaseRepository()
        {
            _db = new DatabaseHelper(
                DbConfig.GetConnectionString(),
                DbConfig.GetCommandTimeout()
            );
        }

        // Constructor cho custom DatabaseHelper (dung cho testing)
        protected BaseRepository(DatabaseHelper databaseHelper)
        {
            _db = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
        }

        // ================== HELPER METHODS ==================

        // Kiem tra record ton tai
        protected bool Exists(string tableName, string whereClause, SqlParameter[] parameters)
        {
            return _db.Exists(tableName, whereClause, parameters);
        }

        // Lay gia tri tu DataRow an toan (tranh null exception)
        protected T GetValue<T>(DataRow row, string columnName, T defaultValue = default!)
        {
            try
            {
                if (row[columnName] == DBNull.Value)
                    return defaultValue;

                return (T)Convert.ChangeType(row[columnName], typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        // Lay gia tri nullable tu DataRow
        protected T? GetNullableValue<T>(DataRow row, string columnName) where T : struct
        {
            try
            {
                if (row[columnName] == DBNull.Value)
                    return null;

                return (T)Convert.ChangeType(row[columnName], typeof(T));
            }
            catch
            {
                return null;
            }
        }

        // Lay string tu DataRow (null-safe)
        protected string? GetString(DataRow row, string columnName)
        {
            try
            {
                if (row[columnName] == DBNull.Value)
                    return null;

                return row[columnName].ToString();
            }
            catch
            {
                return null;
            }
        }

        // Tao SqlParameter de tranh null
        protected SqlParameter CreateParameter(string name, object? value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        // Tao mang SqlParameters
        protected SqlParameter[] CreateParameters(params (string name, object? value)[] parameters)
        {
            var sqlParams = new SqlParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                sqlParams[i] = CreateParameter(parameters[i].name, parameters[i].value);
            }
            return sqlParams;
        }

        // ================== COMMON CRUD ==================

        // Execute query va tra ve single result (hoac null)
        protected DataRow? GetSingleRow(string query, SqlParameter[]? parameters = null)
        {
            var table = _db.ExecuteQuery(query, parameters);
            return table.Rows.Count > 0 ? table.Rows[0] : null;
        }

        // Execute query va tra ve multiple results
        protected DataRowCollection GetRows(string query, SqlParameter[]? parameters = null)
        {
            var table = _db.ExecuteQuery(query, parameters);
            return table.Rows;
        }

        // Insert va lay ID vua insert
        protected int InsertAndGetId(string query, SqlParameter[] parameters)
        {
            return _db.ExecuteInsertAndGetId(query, parameters);
        }

        // Update/Delete - tra ve so dong bi anh huong
        protected int Execute(string query, SqlParameter[] parameters)
        {
            return _db.ExecuteNonQuery(query, parameters);
        }

        // ================== TRANSACTION HELPERS ==================

        // Execute nhieu operations trong 1 transaction
        protected T ExecuteInTransaction<T>(Func<SqlConnection, SqlTransaction, T> action)
        {
            return _db.ExecuteInTransaction(action);
        }

        // Execute void action trong transaction
        protected void ExecuteInTransaction(Action<SqlConnection, SqlTransaction> action)
        {
            _db.ExecuteInTransaction<object?>((conn, trans) =>
            {
                action(conn, trans);
                return null;
            });
        }
    }
}
