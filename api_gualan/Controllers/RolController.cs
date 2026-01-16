using api_gualan.Helpers.Interfaces;
using api_gualan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data.Common;

namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : ControllerBase
    {
        private readonly IDbHelper _db;
        private readonly IConfiguration _config;

        public RolController(IDbHelper db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // =====================================================
        // 🔹 GET api/rol
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _db.ExecuteProcedureJsonAsync("sp_listaRol");
            return Ok(data);
        }

        // =====================================================
        // 🔹 POST api/rol/crear
        // =====================================================
        [HttpPost("crear")]
        public async Task<IActionResult> CrearRol([FromBody] Rol_model rol)
        {
            try
            {
                var tipoConexion = _config["TipoConexion"];
                DbParameter[] parameters;

                // =====================================================
                // 🔹 MySQL
                // =====================================================
                if (tipoConexion == "MySql")
                {
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@p_codigoRol", MySqlDbType.VarChar)
                        {
                            Value = rol.codigoRol
                        },
                        new MySqlParameter("@p_nombreRol", MySqlDbType.VarChar)
                        {
                            Value = rol.nombreRol
                        },
                        new MySqlParameter("@USUARIO_BITACORA", MySqlDbType.VarChar)
                        {
                            Value = rol.UsuarioBitacora
                        }
                    };
                }
                // =====================================================
                // 🔹 SQL SERVER
                // =====================================================
                else if (tipoConexion == "SqlServer")
                {
                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@p_codigoRol", System.Data.SqlDbType.VarChar)
                        {
                            Value = rol.codigoRol
                        },
                        new SqlParameter("@p_nombreRol", System.Data.SqlDbType.VarChar)
                        {
                            Value = rol.nombreRol
                        },
                        new SqlParameter("@USUARIO_BITACORA", System.Data.SqlDbType.VarChar)
                        {
                            Value = rol.UsuarioBitacora
                        }
                    };
                }
                else
                {
                    return StatusCode(500, "TipoConexion no soportado");
                }

                int filasAfectadas = await _db.ExecuteProcedureNonQueryAsync(
                    "sp_InsertUpdate_rol",
                    parameters
                );

                return Ok(new
                {
                    success = true,
                    message = "Rol creado correctamente",
                    filasAfectadas
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
