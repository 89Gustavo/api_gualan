using System.Data;
using System.Data.Common;

namespace api_gualan.Helpers.Interfaces
{
    public interface IDbHelper
    {
        // SQL normal
        Task<DataTable> ExecuteQueryAsync(string sql, DbParameter[] parameters = null);
        Task<int> ExecuteNonQueryAsync(string sql, DbParameter[] parameters = null, int commandTimeout = 30);
        Task<object> ExecuteScalarAsync(string sql, DbParameter[] parameters = null);

        // Procedimientos
        Task<DataTable> ExecuteProcedureAsync(string procedureName, DbParameter[] parameters = null);
        Task<int> ExecuteProcedureNonQueryAsync(string procedureName, DbParameter[] parameters = null);
        Task<object> ExecuteProcedureScalarAsync(string procedureName, DbParameter[] parameters = null);

        // 🔥 JSON (EL QUE TE FALTABA)
        Task<List<Dictionary<string, object>>> ExecuteProcedureJsonAsync(
            string procedureName,
            DbParameter[] parameters = null
        );

        // Parámetros genéricos
        DbParameter CreateParameter(
            string name,
            object value,
            DbType? dbType = null,
            ParameterDirection direction = ParameterDirection.Input
        );
    }
}
