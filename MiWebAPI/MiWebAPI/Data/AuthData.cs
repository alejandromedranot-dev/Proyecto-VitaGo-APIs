using MiWebAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace MiWebAPI.Data
{
    public class AuthData
    {
        private readonly string _conexion;

        public AuthData(IConfiguration configuration)
        {
            _conexion = configuration.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException(
                    "Falta la cadena de conexión 'CadenaSQL' en appsettings.json");
        }

        public async Task<UsuarioLogin?> ObtenerPorCorreo(string correo)
        {
            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_loginUsuario", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@Correo", SqlDbType.NVarChar, 254) { Value = correo });

            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;

            int iHash = r.GetOrdinal("PasswordHash");
            int iNombre = r.GetOrdinal("Nombre");
            int iPerfil = r.GetOrdinal("PerfilId");

            return new UsuarioLogin
            {
                UsuarioId = r.GetGuid(r.GetOrdinal("UsuarioId")),
                Correo = r.GetString(r.GetOrdinal("Correo")),
                PasswordHash = r.IsDBNull(iHash) ? null : r.GetString(iHash),
                Rol = r.GetString(r.GetOrdinal("Rol")),
                Nombre = r.IsDBNull(iNombre) ? null : r.GetString(iNombre),
                PerfilId = r.IsDBNull(iPerfil) ? null : r.GetGuid(iPerfil)
            };
        }
    }
}