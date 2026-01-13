using MySqlConnector;
using System.Data;

namespace api_gualan.Helpers
{
    public class MySqlHelper
    {
        private readonly string _connectionString;

        public MySqlHelper(IConfiguration configuration)
        {
            var baseConnectionString =
                configuration.GetConnectionString("MySqlConnection");

            var builder = new MySqlConnectionStringBuilder(baseConnectionString)
            {
                AllowUserVariables = true,       // 🔴 CLAVE para @EMPRESA
                AllowLoadLocalInfile = true,     // 🔴 LOAD DATA INFILE
                DefaultCommandTimeout = 0        // 🔴 cargas grandes
            };

            _connectionString = builder.ConnectionString;
        }


        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        // =====================================================
        // MÉTODO BASE PARA PROCEDIMIENTOS ALMACENADOS
        // =====================================================
        private MySqlCommand CreateStoredProcedureCommand(
            string procedureName,
            MySqlConnection conn,
            MySqlParameter[] parameters = null)
        {
            var cmd = new MySqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            return cmd;
        }

        // =====================================================
        // ===================== SQL NORMAL =====================
        // =====================================================

        // SELECT → DataTable
        public async Task<DataTable> ExecuteQueryAsync(
            string query,
            MySqlParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        // SELECT → JSON
        public async Task<List<Dictionary<string, object>>> ExecuteQueryJsonAsync(
            string query,
            MySqlParameter[] parameters = null)
        {
            var table = await ExecuteQueryAsync(query, parameters);
            return DataTableToJson(table);
        }

        // INSERT / UPDATE / DELETE
        public async Task<int> ExecuteNonQueryAsync(
            string query,
            MySqlParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // SCALAR
        public async Task<object> ExecuteScalarAsync(
            string query,
            MySqlParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }

        // =====================================================
        // ============ PROCEDIMIENTOS ALMACENADOS ==============
        // =====================================================

        // PROCEDURE → DataTable
        public async Task<DataTable> ExecuteProcedureAsync(
            string procedureName,
            MySqlParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = CreateStoredProcedureCommand(procedureName, conn, parameters);

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        // PROCEDURE → JSON
        public async Task<List<Dictionary<string, object>>> ExecuteProcedureJsonAsync(
            string procedureName,
            MySqlParameter[] parameters = null)
        {
            var table = await ExecuteProcedureAsync(procedureName, parameters);
            return DataTableToJson(table);
        }

        // PROCEDURE → INSERT / UPDATE / DELETE
        public async Task<int> ExecuteProcedureNonQueryAsync(
            string procedureName,
            MySqlParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = CreateStoredProcedureCommand(procedureName, conn, parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // PROCEDURE → SCALAR
        public async Task<object> ExecuteProcedureScalarAsync(
            string procedureName,
            MySqlParameter[] parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = CreateStoredProcedureCommand(procedureName, conn, parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }

        // =====================================================
        // UTILIDAD INTERNA → DataTable a JSON
        // =====================================================
        private List<Dictionary<string, object>> DataTableToJson(DataTable table)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }

                result.Add(dict);
            }

            return result;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, MySqlParameter[] parameters = null, int commandTimeout = 30)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.CommandTimeout = commandTimeout; // segundos
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task ExecuteNonQueryAsync(string sql)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }


    }
}
