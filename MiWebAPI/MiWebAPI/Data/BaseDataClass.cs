using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiWebAPI.Models;
using System.Data;

namespace MiWebAPI.Data
{
    /// <summary>
    /// Base común de las clases de acceso a datos: cadena de conexión,
    /// ejecución de procedures con @numeroError/@respuesta y lectores seguros.
    /// </summary>
    public abstract class BaseData
    {
        protected readonly string Conexion;
        private readonly ILogger _logger;

        protected BaseData(IConfiguration configuration, ILogger logger)
        {
            Conexion = configuration.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException(
                    "Falta la cadena de conexión 'CadenaSQL' en appsettings.json");
            _logger = logger;
        }

        protected static SqlCommand Comando(SqlConnection con, string procedimiento) =>
            new(procedimiento, con) { CommandType = CommandType.StoredProcedure };

        protected async Task<RespuestaOperacion> Ejecutar(string procedimiento, Action<SqlParameterCollection> parametros)
        {
            try
            {
                await using var con = new SqlConnection(Conexion);
                await using var cmd = Comando(con, procedimiento);

                parametros(cmd.Parameters);

                var numeroError = new SqlParameter("@numeroError", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                var respuesta = new SqlParameter("@respuesta", SqlDbType.VarChar, 1000)
                {
                    Direction = ParameterDirection.Output
                };
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
                _logger.LogError(ex, "Error al ejecutar {Procedimiento}", procedimiento);
                return new RespuestaOperacion
                {
                    NumeroError = -1,
                    Respuesta = "Ocurrió un error al procesar la solicitud"
                };
            }
        }

        // ---- Lectores tolerantes a NULL y a cambios de tipo numérico ----
        protected static string? Txt(SqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i));
        }

        protected static int? Ent(SqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i) ? null : Convert.ToInt32(r.GetValue(i));
        }

        protected static decimal? Dec(SqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i) ? null : Convert.ToDecimal(r.GetValue(i));
        }

        protected static bool Bool(SqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return !r.IsDBNull(i) && Convert.ToBoolean(r.GetValue(i));
        }

        protected static Guid? Gid(SqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i) ? null : r.GetGuid(i);
        }

        protected static DateTime Fecha(SqlDataReader r, string c) =>
            r.GetDateTime(r.GetOrdinal(c));

        protected static string Hora(SqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i) ? string.Empty : ((TimeSpan)r.GetValue(i)).ToString(@"hh\:mm");
        }
    }
}