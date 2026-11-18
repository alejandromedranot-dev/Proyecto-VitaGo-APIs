using System.Data;
using Microsoft.Data.SqlClient;
using MiWebAPI.Models;

namespace MiWebAPI.Data
{
    public class UsuariosData : BaseData
    {
        public UsuariosData(IConfiguration configuration, ILogger<UsuariosData> logger)
            : base(configuration, logger) { }

        // Busca el usuario y devuelve su tupla (UsuarioSesion, HashDeContrasena)
        public async Task<(UsuarioSesion? usuario, string? hash)> ObtenerParaLogin(string correo)
        {
            await using var con = new SqlConnection(Conexion);
            await con.OpenAsync();
            await using var cmd = Comando(con, "sp_obtenerUsuarioPorCorreo"); // Reemplaza con tu SP real
            cmd.Parameters.AddWithValue("@Correo", correo);

            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return (null, null);

            var usuario = new UsuarioSesion
            {
                Id = r.GetGuid(r.GetOrdinal("Id")),
                Nombre = Txt(r, "Nombre"),
                Correo = Txt(r, "Correo"),
                Rol = Txt(r, "Rol"),
                PerfilId = r.IsDBNull(r.GetOrdinal("PerfilId")) ? null : r.GetGuid(r.GetOrdinal("PerfilId"))
            };

            var hash = Txt(r, "PasswordHash");
            return (usuario, hash);
        }

        public async Task<RespuestaOperacion> RegistrarPaciente(RegistroPacienteRequest request)
        {
            // Aquí ejecutas tu procedimiento almacenado de registro usando tu método Ejecutar o SqlCommand
            // y retornas el objeto RespuestaOperacion con el @numeroError y @respuesta que arroje la BD.
            throw new NotImplementedException();
        }
    }
}