using api_gualan.Helpers.Interfaces;
using api_gualan.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonaController : ControllerBase
    {
        private readonly IDbHelper _db;

        public PersonaController(IDbHelper db)
        {
            _db = db;
        }

        // =====================================================
        // GET api/persona
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _db.ExecuteProcedureJsonAsync("sp_listar_personas");
            return Ok(data);
        }

        // =====================================================
        // POST api/persona/crear
        // =====================================================
        [HttpPost("crear")]
        public async Task<IActionResult> CrearPersona([FromBody] Persona_model persona)
        {
            try
            {
                var parameters = new[]
                {
                    _db.CreateParameter("@p_primerNombre", persona.primerNombre, DbType.String),
                    _db.CreateParameter("@p_segundoNombre", persona.segundoNombre, DbType.String),
                    _db.CreateParameter("@p_tercerNombre", persona.tercerNombre, DbType.String),
                    _db.CreateParameter("@p_primerApellido", persona.primerApellido, DbType.String),
                    _db.CreateParameter("@p_segundoApellido", persona.segundoApellido, DbType.String),
                    _db.CreateParameter("@p_apellidoCasada", persona.apellidoCasada, DbType.String),
                    _db.CreateParameter("@p_dpi", persona.dpi, DbType.String),
                    _db.CreateParameter("@p_fechaNacimiento", persona.fechaNacimiento, DbType.String),//formato 1990-05-12
                    _db.CreateParameter("@p_edad", persona.edad, DbType.Int32),
                    _db.CreateParameter("@USUARIO_BITACORA", persona.UsuarioBitacora, DbType.String)
                };

                int filasAfectadas =
                    await _db.ExecuteProcedureNonQueryAsync(
                        "sp_InsertUpdate_personas",
                        parameters
                    );

                return Ok(new
                {
                    message = "Persona creada correctamente",
                    filasAfectadas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
        }
    }
}
