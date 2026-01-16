using api_gualan.Helpers.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace api_gualan.Services
{
    public class CsvBatchService
    {
        private readonly IDbHelper? _db;
        private readonly string _tipoConexion;

        public CsvBatchService(IDbHelper? db, IConfiguration configuration)
        {
            _db = db;
            // Valor por defecto para evitar null
            _tipoConexion = configuration["TipoConexion"] ?? "SqlServer";
        }

        public async Task ExecuteColocacionAsync(string filePath, string nombreOriginal, string usuarioBitacora)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Archivo CSV no encontrado: {filePath}");

            CsvValidator.ValidateDelimiter(filePath, '|');

            if (_db == null)
            {
                throw new Exception("DB Helper no disponible. Revise logs.");
            }

            if (_tipoConexion == "MySql")
            {
                await EjecutarMySqlAsync(filePath, nombreOriginal, usuarioBitacora);
            }
            else if (_tipoConexion == "SqlServer")
            {
                await EjecutarSqlServerAsync(filePath, nombreOriginal, usuarioBitacora);
            }
            else
            {
                throw new Exception($"TipoConexion no soportado: {_tipoConexion}");
            }
        }

        private async Task EjecutarMySqlAsync(string filePath, string nombreOriginal, string usuarioBitacora)
        {
            string safePath = filePath.Replace("\\", "/");
            string sql = $@"
USE exportablecsv;
ALTER TABLE DatosColocacion DISABLE KEYS;

LOAD DATA INFILE '{safePath}'
INTO TABLE DatosColocacion
CHARACTER SET utf8mb4
FIELDS TERMINATED BY '|'
ENCLOSED BY '""'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(
 EMPRESA, CLIENTE, NUMERODOCUMENTO, ARRANGEMENT_ID
 -- etc...
)
SET
 NombreArchivo = '{nombreOriginal}',
 Estatus = 1,
 UsuarioBitacora = '{usuarioBitacora}',
 Fecha_Creacion = NOW();

ALTER TABLE DatosColocacion ENABLE KEYS;
";

            await _db!.ExecuteNonQueryAsync(sql, null, 0);
        }

        private async Task EjecutarSqlServerAsync(string filePath, string nombreOriginal, string usuarioBitacora)
        {
            DbParameter[] parametros =
            {
                _db!.CreateParameter("@RutaArchivo", filePath),
                _db.CreateParameter("@NombreArchivo", nombreOriginal),
                _db.CreateParameter("@UsuarioBitacora", usuarioBitacora)
            };

            await _db.ExecuteProcedureNonQueryAsync("sp_carga_colocacion_bulk", parametros);
        }
    }
}
