namespace api_gualan.Services
{
    public static class CsvValidator
    {
        public static void Validate(string file, int expectedColumns, char delimiter)
        {
            var header = File.ReadLines(file).First();
            var columns = header.Split(delimiter);

            if (columns.Length != expectedColumns)
                throw new Exception($"Columnas inválidas: {columns.Length}");
        }
        public static void ValidateDelimiter(string path, char delimiter)
        {
            var header = File.ReadLines(path).First();
            if (!header.Contains(delimiter))
                throw new Exception("El archivo no usa el delimitador esperado");
        }
    }
}
