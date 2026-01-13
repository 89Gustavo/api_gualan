using System.Text;
using api_gualan.Dtos;

namespace api_gualan.Helpers
{
    public static class ColocacionHelper
    {
        public static IEnumerable<DatosColocacionDto> LeerCsv(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            string? line;
            bool firstLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (firstLine)
                {
                    firstLine = false;
                    continue; // saltar encabezado
                }

                var c = line.Split(',');

                yield return new DatosColocacionDto
                {
                    Empresa = CsvParserUtils.Clean(c[0]),
                    Cliente = CsvParserUtils.Clean(c[1]),
                    NumeroDocumento = CsvParserUtils.Clean(c[2]),
                    ArrangementId = CsvParserUtils.Clean(c[3]),
                    Categoria = CsvParserUtils.Clean(c[4]),
                    TipoDocumento = CsvParserUtils.Clean(c[5]),
                    Moneda = CsvParserUtils.Clean(c[6]),
                    Producto = CsvParserUtils.Clean(c[7]),
                    AreaFinanciera = CsvParserUtils.Clean(c[8]),
                    UsuarioAsesor = CsvParserUtils.Clean(c[9]),
                    UsuarioCobranza = CsvParserUtils.Clean(c[10]),
                    UsuarioEjecutivo = CsvParserUtils.Clean(c[11]),
                    RecordStatus = CsvParserUtils.Clean(c[12]),
                    DiasMora = CsvParserUtils.ParseInt(c[13]),
                    TasaInteres = CsvParserUtils.ParseDecimal(c[14]),
                    MontoDocumento = CsvParserUtils.ParseDecimal(c[15]),
                    SaldoCapital = CsvParserUtils.ParseDecimal(c[16]),
                    CapitalVigente = CsvParserUtils.ParseDecimal(c[17]),
                    CapitalVencido = CsvParserUtils.ParseDecimal(c[18]),
                    InteresesVencidos = CsvParserUtils.ParseDecimal(c[19]),
                    MontoMora = CsvParserUtils.ParseDecimal(c[20]),
                    MontoAdeudado = CsvParserUtils.ParseDecimal(c[21]),
                    InteresesDevengados = CsvParserUtils.ParseDecimal(c[22]),
                    Plazo = CsvParserUtils.ParseInt(c[23]),
                    TipoGarantia = CsvParserUtils.Clean(c[24]),
                    FechaInicio = CsvParserUtils.ParseDate(c[25]),
                    FechaVencimiento = CsvParserUtils.ParseDate(c[26]),
                    FrecuenciaCapital = CsvParserUtils.Clean(c[27]),
                    FrecuenciaInteres = CsvParserUtils.Clean(c[28]),
                    EstadoDocumento = CsvParserUtils.Clean(c[29]),
                    FechaAprobacion = CsvParserUtils.ParseDate(c[30]),
                    CalificacionCredito = CsvParserUtils.Clean(c[31]),
                    ClasificacionProyecto = CsvParserUtils.Clean(c[32]),
                    OrigenFondos = CsvParserUtils.Clean(c[33]),
                    ColDisponible = CsvParserUtils.ParseDecimal(c[34]),
                    ActividadEconomica = CsvParserUtils.Clean(c[35]),
                    CuotaCapital = CsvParserUtils.ParseDecimal(c[36]),
                    Referencia = CsvParserUtils.Clean(c[37]),
                    LtDepurado = CsvParserUtils.Clean(c[38]),
                    Depurado = CsvParserUtils.Clean(c[39]),
                    FechaUltimoPago = CsvParserUtils.ParseDate(c[40]),
                    Gender = CsvParserUtils.Clean(c[41]),
                    MaritalStatus = CsvParserUtils.Clean(c[42]),
                    CuentaDebito = CsvParserUtils.Clean(c[43]),
                    CuentaCredito = CsvParserUtils.Clean(c[44]),
                    DiasMoraInteres = CsvParserUtils.ParseInt(c[45]),
                    MontoGastos = CsvParserUtils.ParseDecimal(c[46]),
                    FechaUltimoPagoCapital = CsvParserUtils.ParseDate(c[47]),
                    FechaProximoPagoCapital = CsvParserUtils.ParseDate(c[48]),
                    TipoFrecuenciaPago = CsvParserUtils.Clean(c[49]),
                    TipoProducto = CsvParserUtils.Clean(c[50]),
                    FechaRefinancia = CsvParserUtils.ParseDate(c[51]),
                    FechaReestructura = CsvParserUtils.ParseDate(c[52]),
                    MontoDesembolsado = CsvParserUtils.ParseDecimal(c[53]),
                    Fecha1erDesemb = CsvParserUtils.ParseDate(c[54]),
                    FechaUltDesemb = CsvParserUtils.ParseDate(c[55]),
                    UsuarioCreacionCta = CsvParserUtils.Clean(c[56]),
                    CategoriaProducto = CsvParserUtils.Clean(c[57]),
                    Grupo = CsvParserUtils.Clean(c[58]),
                    NumeroActa = CsvParserUtils.Clean(c[59]),
                    Payin = CsvParserUtils.Clean(c[60]),
                    Segpromutua = CsvParserUtils.Clean(c[61]),
                    Solicitud = CsvParserUtils.Clean(c[62]),
                    Guarantor = CsvParserUtils.Clean(c[63]),
                    TasaMora = CsvParserUtils.ParseDecimal(c[64])
                };
            }
        }
    }
}
