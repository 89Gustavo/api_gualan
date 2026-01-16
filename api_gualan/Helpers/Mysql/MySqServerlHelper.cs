using MySqlConnector;
using System.Data;
using System.Data.Common;
using api_gualan.Helpers.Interfaces;

namespace api_gualan.Helpers.MySql
{
    public class MySqlServerHelper : IDbHelper
    {
        private readonly string _connectionString;

        public MySqlServerHelper(IConfiguration configuration)
        {
            var builder = new MySqlConnectionStringBuilder(
                configuration.GetConnectionString("MySqlConnection"))
            {
                AllowUserVariables = true,
                AllowLoadLocalInfile = true,
                DefaultCommandTimeout = 0
            };

            _connectionString = builder.ConnectionString;
        }

        private MySqlConnection GetConnection()
            => new MySqlConnection(_connectionString);

        // =====================================================
        // SQL NORMAL
        // =====================================================
        public async Task<DataTable> ExecuteQueryAsync(
            string sql, DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(sql, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        public async Task<int> ExecuteNonQueryAsync(
            string sql, DbParameter[] parameters = null, int commandTimeout = 30)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(sql, conn)
            {
                CommandTimeout = commandTimeout
            };

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(
            string sql, DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(sql, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }

        // =====================================================
        // PROCEDIMIENTOS
        // =====================================================
        public async Task<DataTable> ExecuteProcedureAsync(
            string procedureName, DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        public async Task<int> ExecuteProcedureNonQueryAsync(
            string procedureName, DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteProcedureScalarAsync(
            string procedureName, DbParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }

        public async Task<List<Dictionary<string, object>>> ExecuteProcedureJsonAsync(
            string procedureName, DbParameter[] parameters = null)
        {
            var table = await ExecuteProcedureAsync(procedureName, parameters);
            return DataTableToJson(table);
        }

        // =====================================================
        // PARAMETROS
        // =====================================================
        public DbParameter CreateParameter(
            string name,
            object value,
            DbType? dbType = null,
            ParameterDirection direction = ParameterDirection.Input)
        {
            var p = new MySqlParameter
            {
                ParameterName = name,
                Value = value ?? DBNull.Value,
                Direction = direction
            };

            if (dbType.HasValue)
                p.DbType = dbType.Value;

            return p;
        }

        private List<Dictionary<string, object>> DataTableToJson(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                    dict[col.ColumnName] = row[col];

                list.Add(dict);
            }

            return list;
        }
    }
}
