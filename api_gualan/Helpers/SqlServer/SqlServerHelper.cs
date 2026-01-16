using api_gualan.Helpers.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace api_gualan.Helpers.SqlServer
{
    public class SqlServerHelper : IDbHelper
    {
        private readonly string _connectionString;

        public SqlServerHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerConnection");
        }

        private SqlConnection GetConnection()
            => new SqlConnection(_connectionString);

        // =====================================================
        // SQL NORMAL
        // =====================================================

        public async Task<DataTable> ExecuteQueryAsync(
            string sql,
            DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn)
            {
                CommandTimeout = 0
            };

            AddParameters(cmd, parameters);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        public async Task<int> ExecuteNonQueryAsync(
            string sql,
            DbParameter[] parameters = null,
            int commandTimeout = 30)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn)
            {
                CommandTimeout = commandTimeout
            };

            AddParameters(cmd, parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(
            string sql,
            DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn)
            {
                CommandTimeout = 0
            };

            AddParameters(cmd, parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }

        // =====================================================
        // PROCEDIMIENTOS ALMACENADOS
        // =====================================================

        public async Task<DataTable> ExecuteProcedureAsync(
            string procedureName,
            DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = CreateProcedureCommand(procedureName, conn, parameters);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        public async Task<int> ExecuteProcedureNonQueryAsync(
            string procedureName,
            DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = CreateProcedureCommand(procedureName, conn, parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteProcedureScalarAsync(
            string procedureName,
            DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = CreateProcedureCommand(procedureName, conn, parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }

        public async Task<List<Dictionary<string, object>>> ExecuteProcedureJsonAsync(
            string procedureName,
            DbParameter[] parameters = null)
        {
            var table = await ExecuteProcedureAsync(procedureName, parameters);
            return DataTableToJson(table);
        }

        // =====================================================
        // CREACIÓN DE PARÁMETROS (🔥 CLAVE)
        // =====================================================

        public DbParameter CreateParameter(
            string name,
            object value,
            DbType? dbType = null,
            ParameterDirection direction = ParameterDirection.Input)
        {
            var param = new SqlParameter
            {
                ParameterName = name,
                Value = value ?? DBNull.Value,
                Direction = direction
            };

            if (dbType.HasValue)
                param.DbType = dbType.Value;

            return param;
        }

        // =====================================================
        // HELPERS PRIVADOS
        // =====================================================

        private SqlCommand CreateProcedureCommand(
            string procedureName,
            SqlConnection conn,
            DbParameter[] parameters)
        {
            var cmd = new SqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 0
            };

            AddParameters(cmd, parameters);
            return cmd;
        }

        private void AddParameters(SqlCommand cmd, DbParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return;

            foreach (SqlParameter p in parameters)
                cmd.Parameters.Add(p);
        }

        private List<Dictionary<string, object>> DataTableToJson(DataTable table)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                    dict[col.ColumnName] = row[col];

                result.Add(dict);
            }

            return result;
        }
    }
}
