using System.Text;
using api_gualan.Dtos;

namespace api_gualan.Helpers
{
    public static class CaptacionHelper
    {
        public static IEnumerable<DatosCaptacionDto> LeerCsv(Stream fileStream)
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

                yield return new DatosCaptacionDto
                {
                    codigo_empresa = CsvParserUtils.Clean(c[0]),
                    nombre_empresa = CsvParserUtils.Clean(c[1]),
                    cliente = CsvParserUtils.Clean(c[2]),
                    numero_cuenta = CsvParserUtils.Clean(c[3]),
                    arrangement = CsvParserUtils.Clean(c[4]),
                    categoria = CsvParserUtils.Clean(c[5]),
                    producto = CsvParserUtils.Clean(c[6]),
                    grupo_producto = CsvParserUtils.Clean(c[7]),
                    moneda = CsvParserUtils.Clean(c[8]),
                    saldo = CsvParserUtils.ParseDecimal(c[9]) ?? 0m,
                    estado = CsvParserUtils.Clean(c[10]),
                    fecha_apertura = CsvParserUtils.ParseDate(c[11]),
                    plazo = CsvParserUtils.ParseInt(c[12]),
                    vencimiento = CsvParserUtils.ParseDate(c[13]),
                    tasa = CsvParserUtils.ParseDecimal(c[14]),
                    interes_acumulado = CsvParserUtils.ParseDecimal(c[15]),
                    monto_reserva = CsvParserUtils.ParseDecimal(c[16]),
                    ejecutivo = CsvParserUtils.Clean(c[17]),
                    frecuencia_pago = CsvParserUtils.Clean(c[18]),
                    fecha_ultimo_credito = CsvParserUtils.ParseDate(c[19]),
                    fecha_ultimo_debito = CsvParserUtils.ParseDate(c[20]),
                    agencia = CsvParserUtils.Clean(c[21]),
                    usuario_act = CsvParserUtils.Clean(c[22]),
                    monto_disponible = CsvParserUtils.ParseDecimal(c[23]),
                    referencia = CsvParserUtils.Clean(c[24]),
                    tipo_cliente = CsvParserUtils.Clean(c[25]),
                    fecha_renovacion = CsvParserUtils.ParseDate(c[26]),
                    monto_apertura = CsvParserUtils.ParseDecimal(c[27]),
                    cuenta_dep_int = CsvParserUtils.Clean(c[28]),
                    nombre_cuenta_depint = CsvParserUtils.Clean(c[29]),
                    interes_x_pagar = CsvParserUtils.ParseDecimal(c[30]),
                    tarjeta_asociada = CsvParserUtils.Clean(c[31]),
                    categoria_producto = CsvParserUtils.Clean(c[32]),
                    clasificacion = CsvParserUtils.Clean(c[33]),
                    grupo = CsvParserUtils.Clean(c[34]),
                    usuario_creacion_cta = CsvParserUtils.Clean(c[35]),
                    encaje = CsvParserUtils.ParseDecimal(c[36]),
                    monto_pignorado = CsvParserUtils.ParseDecimal(c[37])
                };
            }
        }
    }
}
