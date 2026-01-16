using api_gualan.Helpers.Interfaces;
using api_gualan.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IDbHelper _db;

        public AuthController(IDbHelper db)
        {
            _db = db;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                DbParameter[] parameters =
                {
                    _db.CreateParameter("@p_nombreUsuario", request.nombreUsuario),
                    _db.CreateParameter("@p_clave", request.claveUsuario)
                };

                var result = await _db.ExecuteProcedureJsonAsync(
                    "sp_login_usuario",
                    parameters
                );

                if (result.Count == 0 ||
                    !Convert.ToBoolean(result[0]["success"]))
                {
                    return Ok(new LoginResponseDto
                    {
                        success = false,
                        nombreUsuario = "",
                        codigoRol = 0
                    });
                }

                var row = result[0];

                return Ok(new LoginResponseDto
                {
                    success = true,
                    nombreUsuario = row["nombreUsuario"].ToString(),
                    codigoRol = Convert.ToInt32(row["codigoRol"])
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }
}
