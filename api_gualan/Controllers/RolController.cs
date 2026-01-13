using api_gualan.Helpers;
using api_gualan.Models;
using Microsoft.AspNetCore.Mvc;

using MySqlConnector;


namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : Controller
    {
        private readonly Helpers.MySqlHelper _db;

        public RolController(Helpers.MySqlHelper db)
        {
            _db = db;
        }
        // GET api/rol
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _db.ExecuteProcedureJsonAsync("sp_listaRol");
            return Ok(data);
        }

        // POST api/rol
        [HttpPost("crear")]
        public async Task<IActionResult> CrearPersona([FromBody] Rol_model rol)
        {
            try
            {
                // Crear parámetros para el procedimiento
                var parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@p_codigoRol", MySqlConnector.MySqlDbType.VarChar) { Value = rol.codigoRol},
                        new MySqlConnector.MySqlParameter("@p_nombreRol", MySqlConnector.MySqlDbType.VarChar) { Value = rol.nombreRol},
                        new MySqlConnector.MySqlParameter("@USUARIO_BITACORA", MySqlConnector.MySqlDbType.VarChar) { Value = rol.UsuarioBitacora}
                

                    };

                // Ejecutar procedimiento
                int filasAfectadas = await _db.ExecuteProcedureNonQueryAsync("sp_InsertUpdate_rol", parameters);

                return Ok(new { message = "Rol creado correctamente", filasAfectadas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
