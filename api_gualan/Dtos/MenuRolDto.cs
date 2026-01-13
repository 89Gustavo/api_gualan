namespace api_gualan.Dtos
{
    public class MenuRolDto
    {
        public int codigoMenu { get; set; }
        public int padre { get; set; }   // 0 = raíz
        public string texto { get; set; }
        public string href { get; set; }
    }
}
