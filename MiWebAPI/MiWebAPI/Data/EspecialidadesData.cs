using MiWebAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace MiWebAPI.Data
{
    public class EspecialidadesData
    {
        private readonly string _conexion;

        public EspecialidadesData(IConfiguration configuration)
        {
            _conexion = configuration.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException(
                    "Falta la cadena de conexión 'CadenaSQL' en appsettings.json");
        }

        public async Task<List<Especialidad>> Lista()
        {
            var lista = new List<Especialidad>();

            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_listaEspecialidades", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            await using var reader = await cmd.ExecuteReaderAsync();
            int iId = reader.GetOrdinal("Id");
            int iNombre = reader.GetOrdinal("Nombre");
            int iDesc = reader.GetOrdinal("Descripcion");
            int iIcono = reader.GetOrdinal("Icono");

            while (await reader.ReadAsync())
            {
                lista.Add(new Especialidad
                {
                    Id = reader.GetInt32(iId),
                    Nombre = reader.IsDBNull(iNombre) ? null : reader.GetString(iNombre),
                    Descripcion = reader.IsDBNull(iDesc) ? null : reader.GetString(iDesc),
                    Icono = reader.IsDBNull(iIcono) ? null : reader.GetString(iIcono)
                });
            }

            return lista;
        }
    }
}