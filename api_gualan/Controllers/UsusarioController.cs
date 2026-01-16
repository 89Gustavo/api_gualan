using api_gualan.Helpers.Interfaces;
using api_gualan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace api_gualan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IDbHelper _db;
        private readonly IConfiguration _config;

        public UsuarioController(IDbHelper db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // =====================================================
        // 🔹 GET api/usuario
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _db.ExecuteProcedureJsonAsync("sp_lista_usuarios");
            return Ok(data);
        }

        // =====================================================
        // 🔹 POST api/usuario/crear
        // =====================================================
        [HttpPost("crear")]
        public async Task<IActionResult> CrearUsuario([FromBody] Usuario_model usuario)
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
                        new MySqlParameter("@p_codigoUsuario", MySqlDbType.VarChar)
                        {
                            Value = usuario.codigoUsuario
                        },
                        new MySqlParameter("@p_codiogoPersona", MySqlDbType.VarChar)
                        {
                            Value = usuario.codigoPersona
                        },
                        new MySqlParameter("@p_codigoRol", MySqlDbType.VarChar)
                        {
                            Value = usuario.codigoRol
                        },
                        new MySqlParameter("@p_nombreUsuario", MySqlDbType.VarChar)
                        {
                            Value = usuario.nombreUsuario
                        },
                        new MySqlParameter("@p_claveUsuario", MySqlDbType.VarChar)
                        {
                            Value = usuario.claveUsuario
                        },
                        new MySqlParameter("@USUARIO_BITACORA", MySqlDbType.VarChar)
                        {
                            Value = usuario.UsuarioBitacora
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
                        new SqlParameter("@p_codigoUsuario", System.Data.SqlDbType.VarChar)
                        {
                            Value = usuario.codigoUsuario
                        },
                        new SqlParameter("@p_codiogoPersona", System.Data.SqlDbType.VarChar)
                        {
                            Value = usuario.codigoPersona
                        },
                        new SqlParameter("@p_codigoRol", System.Data.SqlDbType.VarChar)
                        {
                            Value = usuario.codigoRol
                        },
                        new SqlParameter("@p_nombreUsuario", System.Data.SqlDbType.VarChar)
                        {
                            Value = usuario.nombreUsuario
                        },
                        new SqlParameter("@p_claveUsuario", System.Data.SqlDbType.VarChar)
                        {
                            Value = usuario.claveUsuario
                        },
                        new SqlParameter("@USUARIO_BITACORA", System.Data.SqlDbType.VarChar)
                        {
                            Value = usuario.UsuarioBitacora
                        }
                    };
                }
                else
                {
                    return StatusCode(500, "TipoConexion no soportado");
                }

                int filasAfectadas = await _db.ExecuteProcedureNonQueryAsync(
                    "sp_InsertUpdate_usuarios",
                    parameters
                );

                return Ok(new
                {
                    success = true,
                    message = "Usuario creado correctamente",
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
