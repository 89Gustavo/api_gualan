using api_gualan.Helpers;
using api_gualan.Models;
using Microsoft.AspNetCore.Mvc;

using MySqlConnector;
using api_gualan.Dtos;


namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly Helpers.MySqlHelper _db;

        public AuthController(Helpers.MySqlHelper db)
        {
            _db = db;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var parameters = new MySqlConnector.MySqlParameter[]
                {
                     new MySqlConnector.MySqlParameter("@p_nombreUsuario", MySqlConnector.MySqlDbType.VarChar) { Value = request.nombreUsuario},
                     new MySqlConnector.MySqlParameter("@p_clave", MySqlConnector.MySqlDbType.VarChar) { Value = request.claveUsuario},
                };

                var result = await _db.ExecuteProcedureJsonAsync("sp_login_usuario",parameters);

                if (result.Count == 0 || Convert.ToBoolean(result[0]["success"]) == false)
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
