using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/csv")]
    public class CsvController : ControllerBase
    {
        [HttpPost("limpiar")]
        public async Task<IActionResult> LimpiarCsv(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo CSV requerido");

            var outputPath = Path.Combine(
                Path.GetTempPath(),
                $"LIMPIO_{archivo.FileName}"
            );

            using var reader = new StreamReader(archivo.OpenReadStream(), Encoding.Latin1);
            using var writer = new StreamWriter(outputPath, false, Encoding.Latin1);

            while (!reader.EndOfStream)
            {
                var linea = await reader.ReadLineAsync();
                if (linea == null) continue;

                var columnas = linea.Split('|');

                for (int i = 0; i < columnas.Length; i++)
                {
                    columnas[i] = LimpiarValor(columnas[i]);
                }

                await writer.WriteLineAsync(string.Join("|", columnas));
            }

            return Ok(new
            {
                mensaje = "CSV limpiado correctamente",
                archivoSalida = outputPath
            });
        }

        private static string LimpiarValor(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "NULL";

            valor = valor.Trim();

            if (valor.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return "NULL";

            return valor;
        }
    }
}
