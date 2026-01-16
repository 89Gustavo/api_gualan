using api_gualan.Dtos;
using api_gualan.Helpers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/menu")]
    public class MenuController : ControllerBase
    {
        private readonly IDbHelper _db;

        public MenuController(IDbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// Devuelve el menú según el rol
        /// </summary>
        [HttpGet("menuRol/{codigoRol}")]
        public async Task<IActionResult> ObtenerMenuPorRol(int codigoRol)
        {
            try
            {
                DbParameter[] parameters =
                {
                    _db.CreateParameter("@p_codigoRol", codigoRol)
                };

                var data = await _db.ExecuteProcedureJsonAsync(
                    "sp_lista_menuRol",
                    parameters
                );

                var result = data.Select(row => new MenuRolDto
                {
                    codigoMenu = Convert.ToInt32(row["codigoMenu"]),
                    padre = Convert.ToInt32(row["padre"]),
                    texto = row["texto"]?.ToString(),
                    href = row["href"]?.ToString()
                }).ToList();

                return Ok(result);
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
