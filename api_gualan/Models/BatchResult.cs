namespace api_gualan.Models;

public class BatchResult
{
    public bool Success { get; set; }
    public string Archivo { get; set; }
    public double TiempoSegundos { get; set; }
    public string Mensaje { get; set; }
}
