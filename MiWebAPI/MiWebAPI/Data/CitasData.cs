using MiWebAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace MiWebAPI.Data
{
    public class CitasData
    {
        private readonly string _conexion;
        private readonly ILogger<CitasData> _logger;

        public CitasData(IConfiguration configuration, ILogger<CitasData> logger)
        {
            _conexion = configuration.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException(
                    "Falta la cadena de conexión 'CadenaSQL' en appsettings.json");
            _logger = logger;
        }

        public async Task<ResultadoCita> Crear(Guid pacienteId, CitaCrear c)
        {
            try
            {
                // Las citas son a minuto exacto: se descartan segundos y fracciones
                DateTime fecha = c.FechaHora.AddTicks(-(c.FechaHora.Ticks % TimeSpan.TicksPerMinute));

                await using var con = new SqlConnection(_conexion);
                await using var cmd = new SqlCommand("sp_crearCita", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@PacienteId", SqlDbType.UniqueIdentifier) { Value = pacienteId });
                cmd.Parameters.Add(new SqlParameter("@MedicoId", SqlDbType.UniqueIdentifier) { Value = c.MedicoId });
                cmd.Parameters.Add(new SqlParameter("@ConsultorioId", SqlDbType.UniqueIdentifier)
                { Value = (object?)c.ConsultorioId ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@FechaHora", SqlDbType.DateTime2) { Value = fecha });
                cmd.Parameters.Add(new SqlParameter("@Modalidad", SqlDbType.VarChar, 12) { Value = c.Modalidad });
                cmd.Parameters.Add(new SqlParameter("@Motivo", SqlDbType.NVarChar, 1000)
                { Value = (object?)c.Motivo ?? DBNull.Value });

                var idCita = new SqlParameter("@IdCita", SqlDbType.UniqueIdentifier)
                { Direction = ParameterDirection.Output };
                var folio = new SqlParameter("@Folio", SqlDbType.VarChar, 20)
                { Direction = ParameterDirection.Output };
                var numeroError = new SqlParameter("@numeroError", SqlDbType.Int)
                { Direction = ParameterDirection.Output };
                var respuesta = new SqlParameter("@respuesta", SqlDbType.VarChar, 1000)
                { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(idCita);
                cmd.Parameters.Add(folio);
                cmd.Parameters.Add(numeroError);
                cmd.Parameters.Add(respuesta);

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return new ResultadoCita
                {
                    NumeroError = numeroError.Value is int n ? n : 0,
                    Respuesta = respuesta.Value as string ?? string.Empty,
                    IdCita = idCita.Value is Guid g ? g : null,
                    Folio = folio.Value as string
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar sp_crearCita");
                return new ResultadoCita
                {
                    NumeroError = -1,
                    Respuesta = "Ocurrió un error al procesar la solicitud"
                };
            }
        }

        public async Task<List<CitaDetalle>> MisCitas(Guid pacienteId)
        {
            var lista = new List<CitaDetalle>();

            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_citasPaciente", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@PacienteId", SqlDbType.UniqueIdentifier) { Value = pacienteId });

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                lista.Add(MapearCita(r));

            return lista;
        }

        public async Task<CitaDetalle?> Obtener(Guid idCita, Guid pacienteId)
        {
            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_obtenerCita", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@IdCita", SqlDbType.UniqueIdentifier) { Value = idCita });
            cmd.Parameters.Add(new SqlParameter("@PacienteId", SqlDbType.UniqueIdentifier) { Value = pacienteId });

            await using var r = await cmd.ExecuteReaderAsync();
            return await r.ReadAsync() ? MapearCita(r) : null;
        }

        public async Task<RespuestaOperacion> Cancelar(Guid idCita, Guid pacienteId)
        {
            try
            {
                await using var con = new SqlConnection(_conexion);
                await using var cmd = new SqlCommand("sp_cancelarCita", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@IdCita", SqlDbType.UniqueIdentifier) { Value = idCita });
                cmd.Parameters.Add(new SqlParameter("@PacienteId", SqlDbType.UniqueIdentifier) { Value = pacienteId });

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
                _logger.LogError(ex, "Error al ejecutar sp_cancelarCita");
                return new RespuestaOperacion
                {
                    NumeroError = -1,
                    Respuesta = "Ocurrió un error al procesar la solicitud"
                };
            }
        }

        // ---------- Auxiliares ----------

        private static string? Txt(SqlDataReader r, string columna)
        {
            int i = r.GetOrdinal(columna);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        private static CitaDetalle MapearCita(SqlDataReader r) => new()
        {
            Id = r.GetGuid(r.GetOrdinal("Id")),
            Folio = Txt(r, "Folio") ?? string.Empty,
            FechaHora = r.GetDateTime(r.GetOrdinal("FechaHora")),
            DuracionMin = Convert.ToInt32(r["DuracionMin"]),
            Estado = Txt(r, "Estado") ?? string.Empty,
            Costo = r.GetDecimal(r.GetOrdinal("Costo")),
            Moneda = Txt(r, "Moneda") ?? "MXN",
            Modalidad = Txt(r, "Modalidad") ?? string.Empty,
            Motivo = Txt(r, "Motivo"),
            MedicoId = r.GetGuid(r.GetOrdinal("MedicoId")),
            Doctor = Txt(r, "Doctor") ?? string.Empty,
            Especialidad = Txt(r, "Especialidad"),
            Consultorio = Txt(r, "Consultorio"),
            Direccion = Txt(r, "Direccion"),
            Ciudad = Txt(r, "Ciudad"),
            Paciente = Txt(r, "Paciente") ?? string.Empty
        };
    }
}