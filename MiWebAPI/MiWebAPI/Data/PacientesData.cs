using MiWebAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace MiWebAPI.Data
{
    public class PacientesData
    {
        private readonly string _conexion;
        private readonly ILogger<PacientesData> _logger;

        public PacientesData(IConfiguration configuration, ILogger<PacientesData> logger)
        {
            _conexion = configuration.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException(
                    "Falta la cadena de conexión 'CadenaSQL' en appsettings.json");
            _logger = logger;
        }

        public async Task<RespuestaOperacion> Registrar(PacienteRegistro p)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(p.Password);

            try
            {
                await using var con = new SqlConnection(_conexion);
                await using var cmd = new SqlCommand("sp_registrarPaciente", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Correo", p.Correo.Trim());
                cmd.Parameters.AddWithValue("@PasswordHash", hash);
                cmd.Parameters.AddWithValue("@Nombre", p.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Apellidos", p.Apellidos.Trim());

                var numeroError = new SqlParameter("@numeroError", SqlDbType.Int)
                { Direction = ParameterDirection.Output };
                var respuesta = new SqlParameter("@respuesta", SqlDbType.VarChar, 1000)
                { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(numeroError);
                cmd.Parameters.Add(respuesta);

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return new RespuestaOperacion
                {
                    NumeroError = numeroError.Value is int n ? n : 0,
                    Respuesta = respuesta.Value as string ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar sp_registrarPaciente");
                return new RespuestaOperacion
                {
                    NumeroError = -1,
                    Respuesta = "Ocurrió un error al procesar la solicitud"
                };
            }
        }
    }
}