using MiWebAPI.Models;
using System.Data;
using System.Data.SqlClient;
using BCrypt.Net;

namespace MiWebAPI.Data
{
    public class DoctoresData
    {
        private readonly string _conexion;
        private readonly ILogger<DoctoresData> _logger;

        public DoctoresData(IConfiguration configuration, ILogger<DoctoresData> logger)
        {
            _conexion = configuration.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException(
                    "Falta la cadena de conexión 'CadenaSQL' en appsettings.json");
            _logger = logger;
        }

        // ---------- Lecturas ----------

        public async Task<List<Doctor>> Lista()
        {
            var lista = new List<Doctor>();

            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_listarDoctores", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                lista.Add(Mapear(reader));

            return lista;
        }

        public async Task<Doctor?> Obtener(Guid id)
        {
            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_obtenerDoctor", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdDoctor", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Mapear(reader) : null;
        }

        // ---------- Escrituras ----------

        public Task<RespuestaOperacion> Crear(Doctor d)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(d.Password);

            return Ejecutar("sp_crearDoctor", p =>
            {
                p.AddWithValue("@Nombre", d.Nombre ?? string.Empty);
                p.AddWithValue("@Apellido", d.Apellidos ?? string.Empty); // <--- Cambiado a @Apellido
                p.AddWithValue("@Especialidad", d.Especialidad ?? string.Empty);
                p.AddWithValue("@Cedula", d.Cedula ?? string.Empty);
                p.AddWithValue("@Correo", d.Correo ?? string.Empty);
                p.AddWithValue("@Telefono", (object?)d.Telefono ?? DBNull.Value);
                p.AddWithValue("@PrecioConsulta", d.PrecioConsulta);
                p.AddWithValue("@PasswordHash", hash);
            });
        }

        public Task<RespuestaOperacion> Editar(Doctor d) =>
            Ejecutar("sp_editarDoctor", p =>
            {
                p.AddWithValue("@IdDoctor", d.Id);
                p.AddWithValue("@Nombre", d.Nombre ?? string.Empty);
                //p.AddWithValue("@Apellido", d.Apellido ?? string.Empty);
                p.AddWithValue("@Especialidad", d.Especialidad ?? string.Empty);
                p.AddWithValue("@Cedula", d.Cedula ?? string.Empty);
                p.AddWithValue("@Correo", d.Correo ?? string.Empty);
                p.AddWithValue("@Telefono", (object?)d.Telefono ?? DBNull.Value);
                p.AddWithValue("@PrecioConsulta", d.PrecioConsulta);
            });

        public Task<RespuestaOperacion> Eliminar(Guid id) =>
            Ejecutar("sp_eliminarDoctor", p => p.AddWithValue("@IdDoctor", id));

        // ---------- Auxiliares ----------

        /// <summary>
        /// Ejecuta un procedure con los parámetros de salida estándar
        /// (@numeroError y @respuesta) y devuelve un RespuestaOperacion.
        /// </summary>
        private async Task<RespuestaOperacion> Ejecutar(string procedimiento, Action<SqlParameterCollection> parametros)
        {
            try
            {
                await using var con = new SqlConnection(_conexion);
                await using var cmd = new SqlCommand(procedimiento, con)
                {
                    CommandType = CommandType.StoredProcedure
                };

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
                // El detalle técnico se queda en el log, no se expone al cliente
                _logger.LogError(ex, "Error al ejecutar {Procedimiento}", procedimiento);
                return new RespuestaOperacion
                {
                    NumeroError = -1,
                    Respuesta = "Ocurrió un error al procesar la solicitud"
                };
            }
        }

        private static Doctor Mapear(SqlDataReader r) => new()
        {
            Id = r.GetGuid(r.GetOrdinal("id")),
            UsuarioId = r.GetGuid(r.GetOrdinal("usuario_id")),
            Nombre = Texto(r, "nombre"),
            Apellidos = Texto(r, "apellidos"),
            Especialidad = Texto(r, "Especialidad"),
            Cedula = Texto(r, "cedula"),
            Correo = Texto(r, "correo"),
            Telefono = Texto(r, "telefono"),
            Bio = Texto(r, "bio"),
            AniosExperiencia = Entero(r, "anios_experiencia"),
            NumPacientes = Entero(r, "num_pacientes"),
            Idiomas = Texto(r, "idiomas"),
            PrecioConsulta = r.GetDecimal(r.GetOrdinal("precio_consulta")),
            ConsultaOnline = r.GetBoolean(r.GetOrdinal("consulta_online")),
            Verificado = r.GetBoolean(r.GetOrdinal("verificado")),
            EstadoRegistro = Texto(r, "estado_registro"),
            RatingPromedio = r.IsDBNull(r.GetOrdinal("rating_promedio")) ? null : r.GetDecimal(r.GetOrdinal("rating_promedio")),
            TotalResenas = Entero(r, "total_resenas"),
            FotoUrl = Texto(r, "foto_url")
        };

        private static int Entero(SqlDataReader r, string columna)
        {
            int i = r.GetOrdinal(columna);
            return r.IsDBNull(i) ? 0 : Convert.ToInt32(r[i]);
        }

        private static string? Texto(SqlDataReader r, string columna)
        {
            int i = r.GetOrdinal(columna);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }
    }
}