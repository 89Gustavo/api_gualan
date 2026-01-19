using api_gualan.Helpers;
using api_gualan.Helpers.Interfaces;
using api_gualan.Services;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using System.Data.Common;
using System.Diagnostics;


namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/csv")]
    public class CargaCsvController : ControllerBase
    {
        //private readonly Helpers.MySqlHelper _db;
        private readonly IDbHelper _db;
        private readonly IConfiguration _config;


        //public CargaCsvController(Helpers.MySqlHelper db, IConfiguration config)
       public CargaCsvController(IDbHelper db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            
            return Ok(_config["TipoConexion"]);
        }

        #region Captacion - fila por fila
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("cargarCaptacion")]
        public async Task<IActionResult> CargaCaptacion(IFormFile archivo, [FromQuery] string usuarioBitacora)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo inválido");

            string nombreArchivo = archivo.FileName;

            using var stream = archivo.OpenReadStream();

            int totalInsertados = 0;
            int totalErrores = 0;

            foreach (var fila in CaptacionHelper.LeerCsv(stream))
            {
                try
                {
                    var parameters = new[]
                    {
                        MySqlParameterHelper.Create("p_codigo_empresa", fila.codigo_empresa),
                        MySqlParameterHelper.Create("p_nombre_empresa", fila.nombre_empresa),
                        MySqlParameterHelper.Create("p_cliente", fila.cliente),
                        MySqlParameterHelper.Create("p_numero_cuenta", fila.numero_cuenta),
                        MySqlParameterHelper.Create("p_arrangement", fila.arrangement),
                        MySqlParameterHelper.Create("p_categoria", fila.categoria),
                        MySqlParameterHelper.Create("p_producto", fila.producto),
                        MySqlParameterHelper.Create("p_grupo_producto", fila.grupo_producto),
                        MySqlParameterHelper.Create("p_moneda", fila.moneda),
                        MySqlParameterHelper.Create("p_saldo", fila.saldo),
                        MySqlParameterHelper.Create("p_estado", fila.estado),
                        MySqlParameterHelper.Create("p_fecha_apertura", fila.fecha_apertura),
                        MySqlParameterHelper.Create("p_plazo", fila.plazo),
                        MySqlParameterHelper.Create("p_vencimiento", fila.vencimiento),
                        MySqlParameterHelper.Create("p_tasa", fila.tasa),
                        MySqlParameterHelper.Create("p_interes_acumulado", fila.interes_acumulado),
                        MySqlParameterHelper.Create("p_monto_reserva", fila.monto_reserva),
                        MySqlParameterHelper.Create("p_ejecutivo", fila.ejecutivo),
                        MySqlParameterHelper.Create("p_frecuencia_pago", fila.frecuencia_pago),
                        MySqlParameterHelper.Create("p_fecha_ultimo_credito", fila.fecha_ultimo_credito),
                        MySqlParameterHelper.Create("p_fecha_ultimo_debito", fila.fecha_ultimo_debito),
                        MySqlParameterHelper.Create("p_agencia", fila.agencia),
                        MySqlParameterHelper.Create("p_usuario_act", fila.usuario_act),
                        MySqlParameterHelper.Create("p_monto_disponible", fila.monto_disponible),
                        MySqlParameterHelper.Create("p_referencia", fila.referencia),
                        MySqlParameterHelper.Create("p_tipo_cliente", fila.tipo_cliente),
                        MySqlParameterHelper.Create("p_fecha_renovacion", fila.fecha_renovacion),
                        MySqlParameterHelper.Create("p_monto_apertura", fila.monto_apertura),
                        MySqlParameterHelper.Create("p_cuenta_dep_int", fila.cuenta_dep_int),
                        MySqlParameterHelper.Create("p_nombre_cuenta_depint", fila.nombre_cuenta_depint),
                        MySqlParameterHelper.Create("p_interes_x_pagar", fila.interes_x_pagar),
                        MySqlParameterHelper.Create("p_tarjeta_asociada", fila.tarjeta_asociada),
                        MySqlParameterHelper.Create("p_categoria_producto", fila.categoria_producto),
                        MySqlParameterHelper.Create("p_clasificacion", fila.clasificacion),
                        MySqlParameterHelper.Create("p_grupo", fila.grupo),
                        MySqlParameterHelper.Create("p_usuario_creacion_cta", fila.usuario_creacion_cta),
                        MySqlParameterHelper.Create("p_encaje", fila.encaje),
                        MySqlParameterHelper.Create("p_monto_pignorado", fila.monto_pignorado),
                        MySqlParameterHelper.Create("p_nombreArchivoCargado", nombreArchivo),
                        MySqlParameterHelper.Create("p_usuarioBitacora", usuarioBitacora)
                    };

                    await _db.ExecuteProcedureNonQueryAsync("sp_insert_datosCapatacion", parameters);

                    totalInsertados++;
                }
                catch
                {
                    totalErrores++;
                }
            }

            return Ok(new
            {
                success = totalErrores == 0,
                archivo = nombreArchivo,
                registrosInsertados = totalInsertados,
                registrosErrores = totalErrores
            });
        }
        #endregion

        #region Colocacion - fila por fila
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("cargarColocacion")]
        public async Task<IActionResult> CargarColocacion(IFormFile archivo, [FromQuery] string usuarioBitacora)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo inválido");

            string nombreArchivo = archivo.FileName;

            using var stream = archivo.OpenReadStream();

            int totalInsertados = 0;
            int totalErrores = 0;
            var errores = new List<string>();

            foreach (var fila in ColocacionHelper.LeerCsv(stream))
            {
                try
                {
                    var parameters = fila.GetType()
                        .GetProperties()
                        .Select(prop => MySqlParameterHelper.Create(
                            "p_" + ToSnakeCase(prop.Name),
                            prop.GetValue(fila) ?? (object)DBNull.Value
                        ))
                        .ToList();

                    parameters.Add(MySqlParameterHelper.Create("p_nombreArchivoCargado", nombreArchivo));
                    parameters.Add(MySqlParameterHelper.Create("p_usuarioBitacora", usuarioBitacora));

                    await _db.ExecuteProcedureNonQueryAsync("sp_insert_datoscolocacion", parameters.ToArray());

                    totalInsertados++;
                }
                catch (Exception ex)
                {
                    totalErrores++;
                    errores.Add($"Fila {fila.NumeroDocumento}: {ex.Message}");
                }
            }

            return Ok(new
            {
                success = totalErrores == 0,
                archivo = nombreArchivo,
                registrosInsertados = totalInsertados,
                registrosErrores = totalErrores,
                errores
            });
        }

        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var sb = new System.Text.StringBuilder();
            sb.Append(char.ToLower(input[0]));
            for (int i = 1; i < input.Length; i++)
            {
                var c = input[i];
                if (char.IsUpper(c) || char.IsDigit(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLower(c));
                }
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
        #endregion

        #region Colocacion - carga masiva rápida usando SourceStream

   
        [HttpPost("cargarColocacionMasiva")]
        public async Task<IActionResult> CargarColocacionBatchMasiva(
            IFormFile archivo,
            [FromQuery] string usuarioBitacora)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest("Archivo inválido");

                string uploadPath = @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads";

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string nombreOrignal =  archivo.FileName;
                //long pesoBytes = archivo.Length;

                //// Convertir a miligramos (1 byte = 0.001 mg)
                //double pesoMg = pesoBytes * 0.001;

                //// Opcional: redondear a 2 decimales
                //pesoMg = Math.Round(pesoMg, 2);


                long pesoBytes = archivo.Length;

                double pesoMg = pesoBytes / (1024.0 * 1024.0);

                // Redondear a 2 decimales
                pesoMg = Math.Round(pesoMg, 2);

                string nombreOriginal = archivo.FileName;

                string nombreArchivo =
                    $"{DateTime.Now:yyyy-MM-dd_HHmmss}_DatosColocacion.csv";

                string rutaCompleta = Path.Combine(uploadPath, nombreArchivo);

                // 1️⃣ Guardar archivo físico
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // 2️⃣ SQL BATCH
                string sql = $@"
                        USE exportablecsv;

                        ALTER TABLE DatosColocacion DISABLE KEYS;

                        LOAD DATA INFILE '{rutaCompleta.Replace("\\", "/")}'
                        INTO TABLE DatosColocacion
                        CHARACTER SET utf8mb4
                        FIELDS TERMINATED BY '|'
                        ENCLOSED BY '""'
                        LINES TERMINATED BY '\n'
                        IGNORE 1 ROWS
                        (
                         @EMPRESA,
                         @CLIENTE,
                         @NUMERODOCUMENTO,
                         @ARRANGEMENT_ID,
                         @CATEGORIA,
                         @TIPODOCUMENTO,
                         @MONEDA,
                         @PRODUCTO,
                         @AREA_FINANCIERA,
                         @USUARIOASESOR,
                         @USUARIOCOBRANZA,
                         @USUARIOEJECUTIVO,
                         @RECORD_STATUS,
                         @DIASMORA,
                         @TASAINTERES,
                         @MONTODOCUMENTO,
                         @SALDOCAPITAL,
                         @CAPITALVIGENTE,
                         @CAPITALVENCIDO,
                         @INTERESESVENCIDOS,
                         @MONTO_MORA,
                         @MONTO_ADEUDADO,
                         @INTERESESDEVENGADOS,
                         @PLAZO,
                         @TIPOGARANTIA,
                         @fechainicio,
                         @fechavencimiento,
                         @frecuenciaCapital,
                         @frecuenciaInteres,
                         @ESTADODOCUMENTO,
                         @FECHAPROBACION,
                         @CALIFICACION_CREDITO,
                         @CLASIFICACIONPROYECTO,
                         @ORIGENFONDOS,
                         @COLDISPONIBLE,
                         @ACTIVIDADECONOMICA,
                         @CUOTACAPITAL,
                         @REFERENCIA,
                         @LTDEPURADO,
                         @DEPURADO,
                         @FECHAULTIMOPAGO,
                         @GENDER,
                         @MARITALSTATUS,
                         @CUENTADEBITO,
                         @CUENTACREDITO,
                         @DIASMORAINTERES,
                         @MONTOGASTOS,
                         @FECHAULTIMOPAGOCAPITAL,
                         @FECHAPROXIMOPAGOCAPITAL,
                         @TIPOFRECUENCIAPAGO,
                         @TIPO_PRODUCTO,
                         @FECHAREFINANCIA,
                         @FECHAREESTRUCTURA,
                         @monto_desembolsado,
                         @fecha_1erdesemb,
                         @fecha_ultdesemb,
                         @Usuario_Creacion_Cta,
                         @Categoria_Producto,
                         @Grupo,
                         @Numero_Acta,
                         @PAYIN,
                         @SEGPROMUTUA,
                         @Solicitud,
                         @Guarantor,
                         @Tasa_Mora
                        )
                        SET
                         EMPRESA = REPLACE(@EMPRESA, '""', ''),
                         CLIENTE = REPLACE(@CLIENTE, '""', ''),
                         NUMERODOCUMENTO = REPLACE(@NUMERODOCUMENTO, '""', ''),
                         ARRANGEMENT_ID = REPLACE(@ARRANGEMENT_ID, '""', ''),
                         CATEGORIA = REPLACE(@CATEGORIA, '""', ''),
                         TIPODOCUMENTO = REPLACE(@TIPODOCUMENTO, '""', ''),
                         MONEDA = REPLACE(@MONEDA, '""', ''),
                         PRODUCTO = REPLACE(@PRODUCTO, '""', ''),
                         AREA_FINANCIERA = REPLACE(@AREA_FINANCIERA, '""', ''),
                         USUARIOASESOR = REPLACE(@USUARIOASESOR, '""', ''),
                         USUARIOCOBRANZA = REPLACE(@USUARIOCOBRANZA, '""', ''),
                         USUARIOEJECUTIVO = REPLACE(@USUARIOEJECUTIVO, '""', ''),
                         RECORD_STATUS = REPLACE(@RECORD_STATUS, '""', ''),
                         DIASMORA = REPLACE(@DIASMORA, '""', ''),
                         TASAINTERES = REPLACE(@TASAINTERES, '""', ''),
                         MONTODOCUMENTO = REPLACE(@MONTODOCUMENTO, '""', ''),
                         SALDOCAPITAL = REPLACE(@SALDOCAPITAL, '""', ''),
                         CAPITALVIGENTE = REPLACE(@CAPITALVIGENTE, '""', ''),
                         CAPITALVENCIDO = REPLACE(@CAPITALVENCIDO, '""', ''),
                         INTERESESVENCIDOS = REPLACE(@INTERESESVENCIDOS, '""', ''),
                         MONTO_MORA = REPLACE(@MONTO_MORA, '""', ''),
                         MONTO_ADEUDADO = REPLACE(@MONTO_ADEUDADO, '""', ''),
                         INTERESESDEVENGADOS = REPLACE(@INTERESESDEVENGADOS, '""', ''),
                         PLAZO = REPLACE(@PLAZO, '""', ''),
                         TIPOGARANTIA = REPLACE(@TIPOGARANTIA, '""', ''),
                         fechainicio = REPLACE(@fechainicio, '""', ''),
                         fechavencimiento = REPLACE(@fechavencimiento, '""', ''),
                         frecuenciaCapital = REPLACE(@frecuenciaCapital, '""', ''),
                         frecuenciaInteres = REPLACE(@frecuenciaInteres, '""', ''),
                         ESTADODOCUMENTO = REPLACE(@ESTADODOCUMENTO, '""', ''),
                         FECHAPROBACION = REPLACE(@FECHAPROBACION, '""', ''),
                         CALIFICACION_CREDITO = REPLACE(@CALIFICACION_CREDITO, '""', ''),
                         CLASIFICACIONPROYECTO = REPLACE(@CLASIFICACIONPROYECTO, '""', ''),
                         ORIGENFONDOS = REPLACE(@ORIGENFONDOS, '""', ''),
                         COLDISPONIBLE = REPLACE(@COLDISPONIBLE, '""', ''),
                         ACTIVIDADECONOMICA = REPLACE(@ACTIVIDADECONOMICA, '""', ''),
                         CUOTACAPITAL = REPLACE(@CUOTACAPITAL, '""', ''),
                         REFERENCIA = REPLACE(@REFERENCIA, '""', ''),
                         LTDEPURADO = REPLACE(@LTDEPURADO, '""', ''),
                         DEPURADO = REPLACE(@DEPURADO, '""', ''),
                         FECHAULTIMOPAGO = REPLACE(@FECHAULTIMOPAGO, '""', ''),
                         GENDER = REPLACE(@GENDER, '""', ''),
                         MARITALSTATUS = REPLACE(@MARITALSTATUS, '""', ''),
                         CUENTADEBITO = REPLACE(@CUENTADEBITO, '""', ''),
                         CUENTACREDITO = REPLACE(@CUENTACREDITO, '""', ''),
                         DIASMORAINTERES = REPLACE(@DIASMORAINTERES, '""', ''),
                         MONTOGASTOS = REPLACE(@MONTOGASTOS, '""', ''),
                         FECHAULTIMOPAGOCAPITAL = REPLACE(@FECHAULTIMOPAGOCAPITAL, '""', ''),
                         FECHAPROXIMOPAGOCAPITAL = REPLACE(@FECHAPROXIMOPAGOCAPITAL, '""', ''),
                         TIPOFRECUENCIAPAGO = REPLACE(@TIPOFRECUENCIAPAGO, '""', ''),
                         TIPO_PRODUCTO = CONVERT(IFNULL(REPLACE(@TIPO_PRODUCTO, '""', ''), NULL) USING utf8mb4),
                         FECHAREFINANCIA = REPLACE(@FECHAREFINANCIA, '""', ''),
                         FECHAREESTRUCTURA = REPLACE(@FECHAREESTRUCTURA, '""', ''),
                         monto_desembolsado = REPLACE(@monto_desembolsado, '""', ''),
                         fecha_1erdesemb = REPLACE(@fecha_1erdesemb, '""', ''),
                         fecha_ultdesemb = REPLACE(@fecha_ultdesemb, '""', ''),
                         Usuario_Creacion_Cta = REPLACE(@Usuario_Creacion_Cta, '""', ''),
                         Categoria_Producto = REPLACE(@Categoria_Producto, '""', ''),
                         Grupo = REPLACE(@Grupo, '""', ''),
                         Numero_Acta = REPLACE(@Numero_Acta, '""', ''),
                         PAYIN = REPLACE(@PAYIN, '""', ''),
                         SEGPROMUTUA = REPLACE(@SEGPROMUTUA, '""', ''),
                         Solicitud = REPLACE(@Solicitud, '""', ''),
                         Guarantor = REPLACE(@Guarantor, '""', ''),
                         Tasa_Mora = REPLACE(@Tasa_Mora, '""', ''),
                         nombreArchivoCargado = '{nombreOrignal}',
                         Estatus = 1,
                         UsuarioBitacora = '{usuarioBitacora}',
                         Fecha_Creacion = NOW();

                        ALTER TABLE DatosColocacion ENABLE KEYS;
                        ";

                await _db.ExecuteNonQueryAsync(sql);

                stopwatch.Stop();

                return Ok(new
                {
                    success = true,
                    archivo = nombreOriginal,
                    tipoCarga = "BATCH",
                    tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                    peso= $"{pesoMg} mb",
                    mensaje = "Carga masiva ejecutada correctamente"
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    detalle = ex.InnerException?.Message,
                    tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2)
                });
            }
        }




        #endregion


        #region Captaciones - carga masiva rápida usando SourceStream


        [HttpPost("cargarCaptacionesMasiva")]
        public async Task<IActionResult> CargarCaptacionesBatchMasiva(
            IFormFile archivo,
            [FromQuery] string usuarioBitacora)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest("Archivo inválido");

                string uploadPath = @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads";

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string nombreOrignal = archivo.FileName;
                //long pesoBytes = archivo.Length;

                //// Convertir a miligramos (1 byte = 0.001 mg)
                //double pesoMg = pesoBytes * 0.001;

                //// Opcional: redondear a 2 decimales
                //pesoMg = Math.Round(pesoMg, 2);


                long pesoBytes = archivo.Length;

                double pesoMg = pesoBytes / (1024.0 * 1024.0);

                // Redondear a 2 decimales
                pesoMg = Math.Round(pesoMg, 2);
                string nombreOriginal = archivo.FileName;

                string nombreArchivo =
                    $"{DateTime.Now:yyyy-MM-dd_HHmmss}_DatosCaptaciones.csv";

                string rutaCompleta = Path.Combine(uploadPath, nombreArchivo);

                // 1️⃣ Guardar archivo físico
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // 2️⃣ SQL BATCH
                string sql = $@"
                USE exportablecsv;

                ALTER TABLE datoscaptacion DISABLE KEYS;

                LOAD DATA INFILE '{rutaCompleta.Replace("\\", "/")}'
                INTO TABLE datoscaptacion
                CHARACTER SET latin1
                FIELDS TERMINATED BY '|'
                LINES TERMINATED BY '\n'
                IGNORE 1 ROWS
                (
                 @CodigoEmpresa,
                 @NombreEmpresa,
                 @Cliente,
                 @NumeroCuenta,
                 @Arrangement,
                 @Categoria,
                 @Producto,
                 @GrupoProducto,
                 @Moneda,
                 @Saldo,
                 @Estado,
                 @FechaApertura,
                 @Plazo,
                 @Vencimiento,
                 @Tasa,
                 @InteresAcumulado,
                 @MontoReserva,
                 @Ejecutivo,
                 @FrecuenciaPago,
                 @FechaUltimoCredito,
                 @FechaUltimoDebito,
                 @Agencia,
                 @UsuarioAct,
                 @MontoDisponible,
                 @Referencia,
                 @TipoCliente,
                 @FechaRenovacion,
                 @MontoApertura,
                 @CuentaDepInt,
                 @NombreCuentaDepInt,
                 @InteresXPagar,
                 @TarjetaAsociada,
                 @CategoriaProducto,
                 @Clasificacion,
                 @Grupo,
                 @UsuarioCreacionCta,
                 @Encaje,
                 @MontoPignorado
                )
                SET
                 codigo_empresa        = TRIM(@CodigoEmpresa),
                 nombre_empresa        = TRIM(@NombreEmpresa),
                 cliente              = TRIM(@Cliente),
                 numero_cuenta         = TRIM(@NumeroCuenta),
                 arrangement          = TRIM(@Arrangement),
                 categoria             = TRIM(@Categoria),
                 producto              = TRIM(@Producto),
                 grupo_producto         = TRIM(@GrupoProducto),
                 moneda                = TRIM(@Moneda),
                 saldo                 = NULLIF(@Saldo,''),
                 estado                = TRIM(@Estado),
                 fecha_apertura         = NULLIF(@FechaApertura,''),
                 plazo                 = NULLIF(@Plazo,''),
                 vencimiento           = NULLIF(@Vencimiento,''),
                 tasa                  = NULLIF(@Tasa,''),
                 interes_acumulado      = NULLIF(@InteresAcumulado,''),
                 monto_reserva          = NULLIF(@MontoReserva,''),
                 ejecutivo             = TRIM(@Ejecutivo),
                 frecuencia_pago        = TRIM(@FrecuenciaPago),
                 fecha_ultimo_credito    = NULLIF(@FechaUltimoCredito,''),
                 fecha_ultimo_debito     = NULLIF(@FechaUltimoDebito,''),
                 agencia               = TRIM(@Agencia),
                 usuario_act            = TRIM(@UsuarioAct),
                 monto_disponible       = NULLIF(@MontoDisponible,''),
                 referencia            = TRIM(@Referencia),
                 tipo_cliente           = TRIM(@TipoCliente),
                 fecha_renovacion       = NULLIF(@FechaRenovacion,''),
                 monto_apertura         = NULLIF(@MontoApertura,''),
                 cuenta_dep_int          = TRIM(@CuentaDepInt),
                 nombre_cuenta_depint    = TRIM(@NombreCuentaDepInt),
                 interes_x_pagar         = NULLIF(@InteresXPagar,''),
                 tarjeta_asociada       = TRIM(@TarjetaAsociada),
                 categoria_producto     = TRIM(@CategoriaProducto),
                 clasificacion         = TRIM(@Clasificacion),
                 Grupo                 =CONVERT(TRIM(@Grupo) USING utf8mb4) ,
                 usuario_creacion_cta    = TRIM(@UsuarioCreacionCta),
                 encaje                = NULLIF(@Encaje,''),
                 monto_pignorado        = NULLIF(@MontoPignorado,''),
                 nombreArchivoCargado         = '{nombreOrignal}',
                 estatus               = 1,
                 usuarioCreacion       = '{usuarioBitacora}',
                 fechaCreacion        = NOW();

                ALTER TABLE datoscaptacion ENABLE KEYS;
                ";

                await _db.ExecuteNonQueryAsync(sql);

                stopwatch.Stop();

                return Ok(new
                {
                    success = true,
                    archivo = nombreOriginal,
                    tipoCarga = "BATCH",
                    tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                    peso = $"{pesoMg} mb",
                    mensaje = "Carga masiva ejecutada correctamente"
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    detalle = ex.InnerException?.Message,
                    tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2)
                });
            }
        }




        #endregion

        #region Clientes - carga masiva rápida usando SourceStream
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("cargarClientesMasivo")]
        public async Task<IActionResult> CargarClientesMasivo(IFormFile archivo,[FromQuery] string usuarioBitacora)
        {
            var stopwatch = Stopwatch.StartNew();
            if (_config["TipoConexion"] == "MySql")
            {
                try
                {

                    if (archivo == null || archivo.Length == 0)
                        return BadRequest("Archivo inválido");

                    string uploadPath = @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads";

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    string nombreOriginal = archivo.FileName;
                    long pesoBytes = archivo.Length;
                    double pesoMb = Math.Round(pesoBytes / (1024.0 * 1024.0), 2);

                    string nombreArchivo =
                        $"{DateTime.Now:yyyy-MM-dd_HHmmss}_DatosClientes.csv";

                    string rutaCompleta = Path.Combine(uploadPath, nombreArchivo);

                    // 1️⃣ Guardar archivo
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    string sql = $@"
                USE exportablecsv;

                ALTER TABLE datosClientes DISABLE KEYS;

                LOAD DATA INFILE '{rutaCompleta.Replace("\\", "/")}'
                    INTO TABLE datosClientes
                    CHARACTER SET latin1
                    FIELDS TERMINATED BY '|'
                    LINES TERMINATED BY '\n'
                    IGNORE 1 LINES
                    (
                        @CodigoCliente,@Actualizacion,@Nombre1,@Nombre2,@Nombre3,@Apellido1,@Apellido2,@ApellidoCasada,
                        @Celular,@Telefono,@Genero,@FechaApertura,@TipoCliente,@FechaNacimiento,@ActividadEconomicaIVE,@AltoRiesgo,
                        @IdReple,@Dpi,@Pasaporte,@Licencia,@Nit,@Departamento,@Municipio,@Pais,@EstadoCivil,@DeptoNacimiento,
                        @MuniNacimiento,@PaisNacimiento,@Nacionalidad,@Ocupacion,@Profesion,@CorreoElectronico,@ActividadEconomica,
                        @Rubro,@SubRubro,@Direccion,@ZonaDomicilio,@PaisDomicilio,@DeptoDomicilio,@MuniDomicilio,@RelacionDependencia,
                        @NombreRelacionDependencia,@IngresosLaborales,@MonedaIngresoLaboral,@FechaIngresoLaboral,@NegocioPropio,
                        @NombreNegocio,@FechaInicioNegocio,@IngresosNegocioPropio,@MonedaNegocioPropio,@IngresosRemesas,
                        @MontoOtrosIngresos,@OtrosIngresos,@MontoIngresos,@MonedaIngresos,@RangoIngresos,@MontoEgresos,@MonedaEgresos,
                        @RangoEgresos,@ActEconomicaRelacionDependencia,@ActEconomicaNegocio,@Edad,@Cooperativa,@CondicionVivienda,
                        @Puesto,@DireccionLaboral,@ZonaLaboral,@DeptoLaboral,@MuniLaboral,@TelefonoLaboral,@PersonaPEP,@PersonaCPE,
                        @Categoria,@DescripcionSubRubro,@DescripcionRubro,@CoopeCreacion,@parentesco_pep,@relacion_pep,@codigo_cli_encargado,
                        @ActEconRedep,@ActEconNego,@Depto_Domi,@Muni_Domi,@OTR_ING_ACTPROF,@OTR_ING_ACTPROF_DES,@OTR_ING_MANU,
                        @OTR_ING_MANU_DES,@OTR_ING_RENTAS,@OTR_ING_RENTAS_DES,@OTR_ING_JUBILA,@OTR_ING_JUBILA_DES,@OTR_ING_OTRFUE,
                        @OTR_ING_OTRFUE_DES,@CampoSector,@dir_neg_propio,@zona_neg_propio,@pais_neg_propio,@depto_neg_propio,
                        @ciudad_neg_propio,@fecha_expdpi,@fecha_emidpi,@UsuarioActualizacion,@CooperativaActualizacion,@IdRepresentante,
                        @NombreRepresentante,@RelacionRepresentante,@CanalCliente
                    )
                    SET
   
                    CodigoCliente = NULLIF(NULLIF(TRIM(@CodigoCliente),''),'NULL'),
                    Actualizacion = NULLIF(NULLIF(TRIM(@Actualizacion),''),'NULL'),
                    Nombre1 = NULLIF(NULLIF(TRIM(@Nombre1),''),'NULL'),
                    Nombre2 = NULLIF(NULLIF(TRIM(@Nombre2),''),'NULL'),
                    Nombre3 = NULLIF(NULLIF(TRIM(@Nombre3),''),'NULL'),
                    Apellido1 = NULLIF(NULLIF(TRIM(@Apellido1),''),'NULL'),
                    Apellido2 = NULLIF(NULLIF(TRIM(@Apellido2),''),'NULL'),
                    ApellidoCasada = NULLIF(NULLIF(TRIM(@ApellidoCasada),''),'NULL'),
                    Celular = NULLIF(NULLIF(TRIM(@Celular),''),'NULL'),
                    Telefono = NULLIF(NULLIF(TRIM(@Telefono),''),'NULL'),
                    Genero = NULLIF(NULLIF(TRIM(@Genero),''),'NULL'),
                    FechaApertura = NULLIF(NULLIF(TRIM(@FechaApertura),''),'NULL'),
                    TipoCliente = NULLIF(NULLIF(TRIM(@TipoCliente),''),'NULL'),
                    FechaNacimiento = NULLIF(NULLIF(TRIM(@FechaNacimiento),''),'NULL'),
                    ActividadEconomicaIVE = NULLIF(NULLIF(TRIM(@ActividadEconomicaIVE),''),'NULL'),
                    AltoRiesgo = NULLIF(NULLIF(TRIM(@AltoRiesgo),''),'NULL'),
                    IdReple = NULLIF(NULLIF(TRIM(@IdReple),''),'NULL'),
                    Dpi = NULLIF(NULLIF(TRIM(@Dpi),''),'NULL'),
                    Pasaporte = NULLIF(NULLIF(TRIM(@Pasaporte),''),'NULL'),
                    Licencia = NULLIF(NULLIF(TRIM(@Licencia),''),'NULL'),
                    Nit = NULLIF(NULLIF(TRIM(@Nit),''),'NULL'),
                    Departamento = NULLIF(NULLIF(TRIM(@Departamento),''),'NULL'),
                    Municipio = NULLIF(NULLIF(TRIM(@Municipio),''),'NULL'),
                    Pais = NULLIF(NULLIF(TRIM(@Pais),''),'NULL'),
                    EstadoCivil = NULLIF(NULLIF(TRIM(@EstadoCivil),''),'NULL'),
                    DeptoNacimiento = NULLIF(NULLIF(TRIM(@DeptoNacimiento),''),'NULL'),
                    MuniNacimiento = NULLIF(NULLIF(TRIM(@MuniNacimiento),''),'NULL'),
                    PaisNacimiento = NULLIF(NULLIF(TRIM(@PaisNacimiento),''),'NULL'),
                    Nacionalidad = NULLIF(NULLIF(TRIM(@Nacionalidad),''),'NULL'),
                    Ocupacion = NULLIF(NULLIF(TRIM(@Ocupacion),''),'NULL'),
                    Profesion = NULLIF(NULLIF(TRIM(@Profesion),''),'NULL'),
                    CorreoElectronico = NULLIF(NULLIF(TRIM(@CorreoElectronico),''),'NULL'),
                    ActividadEconomica = NULLIF(NULLIF(TRIM(@ActividadEconomica),''),'NULL'),
                    Rubro = NULLIF(NULLIF(TRIM(@Rubro),''),'NULL'),
                    SubRubro = NULLIF(NULLIF(TRIM(@SubRubro),''),'NULL'),
                    Direccion = NULLIF(NULLIF(TRIM(@Direccion),''),'NULL'),
                    ZonaDomicilio = NULLIF(NULLIF(TRIM(@ZonaDomicilio),''),'NULL'),
                    PaisDomicilio = NULLIF(NULLIF(TRIM(@PaisDomicilio),''),'NULL'),
                    DeptoDomicilio = NULLIF(NULLIF(TRIM(@DeptoDomicilio),''),'NULL'),
                    MuniDomicilio = NULLIF(NULLIF(TRIM(@MuniDomicilio),''),'NULL'),
                    RelacionDependencia = NULLIF(NULLIF(TRIM(@RelacionDependencia),''),'NULL'),
                    NombreRelacionDependencia = NULLIF(NULLIF(TRIM(@NombreRelacionDependencia),''),'NULL'),
                    IngresosLaborales = NULLIF(NULLIF(TRIM(@IngresosLaborales),''),'NULL'),
                    MonedaIngresoLaboral = NULLIF(NULLIF(TRIM(@MonedaIngresoLaboral),''),'NULL'),
                    FechaIngresoLaboral = NULLIF(NULLIF(TRIM(@FechaIngresoLaboral),''),'NULL'),
                    NegocioPropio = NULLIF(NULLIF(TRIM(@NegocioPropio),''),'NULL'),
                    NombreNegocio = NULLIF(NULLIF(TRIM(@NombreNegocio),''),'NULL'),
                    FechaInicioNegocio = NULLIF(NULLIF(TRIM(@FechaInicioNegocio),''),'NULL'),
                    IngresosNegocioPropio = NULLIF(NULLIF(TRIM(@IngresosNegocioPropio),''),'NULL'),
                    MonedaNegocioPropio = NULLIF(NULLIF(TRIM(@MonedaNegocioPropio),''),'NULL'),
                    IngresosRemesas = NULLIF(NULLIF(TRIM(@IngresosRemesas),''),'NULL'),
                    MontoOtrosIngresos = NULLIF(NULLIF(TRIM(@MontoOtrosIngresos),''),'NULL'),
                    OtrosIngresos = NULLIF(NULLIF(TRIM(@OtrosIngresos),''),'NULL'),
                    MontoIngresos = NULLIF(NULLIF(TRIM(@MontoIngresos),''),'NULL'),
                    MonedaIngresos = NULLIF(NULLIF(TRIM(@MonedaIngresos),''),'NULL'),
                    RangoIngresos = NULLIF(NULLIF(TRIM(@RangoIngresos),''),'NULL'),
                    MontoEgresos = NULLIF(NULLIF(TRIM(@MontoEgresos),''),'NULL'),
                    MonedaEgresos = NULLIF(NULLIF(TRIM(@MonedaEgresos),''),'NULL'),
                    RangoEgresos = NULLIF(NULLIF(TRIM(@RangoEgresos),''),'NULL'),
                    ActEconomicaRelacionDependencia = NULLIF(NULLIF(TRIM(@ActEconomicaRelacionDependencia),''),'NULL'),
                    ActEconomicaNegocio = NULLIF(NULLIF(TRIM(@ActEconomicaNegocio),''),'NULL'),
                    Edad = NULLIF(NULLIF(TRIM(@Edad),''),'NULL'),
                    Cooperativa = NULLIF(NULLIF(TRIM(@Cooperativa),''),'NULL'),
                    CondicionVivienda = NULLIF(NULLIF(TRIM(@CondicionVivienda),''),'NULL'),
                    Puesto = NULLIF(NULLIF(TRIM(@Puesto),''),'NULL'),
                    DireccionLaboral = NULLIF(NULLIF(TRIM(@DireccionLaboral),''),'NULL'),
                    ZonaLaboral = NULLIF(NULLIF(TRIM(@ZonaLaboral),''),'NULL'),
                    DeptoLaboral = NULLIF(NULLIF(TRIM(@DeptoLaboral),''),'NULL'),
                    MuniLaboral = NULLIF(NULLIF(TRIM(@MuniLaboral),''),'NULL'),
                    TelefonoLaboral = NULLIF(NULLIF(TRIM(@TelefonoLaboral),''),'NULL'),
                    PersonaPEP = NULLIF(NULLIF(TRIM(@PersonaPEP),''),'NULL'),
                    PersonaCPE = NULLIF(NULLIF(TRIM(@PersonaCPE),''),'NULL'),
                    Categoria = NULLIF(NULLIF(TRIM(@Categoria),''),'NULL'),
                    DescripcionSubRubro = NULLIF(NULLIF(TRIM(@DescripcionSubRubro),''),'NULL'),
                    DescripcionRubro = NULLIF(NULLIF(TRIM(@DescripcionRubro),''),'NULL'),
                    CoopeCreacion = NULLIF(NULLIF(TRIM(@CoopeCreacion),''),'NULL'),
                    parentesco_pep = NULLIF(NULLIF(TRIM(@parentesco_pep),''),'NULL'),
                    relacion_pep = NULLIF(NULLIF(TRIM(@relacion_pep),''),'NULL'),
                    codigo_cli_encargado = NULLIF(NULLIF(TRIM(@codigo_cli_encargado),''),'NULL'),
                    ActEconRedep = NULLIF(NULLIF(TRIM(@ActEconRedep),''),'NULL'),
                    ActEconNego = NULLIF(NULLIF(TRIM(@ActEconNego),''),'NULL'),
                    Depto_Domi = NULLIF(NULLIF(TRIM(@Depto_Domi),''),'NULL'),
                    Muni_Domi = NULLIF(NULLIF(TRIM(@Muni_Domi),''),'NULL'),
                    OTR_ING_ACTPROF = NULLIF(NULLIF(TRIM(@OTR_ING_ACTPROF),''),'NULL'),
                    OTR_ING_ACTPROF_DES = NULLIF(NULLIF(TRIM(@OTR_ING_ACTPROF_DES),''),'NULL'),
                    OTR_ING_MANU = NULLIF(NULLIF(TRIM(@OTR_ING_MANU),''),'NULL'),
                    OTR_ING_MANU_DES = NULLIF(NULLIF(TRIM(@OTR_ING_MANU_DES),''),'NULL'),
                    OTR_ING_RENTAS = NULLIF(NULLIF(TRIM(@OTR_ING_RENTAS),''),'NULL'),
                    OTR_ING_RENTAS_DES = NULLIF(NULLIF(TRIM(@OTR_ING_RENTAS_DES),''),'NULL'),
                    OTR_ING_JUBILA = NULLIF(NULLIF(TRIM(@OTR_ING_JUBILA),''),'NULL'),
                    OTR_ING_JUBILA_DES = NULLIF(NULLIF(TRIM(@OTR_ING_JUBILA_DES),''),'NULL'),
                    OTR_ING_OTRFUE = NULLIF(NULLIF(TRIM(@OTR_ING_OTRFUE),''),'NULL'),
                    OTR_ING_OTRFUE_DES = NULLIF(NULLIF(TRIM(@OTR_ING_OTRFUE_DES),''),'NULL'),
                    CampoSector = NULLIF(NULLIF(TRIM(@CampoSector),''),'NULL'),
                    dir_neg_propio = NULLIF(NULLIF(TRIM(@dir_neg_propio),''),'NULL'),
                    zona_neg_propio = NULLIF(NULLIF(TRIM(@zona_neg_propio),''),'NULL'),
                    pais_neg_propio = NULLIF(NULLIF(TRIM(@pais_neg_propio),''),'NULL'),
                    depto_neg_propio = NULLIF(NULLIF(TRIM(@depto_neg_propio),''),'NULL'),
                    ciudad_neg_propio = NULLIF(NULLIF(TRIM(@ciudad_neg_propio),''),'NULL'),
                    fecha_expdpi = NULLIF(NULLIF(TRIM(@fecha_expdpi),''),'NULL'),
                    fecha_emidpi = NULLIF(NULLIF(TRIM(@fecha_emidpi),''),'NULL'),
                    UsuarioActualizacion = NULLIF(NULLIF(TRIM(@UsuarioActualizacion),''),'NULL'),
                    CooperativaActualizacion = NULLIF(NULLIF(TRIM(@CooperativaActualizacion),''),'NULL'),
                    IdRepresentante = NULLIF(NULLIF(TRIM(@IdRepresentante),''),'NULL'),
                    NombreRepresentante = NULLIF(NULLIF(TRIM(@NombreRepresentante),''),'NULL'),
                    RelacionRepresentante = NULLIF(NULLIF(TRIM(@RelacionRepresentante),''),'NULL'),
                    CanalCliente = NULLIF(NULLIF(TRIM(@CanalCliente),''),'NULL'),
                    estatus = 1,
                    usuarioCreacion = '{usuarioBitacora}',
                    fechaCreacion = NOW();



                ALTER TABLE datosClientes ENABLE KEYS;
                ";

                    await _db.ExecuteNonQueryAsync(sql);

                    stopwatch.Stop();

                    return Ok(new
                    {
                        success = true,
                        archivo = nombreOriginal,
                        tipoCarga = "BATCH",
                        tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                        peso = $"{pesoMb} mb",
                        mensaje = "Carga masiva de clientes ejecutada correctamente"
                    });
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    return StatusCode(500, new
                    {
                        success = false,
                        error = ex.Message,
                        detalle = ex.InnerException?.Message,
                        tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2)
                    });
                }

            }
            else if (_config["TipoConexion"] == "SqlServer")
            {

                return Ok(_config["TipoConexion"]);
            }
            else {
                return Ok("conexion no conectado");
            }

        }

        #endregion



        #region #region Clientes - carga masiva rápida usando SourceStream limpio
        [HttpPost("cargarClientesMasivoLimpio")]
        public async Task<IActionResult> CargarClientesMasivoLimpio(IFormFile archivo,[FromQuery] string usuarioBitacora)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo inválido");
            var stopwatch = Stopwatch.StartNew();

            if (_config["TipoConexion"] == "MySql")
            {
                try
                {
                   

                    string uploadPath = @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads";
                    Directory.CreateDirectory(uploadPath);

                    string nombreArchivo = $"{DateTime.Now:yyyy-MM-dd_HHmmss}_DatosClientes.csv";
                    string rutaOriginal = Path.Combine(uploadPath, nombreArchivo);
                    string rutaLimpia = Path.Combine(uploadPath, $"LIMPIO_{nombreArchivo}");

                    string nombreOriginal = archivo.FileName;

                    long pesoBytes = archivo.Length;
                    double pesoMb = Math.Round(pesoBytes / (1024.0 * 1024.0), 2);

                    // 1️⃣ Guardar CSV original
                    using (var stream = new FileStream(rutaOriginal, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    // 2️⃣ LIMPIAR CSV
                    Helpers.LimpiarCsvColumnas.Ejecutar(
                        rutaOriginal: rutaOriginal,
                        rutaLimpia: rutaLimpia
                    );

                    // 3️⃣ USAR CSV LIMPIO
                    string rutaMysql = rutaLimpia.Replace("\\", "/");

                    string sql = $@"
                    USE exportablecsv;

                    ALTER TABLE datosClientes DISABLE KEYS;

                    LOAD DATA INFILE '{rutaMysql}'
                    INTO TABLE datosClientes
                    CHARACTER SET latin1
                    FIELDS TERMINATED BY '|'
                    LINES TERMINATED BY '\n'
                    IGNORE 1 LINES
                    (
                        @CodigoCliente,@Actualizacion,@Nombre1,@Nombre2,@Nombre3,@Apellido1,@Apellido2,@ApellidoCasada,
                        @Celular,@Telefono,@Genero,@FechaApertura,@TipoCliente,@FechaNacimiento,@ActividadEconomicaIVE,@AltoRiesgo,
                        @IdReple,@Dpi,@Pasaporte,@Licencia,@Nit,@Departamento,@Municipio,@Pais,@EstadoCivil,@DeptoNacimiento,
                        @MuniNacimiento,@PaisNacimiento,@Nacionalidad,@Ocupacion,@Profesion,@CorreoElectronico,@ActividadEconomica,
                        @Rubro,@SubRubro,@Direccion,@ZonaDomicilio,@PaisDomicilio,@DeptoDomicilio,@MuniDomicilio,@RelacionDependencia,
                        @NombreRelacionDependencia,@IngresosLaborales,@MonedaIngresoLaboral,@FechaIngresoLaboral,@NegocioPropio,
                        @NombreNegocio,@FechaInicioNegocio,@IngresosNegocioPropio,@MonedaNegocioPropio,@IngresosRemesas,
                        @MontoOtrosIngresos,@OtrosIngresos,@MontoIngresos,@MonedaIngresos,@RangoIngresos,@MontoEgresos,@MonedaEgresos,
                        @RangoEgresos,@ActEconomicaRelacionDependencia,@ActEconomicaNegocio,@Edad,@Cooperativa,@CondicionVivienda,
                        @Puesto,@DireccionLaboral,@ZonaLaboral,@DeptoLaboral,@MuniLaboral,@TelefonoLaboral,@PersonaPEP,@PersonaCPE,
                        @Categoria,@DescripcionSubRubro,@DescripcionRubro,@CoopeCreacion,@parentesco_pep,@relacion_pep,
                        @codigo_cli_encargado,@ActEconRedep,@ActEconNego,@Depto_Domi,@Muni_Domi,
                        @OTR_ING_ACTPROF,@OTR_ING_ACTPROF_DES,@OTR_ING_MANU,@OTR_ING_MANU_DES,
                        @OTR_ING_RENTAS,@OTR_ING_RENTAS_DES,@OTR_ING_JUBILA,@OTR_ING_JUBILA_DES,
                        @OTR_ING_OTRFUE,@OTR_ING_OTRFUE_DES,@CampoSector,@dir_neg_propio,@zona_neg_propio,
                        @pais_neg_propio,@depto_neg_propio,@ciudad_neg_propio,@fecha_expdpi,@fecha_emidpi,
                        @UsuarioActualizacion,@CooperativaActualizacion,@IdRepresentante,@NombreRepresentante,
                        @RelacionRepresentante,@CanalCliente
                    )
                    SET
                       CodigoCliente = NULLIF(NULLIF(TRIM(@CodigoCliente),''),'NULL'),
                        Actualizacion = NULLIF(NULLIF(TRIM(@Actualizacion),''),'NULL'),
                        Nombre1 = NULLIF(NULLIF(TRIM(@Nombre1),''),'NULL'),
                        Nombre2 = NULLIF(NULLIF(TRIM(@Nombre2),''),'NULL'),
                        Nombre3 = NULLIF(NULLIF(TRIM(@Nombre3),''),'NULL'),
                        Apellido1 = NULLIF(NULLIF(TRIM(@Apellido1),''),'NULL'),
                        Apellido2 = NULLIF(NULLIF(TRIM(@Apellido2),''),'NULL'),
                        ApellidoCasada = NULLIF(NULLIF(TRIM(@ApellidoCasada),''),'NULL'),
                        Celular = NULLIF(NULLIF(TRIM(@Celular),''),'NULL'),
                        Telefono = NULLIF(NULLIF(TRIM(@Telefono),''),'NULL'),
                        Genero = NULLIF(NULLIF(TRIM(@Genero),''),'NULL'),
                        FechaApertura = NULLIF(NULLIF(TRIM(@FechaApertura),''),'NULL'),
                        TipoCliente = NULLIF(NULLIF(TRIM(@TipoCliente),''),'NULL'),
                        FechaNacimiento = NULLIF(NULLIF(TRIM(@FechaNacimiento),''),'NULL'),
                        ActividadEconomicaIVE = NULLIF(NULLIF(TRIM(@ActividadEconomicaIVE),''),'NULL'),
                        AltoRiesgo = NULLIF(NULLIF(TRIM(@AltoRiesgo),''),'NULL'),
                        IdReple = NULLIF(NULLIF(TRIM(@IdReple),''),'NULL'),
                        Dpi = NULLIF(NULLIF(TRIM(@Dpi),''),'NULL'),
                        Pasaporte = NULLIF(NULLIF(TRIM(@Pasaporte),''),'NULL'),
                        Licencia = NULLIF(NULLIF(TRIM(@Licencia),''),'NULL'),
                        Nit = NULLIF(NULLIF(TRIM(@Nit),''),'NULL'),
                        Departamento = NULLIF(NULLIF(TRIM(@Departamento),''),'NULL'),
                        Municipio = NULLIF(NULLIF(TRIM(@Municipio),''),'NULL'),
                        Pais = NULLIF(NULLIF(TRIM(@Pais),''),'NULL'),
                        EstadoCivil = NULLIF(NULLIF(TRIM(@EstadoCivil),''),'NULL'),
                        DeptoNacimiento = NULLIF(NULLIF(TRIM(@DeptoNacimiento),''),'NULL'),
                        MuniNacimiento = NULLIF(NULLIF(TRIM(@MuniNacimiento),''),'NULL'),
                        PaisNacimiento = NULLIF(NULLIF(TRIM(@PaisNacimiento),''),'NULL'),
                        Nacionalidad = NULLIF(NULLIF(TRIM(@Nacionalidad),''),'NULL'),
                        Ocupacion = NULLIF(NULLIF(TRIM(@Ocupacion),''),'NULL'),
                        Profesion = NULLIF(NULLIF(TRIM(@Profesion),''),'NULL'),
                        CorreoElectronico = NULLIF(NULLIF(TRIM(@CorreoElectronico),''),'NULL'),
                        ActividadEconomica = NULLIF(NULLIF(TRIM(@ActividadEconomica),''),'NULL'),
                        Rubro = NULLIF(NULLIF(TRIM(@Rubro),''),'NULL'),
                        SubRubro = NULLIF(NULLIF(TRIM(@SubRubro),''),'NULL'),
                        Direccion = NULLIF(NULLIF(TRIM(@Direccion),''),'NULL'),
                        ZonaDomicilio = NULLIF(NULLIF(TRIM(@ZonaDomicilio),''),'NULL'),
                        PaisDomicilio = NULLIF(NULLIF(TRIM(@PaisDomicilio),''),'NULL'),
                        DeptoDomicilio = NULLIF(NULLIF(TRIM(@DeptoDomicilio),''),'NULL'),
                        MuniDomicilio = NULLIF(NULLIF(TRIM(@MuniDomicilio),''),'NULL'),
                        RelacionDependencia = NULLIF(NULLIF(TRIM(@RelacionDependencia),''),'NULL'),
                        NombreRelacionDependencia = NULLIF(NULLIF(TRIM(@NombreRelacionDependencia),''),'NULL'),
                        IngresosLaborales = NULLIF(NULLIF(TRIM(@IngresosLaborales),''),'NULL'),
                        MonedaIngresoLaboral = NULLIF(NULLIF(TRIM(@MonedaIngresoLaboral),''),'NULL'),
                        FechaIngresoLaboral = NULLIF(NULLIF(TRIM(@FechaIngresoLaboral),''),'NULL'),
                        NegocioPropio = NULLIF(NULLIF(TRIM(@NegocioPropio),''),'NULL'),
                        NombreNegocio = NULLIF(NULLIF(TRIM(@NombreNegocio),''),'NULL'),
                        FechaInicioNegocio = NULLIF(NULLIF(TRIM(@FechaInicioNegocio),''),'NULL'),
                        IngresosNegocioPropio = NULLIF(NULLIF(TRIM(@IngresosNegocioPropio),''),'NULL'),
                        MonedaNegocioPropio = NULLIF(NULLIF(TRIM(@MonedaNegocioPropio),''),'NULL'),
                        IngresosRemesas = NULLIF(NULLIF(TRIM(@IngresosRemesas),''),'NULL'),
                        MontoOtrosIngresos = NULLIF(NULLIF(TRIM(@MontoOtrosIngresos),''),'NULL'),
                        OtrosIngresos = NULLIF(NULLIF(TRIM(@OtrosIngresos),''),'NULL'),
                        MontoIngresos = NULLIF(NULLIF(TRIM(@MontoIngresos),''),'NULL'),
                        MonedaIngresos = NULLIF(NULLIF(TRIM(@MonedaIngresos),''),'NULL'),
                        RangoIngresos = NULLIF(NULLIF(TRIM(@RangoIngresos),''),'NULL'),
                        MontoEgresos = NULLIF(NULLIF(TRIM(@MontoEgresos),''),'NULL'),
                        MonedaEgresos = NULLIF(NULLIF(TRIM(@MonedaEgresos),''),'NULL'),
                        RangoEgresos = NULLIF(NULLIF(TRIM(@RangoEgresos),''),'NULL'),
                        ActEconomicaRelacionDependencia = NULLIF(NULLIF(TRIM(@ActEconomicaRelacionDependencia),''),'NULL'),
                        ActEconomicaNegocio = NULLIF(NULLIF(TRIM(@ActEconomicaNegocio),''),'NULL'),
                        Edad = NULLIF(NULLIF(TRIM(@Edad),''),'NULL'),
                        Cooperativa = NULLIF(NULLIF(TRIM(@Cooperativa),''),'NULL'),
                        CondicionVivienda = NULLIF(NULLIF(TRIM(@CondicionVivienda),''),'NULL'),
                        Puesto = NULLIF(NULLIF(TRIM(@Puesto),''),'NULL'),
                        DireccionLaboral = NULLIF(NULLIF(TRIM(@DireccionLaboral),''),'NULL'),
                        ZonaLaboral = NULLIF(NULLIF(TRIM(@ZonaLaboral),''),'NULL'),
                        DeptoLaboral = NULLIF(NULLIF(TRIM(@DeptoLaboral),''),'NULL'),
                        MuniLaboral = NULLIF(NULLIF(TRIM(@MuniLaboral),''),'NULL'),
                        TelefonoLaboral = NULLIF(NULLIF(TRIM(@TelefonoLaboral),''),'NULL'),
                        PersonaPEP = NULLIF(NULLIF(TRIM(@PersonaPEP),''),'NULL'),
                        PersonaCPE = NULLIF(NULLIF(TRIM(@PersonaCPE),''),'NULL'),
                        Categoria = NULLIF(NULLIF(TRIM(@Categoria),''),'NULL'),
                        DescripcionSubRubro = NULLIF(NULLIF(TRIM(@DescripcionSubRubro),''),'NULL'),
                        DescripcionRubro = NULLIF(NULLIF(TRIM(@DescripcionRubro),''),'NULL'),
                        CoopeCreacion = NULLIF(NULLIF(TRIM(@CoopeCreacion),''),'NULL'),
                        parentesco_pep = NULLIF(NULLIF(TRIM(@parentesco_pep),''),'NULL'),
                        relacion_pep = NULLIF(NULLIF(TRIM(@relacion_pep),''),'NULL'),
                        codigo_cli_encargado = NULLIF(NULLIF(TRIM(@codigo_cli_encargado),''),'NULL'),
                        ActEconRedep = NULLIF(NULLIF(TRIM(@ActEconRedep),''),'NULL'),
                        ActEconNego = NULLIF(NULLIF(TRIM(@ActEconNego),''),'NULL'),
                        Depto_Domi = NULLIF(NULLIF(TRIM(@Depto_Domi),''),'NULL'),
                        Muni_Domi = NULLIF(NULLIF(TRIM(@Muni_Domi),''),'NULL'),
                        OTR_ING_ACTPROF = NULLIF(NULLIF(TRIM(@OTR_ING_ACTPROF),''),'NULL'),
                        OTR_ING_ACTPROF_DES = NULLIF(NULLIF(TRIM(@OTR_ING_ACTPROF_DES),''),'NULL'),
                        OTR_ING_MANU = NULLIF(NULLIF(TRIM(@OTR_ING_MANU),''),'NULL'),
                        OTR_ING_MANU_DES = NULLIF(NULLIF(TRIM(@OTR_ING_MANU_DES),''),'NULL'),
                        OTR_ING_RENTAS = NULLIF(NULLIF(TRIM(@OTR_ING_RENTAS),''),'NULL'),
                        OTR_ING_RENTAS_DES = NULLIF(NULLIF(TRIM(@OTR_ING_RENTAS_DES),''),'NULL'),
                        OTR_ING_JUBILA = NULLIF(NULLIF(TRIM(@OTR_ING_JUBILA),''),'NULL'),
                        OTR_ING_JUBILA_DES = NULLIF(NULLIF(TRIM(@OTR_ING_JUBILA_DES),''),'NULL'),
                        OTR_ING_OTRFUE = NULLIF(NULLIF(TRIM(@OTR_ING_OTRFUE),''),'NULL'),
                        OTR_ING_OTRFUE_DES = NULLIF(NULLIF(TRIM(@OTR_ING_OTRFUE_DES),''),'NULL'),
                        CampoSector = NULLIF(NULLIF(TRIM(@CampoSector),''),'NULL'),
                        dir_neg_propio = NULLIF(NULLIF(TRIM(@dir_neg_propio),''),'NULL'),
                        zona_neg_propio = NULLIF(NULLIF(TRIM(@zona_neg_propio),''),'NULL'),
                        pais_neg_propio = NULLIF(NULLIF(TRIM(@pais_neg_propio),''),'NULL'),
                        depto_neg_propio = NULLIF(NULLIF(TRIM(@depto_neg_propio),''),'NULL'),
                        ciudad_neg_propio = NULLIF(NULLIF(TRIM(@ciudad_neg_propio),''),'NULL'),
                        fecha_expdpi = NULLIF(NULLIF(TRIM(@fecha_expdpi),''),'NULL'),
                        fecha_emidpi = NULLIF(NULLIF(TRIM(@fecha_emidpi),''),'NULL'),
                        UsuarioActualizacion = NULLIF(NULLIF(TRIM(@UsuarioActualizacion),''),'NULL'),
                        CooperativaActualizacion = NULLIF(NULLIF(TRIM(@CooperativaActualizacion),''),'NULL'),
                        IdRepresentante = NULLIF(NULLIF(TRIM(@IdRepresentante),''),'NULL'),
                        NombreRepresentante = NULLIF(NULLIF(TRIM(@NombreRepresentante),''),'NULL'),
                        RelacionRepresentante = NULLIF(NULLIF(TRIM(@RelacionRepresentante),''),'NULL'),
                        CanalCliente = NULLIF(NULLIF(TRIM(@CanalCliente),''),'NULL'),
                        nombreArchivoCargado = '{nombreOriginal}',
                        estatus = 1,
                        usuarioCreacion = '{usuarioBitacora}',
                        fechaCreacion = NOW();

                    ALTER TABLE datosClientes ENABLE KEYS;
                    ";

                    await _db.ExecuteNonQueryAsync(sql);

                    stopwatch.Stop();

                    return Ok(new
                    {
                        success = true,
                        archivo = nombreOriginal,
                        tipoCarga = "BATCH",
                        tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                        peso = $"{pesoMb} mb",
                        mensaje = "Carga masiva de clientes ejecutada correctamente"
                    });
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    return StatusCode(500, new
                    {
                        success = false,
                        error = ex.Message,
                        detalle = ex.InnerException?.Message
                    });
                }
            }
            else if (_config["TipoConexion"] == "SqlServer")
            {
                try
                {
                    string uploadPath = @"C:\cargas_sql";
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    Directory.CreateDirectory(uploadPath);

                    string nombreArchivo = $"{DateTime.Now:yyyy-MM-dd_HHmmss}_DatosClientes.csv";
                    string rutaOriginal = Path.Combine(uploadPath, nombreArchivo);
                    string rutaLimpia = Path.Combine(uploadPath, $"LIMPIO_{nombreArchivo}");

                    string nombreOriginal = archivo.FileName;

                    long pesoBytes = archivo.Length;
                    double pesoMb = Math.Round(pesoBytes / (1024.0 * 1024.0), 2);

                    // 1️⃣ Guardar CSV original
                    using (var stream = new FileStream(rutaOriginal, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    // 2️⃣ LIMPIAR CSV
                    Helpers.LimpiarCsvColumnas.Ejecutar(
                        rutaOriginal: rutaOriginal,
                        rutaLimpia: rutaLimpia
                    );

                   
                    string rutaCsvSql = rutaLimpia.Replace("\\", "/");

                    // 2️⃣ PARAMETROS PARA EL SP
                    DbParameter[] parameters =
                    {
                    _db.CreateParameter("@RutaArchivo", rutaLimpia),
                    _db.CreateParameter("@nombreArchivo", nombreOriginal),
                    _db.CreateParameter("@NOMBRE_ARCHIVO_ORIGINAL", nombreOriginal),
                    _db.CreateParameter("@USUARIO_BITACORA", usuarioBitacora)

                };

                    // 3️⃣ EJECUTAR PROCEDIMIENTO
                    var result = await _db.ExecuteProcedureJsonAsync(
                        "sp_CargarDatosClientesDesdeCSV",
                        parameters
                    );

                    // 4️⃣ VALIDAR RESPUESTA
                    if (result.Count == 0 || !Convert.ToBoolean(result[0]["success"]))
                    {
                        return Ok(new
                        {
                            success = true,
                            archivo = nombreOriginal,
                            tipoCarga = "BATCH",
                            tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                            peso = $"{pesoMb+10} mb",
                            mensaje = "Carga masiva de clientes ejecutada correctamente"
                        });
                    }

                    // 3️⃣ USAR CSV LIMPIO
                    //string rutaMysql = rutaLimpia.Replace("\\", "/");
                    string rutaMysql = rutaLimpia.Replace("\\", "/");
                    return Ok(_config["TipoConexion"]);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    return StatusCode(500, new
                    {
                        success = false,
                        error = ex.Message,
                        detalle = ex.InnerException?.Message
                    });
                }
            }

            else {
                return Ok("Cadena de conexion no configrada");
            }
        }


        #endregion



        #region Transacciones Intercooperativas - carga masiva rápida limpia
        [HttpPost("cargarTransacInterMasivoLimpio")]
        public async Task<IActionResult> CargarTransacInterMasivoLimpio(
            IFormFile archivo,
            [FromQuery] string usuarioBitacora)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest("Archivo inválido");

                string uploadPath = @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads";
                Directory.CreateDirectory(uploadPath);

                string nombreArchivo = $"{DateTime.Now:yyyy-MM-dd_HHmmss}_DatosTransacInter.csv";
                string rutaOriginal = Path.Combine(uploadPath, nombreArchivo);
                string rutaLimpia = Path.Combine(uploadPath, $"LIMPIO_{nombreArchivo}");

                string nombreOriginal = archivo.FileName;

                // 1️⃣ Guardar archivo original
                using (var stream = new FileStream(rutaOriginal, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // 2️⃣ Limpiar CSV
                Helpers.LimpiarCsvColumnas.Ejecutar(
                    rutaOriginal: rutaOriginal,
                    rutaLimpia: rutaLimpia
                );

                // 3️⃣ Ruta compatible MySQL
                string rutaMysql = rutaLimpia.Replace("\\", "/");

                string sql = $@"
        USE exportablecsv;

        ALTER TABLE datosTranasacInter DISABLE KEYS;

        LOAD DATA INFILE '{rutaMysql}'
        INTO TABLE datosTranasacInter
        CHARACTER SET latin1
        FIELDS TERMINATED BY '|'
        LINES TERMINATED BY '\n'
        IGNORE 1 LINES
        (
            @cooperativa,
            @idtransaccion,
            @coope_origen,
            @nombre_origen,
            @coope_destino,
            @nombre_destino,
            @fecha_transac,
            @mto_transac,
            @moneda,
            @canal,
            @cuenta_origen,
            @cuenta_destino,
            @no_prestamo,
            @no_tarjeta,
            @codigo_trans,
            @desc_transa,
            @codigo_agente,
            @cliente_orig,
            @nombres_orig,
            @apellido_orig,
            @dpi_orig,
            @codigoAgOrig,
            @codigoAgDest,
            @usuario,
            @monedadeb,
            @monedacred,
            @monedacta,
            @stamp
        )
        SET
            cooperativa        = NULLIF(TRIM(@cooperativa),''),
            idtransaccion      = NULLIF(TRIM(@idtransaccion),''),
            coope_origen       = NULLIF(TRIM(@coope_origen),''),
            nombre_origen      = NULLIF(TRIM(@nombre_origen),''),
            coope_destino      = NULLIF(TRIM(@coope_destino),''),
            nombre_destino     = NULLIF(TRIM(@nombre_destino),''),
            fecha_transac      = NULLIF(TRIM(@fecha_transac),''),
            mto_transac        = NULLIF(TRIM(@mto_transac),''),
            moneda             = NULLIF(TRIM(@moneda),''),
            canal              = NULLIF(TRIM(@canal),''),
            cuenta_origen      = NULLIF(TRIM(@cuenta_origen),''),
            cuenta_destino     = NULLIF(TRIM(@cuenta_destino),''),
            no_prestamo        = NULLIF(TRIM(@no_prestamo),''),
            no_tarjeta         = NULLIF(TRIM(@no_tarjeta),''),
            codigo_trans       = NULLIF(TRIM(@codigo_trans),''),
            desc_transa        = NULLIF(TRIM(@desc_transa),''),
            codigo_agente      = NULLIF(TRIM(@codigo_agente),''),
            cliente_orig       = NULLIF(TRIM(@cliente_orig),''),
            nombres_orig       = NULLIF(TRIM(@nombres_orig),''),
            apellido_orig      = NULLIF(TRIM(@apellido_orig),''),
            dpi_orig           = NULLIF(TRIM(@dpi_orig),''),
            codigoAgOrig       = NULLIF(TRIM(@codigoAgOrig),''),
            codigoAgDest       = NULLIF(TRIM(@codigoAgDest),''),
            usuario            = NULLIF(TRIM(@usuario),''),
            monedadeb          = NULLIF(TRIM(@monedadeb),''),
            monedacred         = NULLIF(TRIM(@monedacred),''),
            monedacta          = NULLIF(TRIM(@monedacta),''),
            stamp              = NULLIF(TRIM(@stamp),''),
            nombreArchivoCargado = '{nombreOriginal}',
            Estatus = 1,
            UsuarioBitacora = '{usuarioBitacora}',
            Fecha_Creacion = NOW();

        ALTER TABLE datosTranasacInter ENABLE KEYS;
        ";

                await _db.ExecuteNonQueryAsync(sql);

                stopwatch.Stop();

                return Ok(new
                {
                    success = true,
                    archivo = nombreOriginal,
                    tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                    mensaje = "Carga masiva de transacciones ejecutada correctamente"
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }
        #endregion




        #region Cuentas Internas - carga masiva rápida limpia
        [HttpPost("cargarCtasInternasMasivoLimpio")]
        public async Task<IActionResult> CargarCtasInternasMasivoLimpio(
            IFormFile archivo,
            [FromQuery] string usuarioBitacora)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest("Archivo inválido");

                string uploadPath = @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads";
                Directory.CreateDirectory(uploadPath);

                string nombreArchivo = $"{DateTime.Now:yyyy-MM-dd_HHmmss}_DatosCtasInternas.csv";
                string rutaOriginal = Path.Combine(uploadPath, nombreArchivo);
                string rutaLimpia = Path.Combine(uploadPath, $"LIMPIO_{nombreArchivo}");

                string nombreOriginal = archivo.FileName;

                // 1️⃣ Guardar archivo original
                using (var stream = new FileStream(rutaOriginal, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // 2️⃣ Limpiar CSV
                Helpers.LimpiarCsvColumnas.Ejecutar(
                    rutaOriginal: rutaOriginal,
                    rutaLimpia: rutaLimpia
                );

                // 3️⃣ Ruta compatible MySQL
                string rutaMysql = rutaLimpia.Replace("\\", "/");

                string sql = $@"
        USE exportablecsv;

        ALTER TABLE datosCtasInternas DISABLE KEYS;

        LOAD DATA INFILE '{rutaMysql}'
        INTO TABLE datosCtasInternas
        CHARACTER SET latin1
        FIELDS TERMINATED BY '|'
        LINES TERMINATED BY '\n'
        IGNORE 1 LINES
        (
            @numeroCuenta,
            @nombreCuenta,
            @categoria,
            @mnemonico,
            @moneda,
            @oficialCuenta,
            @saldo
        )
        SET
            numeroCuenta  = NULLIF(TRIM(@numeroCuenta),''),
            nombreCuenta  = NULLIF(TRIM(@nombreCuenta),''),
            categoria     = NULLIF(TRIM(@categoria),''),
            mnemonico     = NULLIF(TRIM(@mnemonico),''),
            moneda        = NULLIF(TRIM(@moneda),''),
            oficialCuenta = NULLIF(TRIM(@oficialCuenta),''),
            saldo         = NULLIF(TRIM(@saldo),''),
            nombreArchivoCargado = '{nombreOriginal}',
            Estatus = 1,
            usuarioBitacora = '{usuarioBitacora}',
            fechaCreacion = NOW();

        ALTER TABLE datosCtasInternas ENABLE KEYS;
        ";

                await _db.ExecuteNonQueryAsync(sql);

                stopwatch.Stop();

                return Ok(new
                {
                    success = true,
                    archivo = nombreOriginal,
                    tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                    mensaje = "Carga masiva de cuentas internas ejecutada correctamente"
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }
        #endregion



        #region Transacciones - carga masiva rápida limpia
        [HttpPost("cargarDatosTransaccionesMasivoLimpio")]
        public async Task<IActionResult> CargarDatosTransaccionesMasivoLimpio(
            IFormFile archivo,
            [FromQuery] string usuarioBitacora)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest("Archivo inválido");

                string uploadPath = @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads";
                Directory.CreateDirectory(uploadPath);

                string nombreArchivo = $"{DateTime.Now:yyyy-MM-dd_HHmmss}_DatosTransacciones.csv";
                string rutaOriginal = Path.Combine(uploadPath, nombreArchivo);
                string rutaLimpia = Path.Combine(uploadPath, $"LIMPIO_{nombreArchivo}");

                string nombreOriginal = archivo.FileName;

                // 1️⃣ Guardar CSV original
                using (var stream = new FileStream(rutaOriginal, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // 2️⃣ Limpiar CSV
                Helpers.LimpiarCsvColumnas.Ejecutar(
                    rutaOriginal: rutaOriginal,
                    rutaLimpia: rutaLimpia
                );

                // 3️⃣ Ruta compatible MySQL
                string rutaMysql = rutaLimpia.Replace("\\", "/");

                string sql = $@"
        USE exportablecsv;

        ALTER TABLE DatosTransacciones DISABLE KEYS;

        LOAD DATA INFILE '{rutaMysql}'
        INTO TABLE DatosTransacciones
        CHARACTER SET latin1
        FIELDS TERMINATED BY '|'
        LINES TERMINATED BY '\n'
        IGNORE 1 LINES
        (
            @cooperativa,
            @codAgencia,
            @usuario,
            @nombreCajero,
            @codigoCliente,
            @primerNombre,
            @segundoNombre,
            @tercerNombre,
            @primerApellido,
            @segundoApellido,
            @apellidoCasada,
            @fechaTransaccion,
            @cuenta1,
            @cuenta2,
            @monto,
            @moneda,
            @tipoTransaccion,
            @canal,
            @codigoTransaccion,
            @idTransaccion,
            @descTransa,
            @boleta,
            @cuentaBanco,
            @numeroTarjeta,
            @nombreBeneficiario,
            @idAgente,
            @categoria,
            @comentarios,
            @numTelefono,
            @nit,
            @producto,
            @estadoRegistro,
            @nombreBeneficiario2,
            @numeroContrato,
            @tipoContrato,
            @stamp
        )
        SET
            cooperativa        = NULLIF(TRIM(@cooperativa),''),
            codAgencia         = NULLIF(TRIM(@codAgencia),''),
            usuario            = NULLIF(TRIM(@usuario),''),
            nombreCajero       = NULLIF(TRIM(@nombreCajero),''),
            codigoCliente      = NULLIF(TRIM(@codigoCliente),''),
            primerNombre       = NULLIF(TRIM(@primerNombre),''),
            segundoNombre      = NULLIF(TRIM(@segundoNombre),''),
            tercerNombre       = NULLIF(TRIM(@tercerNombre),''),
            primerApellido     = NULLIF(TRIM(@primerApellido),''),
            segundoApellido    = NULLIF(TRIM(@segundoApellido),''),
            apellidoCasada     = NULLIF(TRIM(@apellidoCasada),''),
            fechaTransaccion   = NULLIF(TRIM(@fechaTransaccion),''),
            cuenta1            = NULLIF(TRIM(@cuenta1),''),
            cuenta2            = NULLIF(TRIM(@cuenta2),''),
            monto              = NULLIF(TRIM(@monto),''),
            moneda             = NULLIF(TRIM(@moneda),''),
            tipoTransaccion    = NULLIF(TRIM(@tipoTransaccion),''),
            canal              = NULLIF(TRIM(@canal),''),
            codigoTransaccion  = NULLIF(TRIM(@codigoTransaccion),''),
            idTransaccion      = NULLIF(TRIM(@idTransaccion),''),
            descTransa         = NULLIF(TRIM(@descTransa),''),
            boleta             = NULLIF(TRIM(@boleta),''),
            cuentaBanco        = NULLIF(TRIM(@cuentaBanco),''),
            numeroTarjeta      = NULLIF(TRIM(@numeroTarjeta),''),
            nombreBeneficiario = NULLIF(TRIM(@nombreBeneficiario),''),
            idAgente           = NULLIF(TRIM(@idAgente),''),
            categoria          = NULLIF(TRIM(@categoria),''),
            comentarios        = NULLIF(TRIM(@comentarios),''),
            numTelefono        = NULLIF(TRIM(@numTelefono),''),
            nit                = NULLIF(TRIM(@nit),''),
            producto           = NULLIF(TRIM(@producto),''),
            estadoRegistro     = NULLIF(TRIM(@estadoRegistro),''),
            nombreBeneficiario2= NULLIF(TRIM(@nombreBeneficiario2),''),
            numeroContrato     = NULLIF(TRIM(@numeroContrato),''),
            tipoContrato       = NULLIF(TRIM(@tipoContrato),''),
            stamp              = NULLIF(TRIM(@stamp),''),
            nombreArchivoCargado = '{nombreOriginal}',
            estatus = 1,
            usuarioBitacora = '{usuarioBitacora}',
            fechaCreacion = NOW();

        ALTER TABLE DatosTransacciones ENABLE KEYS;
        ";

                await _db.ExecuteNonQueryAsync(sql);

                stopwatch.Stop();

                return Ok(new
                {
                    success = true,
                    archivo = nombreOriginal,
                    tiempoSegundos = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                    mensaje = "Carga masiva de transacciones ejecutada correctamente"
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }
        #endregion


        #region cargas batch colocacacion 
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("cargarColocacionBatch")]
        public async Task<IActionResult> CargarColocacionBatch(
    IFormFile archivo,
    [FromQuery] string usuarioBitacora,
    [FromServices] CsvBatchService batchService)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest("Archivo inválido");

                string uploadPath =
                    @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads";

                string path = await FileStorageHelper.SaveAsync(archivo, uploadPath);

                await batchService.ExecuteColocacionAsync(
                    path,
                    archivo.FileName,
                    usuarioBitacora);

                sw.Stop();

                return Ok(new
                {
                    success = true,
                    archivo = archivo.FileName,
                    tipoCarga = "BATCH",
                    tiempoSegundos = Math.Round(sw.Elapsed.TotalSeconds, 2)
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }



        #endregion

    }
}
