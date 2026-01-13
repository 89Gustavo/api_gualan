using MySqlConnector;

namespace api_gualan.Helpers
{
    public static class MySqlParameterHelper
    {
        public static MySqlParameter Create(string name, object? value)
        {
            return new MySqlParameter(name, value ?? DBNull.Value);
        }
    }
}
