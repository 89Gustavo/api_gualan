using System;
using System.IO;
using System.Linq;
using System.Text;

namespace api_gualan.Helpers
{
    public class LimpiarCsvColumnas
    {
        /// <summary>
        /// Normaliza un CSV para que todas las filas tengan la misma cantidad de columnas
        /// Rellena columnas faltantes con vacío (que luego MySQL convierte en NULL)
        /// </summary>
        /// <param name="rutaOriginal">Ruta del CSV original</param>
        /// <param name="rutaLimpia">Ruta del CSV limpio</param>
        /// <param name="separador">Separador de columnas (por defecto |)</param>
        public static void Ejecutar(
            string rutaOriginal,
            string rutaLimpia,
            char separador = '|')
        {
            if (!File.Exists(rutaOriginal))
                throw new FileNotFoundException("No se encontró el archivo CSV original", rutaOriginal);

            using var reader = new StreamReader(rutaOriginal, Encoding.Latin1);
            using var writer = new StreamWriter(rutaLimpia, false, Encoding.Latin1);

            // 1️⃣ Leer encabezado
            string header = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(header))
                throw new Exception("El archivo CSV no contiene encabezado");

            writer.WriteLine(header);

            int columnasEsperadas = header.Split(separador).Length;

            string linea;
            int numeroLinea = 1;

            // 2️⃣ Procesar filas
            while ((linea = reader.ReadLine()) != null)
            {
                numeroLinea++;

                // Mantener líneas vacías
                if (string.IsNullOrWhiteSpace(linea))
                {
                    writer.WriteLine(string.Join(separador,
                        Enumerable.Repeat(string.Empty, columnasEsperadas)));
                    continue;
                }

                var columnas = linea.Split(separador);

                if (columnas.Length < columnasEsperadas)
                {
                    // Rellenar columnas faltantes
                    var columnasCompletas = columnas
                        .Concat(Enumerable.Repeat(string.Empty,
                            columnasEsperadas - columnas.Length));

                    writer.WriteLine(string.Join(separador, columnasCompletas));
                }
                else if (columnas.Length > columnasEsperadas)
                {
                    // Recortar columnas sobrantes
                    writer.WriteLine(string.Join(separador,
                        columnas.Take(columnasEsperadas)));
                }
                else
                {
                    // Fila correcta
                    writer.WriteLine(linea);
                }
            }
        }
    }
}
