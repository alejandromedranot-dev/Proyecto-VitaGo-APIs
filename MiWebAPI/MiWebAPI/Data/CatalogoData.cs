using Microsoft.Data.SqlClient;
using MiWebAPI.Models;

namespace MiWebAPI.Data
{
    public class CatalogoData : BaseData
    {
        public CatalogoData(IConfiguration configuration, ILogger<CatalogoData> logger)
            : base(configuration, logger) { }

        public async Task<List<Especialidad>> Especialidades()
        {
            var lista = new List<Especialidad>();

            await using var con = new SqlConnection(Conexion);
            await con.OpenAsync();
            await using var cmd = Comando(con, "sp_listaEspecialidades");
            await using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
                lista.Add(new Especialidad
                {
                    Id = Ent(r, "Id") ?? 0,
                    Nombre = Txt(r, "Nombre"),
                    Descripcion = Txt(r, "Descripcion"),
                    Icono = Txt(r, "Icono")
                });

            return lista;
        }

        public async Task<List<MedicoResumen>> Buscar(string? especialidad, string? ciudad, bool soloVideo)
        {
            var lista = new List<MedicoResumen>();

            await using var con = new SqlConnection(Conexion);
            await con.OpenAsync();
            await using var cmd = Comando(con, "sp_buscarMedicos");
            cmd.Parameters.AddWithValue("@Especialidad", (object?)especialidad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Ciudad", (object?)ciudad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SoloVideo", soloVideo);

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                lista.Add(new MedicoResumen
                {
                    Id = r.GetGuid(r.GetOrdinal("Id")),
                    Nombre = Txt(r, "Nombre"),
                    Apellido = Txt(r, "Apellido"),
                    Especialidad = Txt(r, "Especialidad"),
                    AniosExperiencia = Ent(r, "AniosExperiencia"),
                    Rating = Dec(r, "Rating"),
                    TotalResenas = Ent(r, "TotalResenas") ?? 0,
                    PrecioConsulta = Dec(r, "PrecioConsulta") ?? 0,
                    ConsultaOnline = Bool(r, "ConsultaOnline"),
                    Verificado = Bool(r, "Verificado"),
                    FotoUrl = Txt(r, "FotoUrl"),
                    Consultorio = Txt(r, "Consultorio"),
                    Ciudad = Txt(r, "Ciudad")
                });

            return lista;
        }

        /// <summary>Perfil completo. El procedure devuelve 5 result sets en orden.</summary>
        
    }
}
