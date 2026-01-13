using api_gualan.Helpers;
using api_gualan.Models;
using Microsoft.AspNetCore.Mvc;

using MySqlConnector;
namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsusarioController : Controller
    {

        private readonly Helpers.MySqlHelper _db;

        public UsusarioController(Helpers.MySqlHelper db)
        {
            _db = db;
        }

        // GET api/personas
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _db.ExecuteProcedureJsonAsync("sp_lista_usuarios");
            return Ok(data);
        }
        // POST api/personas

        [HttpPost("crear")]
        public async Task<IActionResult> CrearPersona([FromBody] Usuario_model usuario)
        {
            try
            {
                // Crear parámetros para el procedimiento
                var parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@p_codigoUsuario", MySqlConnector.MySqlDbType.VarChar) { Value = usuario.codigoUsuario},
                        new MySqlConnector.MySqlParameter("@p_codiogoPersona", MySqlConnector.MySqlDbType.VarChar) { Value = usuario.codigoPersona},
                        new MySqlConnector.MySqlParameter("@p_codigoRol", MySqlConnector.MySqlDbType.VarChar) { Value = usuario.codigoRol},
                        new MySqlConnector.MySqlParameter("@p_nombreUsuario", MySqlConnector.MySqlDbType.VarChar) { Value = usuario.nombreUsuario},
                        new MySqlConnector.MySqlParameter("@p_claveUsuario", MySqlConnector.MySqlDbType.VarChar) { Value = usuario.claveUsuario},
                        new MySqlConnector.MySqlParameter("@USUARIO_BITACORA", MySqlConnector.MySqlDbType.VarChar) { Value = usuario.UsuarioBitacora }

                    };

                // Ejecutar procedimiento
                int filasAfectadas = await _db.ExecuteProcedureNonQueryAsync("sp_InsertUpdate_usuarios", parameters);

                return Ok(new { message = "Usuario creada correctamente", filasAfectadas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
