using api_gualan.Helpers;
using api_gualan.Models;
using Microsoft.AspNetCore.Mvc;

using MySqlConnector;


namespace api_gualan.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PersonaController : Controller
    {
        private readonly Helpers.MySqlHelper _db;

        public PersonaController(Helpers.MySqlHelper db)
        {
            _db = db;
        }

        // GET api/personas
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _db.ExecuteProcedureJsonAsync("sp_listar_personas");
            return Ok(data);
        }

        // POST api/personas
 
        [HttpPost("crear")]
        public async Task<IActionResult> CrearPersona([FromBody] Persona_model persona)
        {
            try
            {
                // Crear parámetros para el procedimiento
                var parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@p_primerNombre", MySqlConnector.MySqlDbType.VarChar) { Value = persona.primerNombre},
                        new MySqlConnector.MySqlParameter("@p_segundoNombre", MySqlConnector.MySqlDbType.VarChar) { Value = persona.segundoNombre},
                        new MySqlConnector.MySqlParameter("@p_tercerNombre", MySqlConnector.MySqlDbType.VarChar) { Value = persona.tercerNombre},
                        new MySqlConnector.MySqlParameter("@p_primerApellido", MySqlConnector.MySqlDbType.VarChar) { Value = persona.primerApellido},
                        new MySqlConnector.MySqlParameter("@p_segundoApellido", MySqlConnector.MySqlDbType.VarChar) { Value = persona.segundoApellido},
                        new MySqlConnector.MySqlParameter("@p_apellidoCasada", MySqlConnector.MySqlDbType.VarChar) { Value = persona.apellidoCasada},                        
                        new MySqlConnector.MySqlParameter("@p_dpi", MySqlConnector.MySqlDbType.VarChar) { Value = persona.dpi },
                        new MySqlConnector.MySqlParameter("@p_fechaNacimiento", MySqlConnector.MySqlDbType.VarChar) { Value = persona.fechaNacimiento },
                        new MySqlConnector.MySqlParameter("@p_edad", MySqlConnector.MySqlDbType.VarChar) { Value = persona.edad },
                        new MySqlConnector.MySqlParameter("@USUARIO_BITACORA", MySqlConnector.MySqlDbType.VarChar) { Value = persona.UsuarioBitacora },

                    };

                // Ejecutar procedimiento
                int filasAfectadas = await _db.ExecuteProcedureNonQueryAsync("sp_InsertUpdate_personas", parameters);

                return Ok(new { message = "Persona creada correctamente", filasAfectadas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}
