using api_gualan.Helpers;

namespace api_gualan.Services
{
    public class CsvBatchService
    {
        private readonly MySqlHelper _db;

        public CsvBatchService(MySqlHelper db)
        {
            _db = db;
        }

        public async Task ExecuteColocacionAsync(
            string filePath,
            string nombreOriginal,
            string usuarioBitacora)
        {
            // 1️⃣ Validación rápida del CSV
            CsvValidator.ValidateDelimiter(filePath, '|');

            // 2️⃣ SQL LOAD DATA CORRECTO (SIN ...)
            string sql = $@"
USE exportablecsv;

ALTER TABLE DatosColocacion DISABLE KEYS;

LOAD DATA INFILE '{filePath.Replace("\\", "/")}'
INTO TABLE DatosColocacion
CHARACTER SET utf8mb4
FIELDS TERMINATED BY '|'
ENCLOSED BY '""'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(
 EMPRESA,
 CLIENTE,
 NUMERODOCUMENTO,
 ARRANGEMENT_ID,
 CATEGORIA,
 TIPODOCUMENTO,
 MONEDA,
 PRODUCTO,
 AREA_FINANCIERA,
 USUARIOASESOR,
 USUARIOCOBRANZA,
 USUARIOEJECUTIVO,
 RECORD_STATUS,
 DIASMORA,
 TASAINTERES,
 MONTODOCUMENTO,
 SALDOCAPITAL,
 CAPITALVIGENTE,
 CAPITALVENCIDO,
 INTERESESVENCIDOS,
 MONTO_MORA,
 MONTO_ADEUDADO,
 INTERESESDEVENGADOS,
 PLAZO,
 TIPOGARANTIA,
 fechainicio,
 fechavencimiento,
 frecuenciaCapital,
 frecuenciaInteres,
 ESTADODOCUMENTO,
 FECHAPROBACION,
 CALIFICACION_CREDITO,
 CLASIFICACIONPROYECTO,
 ORIGENFONDOS,
 COLDISPONIBLE,
 ACTIVIDADECONOMICA,
 CUOTACAPITAL,
 REFERENCIA,
 LTDEPURADO,
 DEPURADO,
 FECHAULTIMOPAGO,
 GENDER,
 MARITALSTATUS,
 CUENTADEBITO,
 CUENTACREDITO,
 DIASMORAINTERES,
 MONTOGASTOS,
 FECHAULTIMOPAGOCAPITAL,
 FECHAPROXIMOPAGOCAPITAL,
 TIPOFRECUENCIAPAGO,
 TIPO_PRODUCTO,
 FECHAREFINANCIA,
 FECHAREESTRUCTURA,
 monto_desembolsado,
 fecha_1erdesemb,
 fecha_ultdesemb,
 Usuario_Creacion_Cta,
 Categoria_Producto,
 Grupo,
 Numero_Acta,
 PAYIN,
 SEGPROMUTUA,
 Solicitud,
 Guarantor,
 Tasa_Mora
)
SET
 NombreArchivo = '{nombreOriginal}',
 Estatus = 1,
 UsuarioBitacora = '{usuarioBitacora}',
 Fecha_Creacion = NOW();

ALTER TABLE DatosColocacion ENABLE KEYS;
";

            // 3️⃣ Ejecutar sin timeout (batch grande)
            await _db.ExecuteNonQueryAsync(sql, null, 0);
        }
    }
}
