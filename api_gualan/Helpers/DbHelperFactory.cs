    using api_gualan.Helpers.Interfaces;
    using api_gualan.Helpers.MySql;
    using api_gualan.Helpers.SqlServer;


    public static class DbHelperFactory
    {
        public static IDbHelper Create(IServiceProvider sp)
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var tipo = config["TipoConexion"];

            return tipo switch
            {
                "MySql" => sp.GetRequiredService<MySqlServerHelper>(),
                "SqlServer" => sp.GetRequiredService<SqlServerHelper>(),
                _ => throw new Exception("TipoConexion no soportado")
            };
        }
    }
