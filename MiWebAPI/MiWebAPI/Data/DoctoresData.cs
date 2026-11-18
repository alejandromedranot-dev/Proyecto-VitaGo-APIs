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

        public async Task<MedicoPerfil?> Perfil(Guid id)
        {
            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_perfilMedico", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdDoctor", id);

            await using var r = await cmd.ExecuteReaderAsync();

            // 1) Datos generales. Sin fila = no existe o no está aprobado
            if (!await r.ReadAsync()) return null;

            int iRating = r.GetOrdinal("Rating");
            var perfil = new MedicoPerfil
            {
                Id = r.GetGuid(r.GetOrdinal("Id")),
                Nombre = Texto(r, "Nombre"),
                Apellido = Texto(r, "Apellido"),
                Especialidad = Texto(r, "Especialidad"),
                Bio = Texto(r, "Bio"),
                AniosExperiencia = Entero(r, "AniosExperiencia"),
                NumPacientes = Entero(r, "NumPacientes"),
                Idiomas = Texto(r, "Idiomas"),
                PrecioConsulta = r.GetDecimal(r.GetOrdinal("PrecioConsulta")),
                ConsultaOnline = r.GetBoolean(r.GetOrdinal("ConsultaOnline")),
                Verificado = r.GetBoolean(r.GetOrdinal("Verificado")),
                Rating = r.IsDBNull(iRating) ? null : Convert.ToDecimal(r[iRating]),
                TotalResenas = Entero(r, "TotalResenas"),
                Telefono = Texto(r, "Telefono"),
                FotoUrl = Texto(r, "FotoUrl")
            };

            // 2) Especialidades (la principal primero)
            await r.NextResultAsync();
            while (await r.ReadAsync())
                perfil.Especialidades.Add(r.GetString(0));

            // 3) Consultorios activos
            await r.NextResultAsync();
            while (await r.ReadAsync())
            {
                int iLat = r.GetOrdinal("Latitud");
                int iLng = r.GetOrdinal("Longitud");

                perfil.Consultorios.Add(new ConsultorioPublico
                {
                    Id = r.GetGuid(r.GetOrdinal("Id")),
                    Nombre = Texto(r, "Nombre"),
                    Direccion = Texto(r, "Direccion"),
                    Ciudad = Texto(r, "Ciudad"),
                    Estado = Texto(r, "Estado"),
                    Cp = Texto(r, "Cp"),
                    Latitud = r.IsDBNull(iLat) ? null : Convert.ToDecimal(r[iLat]),
                    Longitud = r.IsDBNull(iLng) ? null : Convert.ToDecimal(r[iLng])
                });
            }

            // 4) Horarios por día
            await r.NextResultAsync();
            while (await r.ReadAsync())
            {
                perfil.Horarios.Add(new HorarioDia
                {
                    DiaSemana = r.GetByte(r.GetOrdinal("DiaSemana")),
                    HoraInicio = r.GetFieldValue<TimeSpan>(r.GetOrdinal("HoraInicio")).ToString(@"hh\:mm"),
                    HoraFin = r.GetFieldValue<TimeSpan>(r.GetOrdinal("HoraFin")).ToString(@"hh\:mm")
                });
            }

            // 5) Últimas 10 reseñas
            await r.NextResultAsync();
            while (await r.ReadAsync())
            {
                perfil.Resenas.Add(new ResenaPublica
                {
                    Autor = Texto(r, "Autor"),
                    Rating = Convert.ToInt32(r["Rating"]),
                    Comentario = Texto(r, "Comentario"),
                    Fecha = r.GetDateTime(r.GetOrdinal("Fecha"))
                });
            }

            return perfil;
        }

        public async Task<List<MedicoBusqueda>> Buscar(string? especialidad, string? ciudad, bool soloVideo)
        {
            var lista = new List<MedicoBusqueda>();

            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_buscarMedicos", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@Especialidad", SqlDbType.NVarChar, 80)
            {
                Value = string.IsNullOrWhiteSpace(especialidad) ? DBNull.Value : especialidad.Trim()
            });
            cmd.Parameters.Add(new SqlParameter("@Ciudad", SqlDbType.NVarChar, 80)
            {
                Value = string.IsNullOrWhiteSpace(ciudad) ? DBNull.Value : ciudad.Trim()
            });
            cmd.Parameters.Add(new SqlParameter("@SoloVideo", SqlDbType.Bit) { Value = soloVideo });

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                lista.Add(MapearBusqueda(reader));

            return lista;
        }

        // ---------- Escrituras ----------

        public Task<RespuestaOperacion> Crear(DoctorRegistro d)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(d.Password);
            var c = d.Consultorio;

            return Ejecutar("sp_crearDoctor", p =>
            {
                p.AddWithValue("@Nombre", d.Nombre);
                p.AddWithValue("@Apellido", d.Apellidos);
                p.AddWithValue("@Especialidad", d.Especialidad);
                p.AddWithValue("@Cedula", d.Cedula);
                p.AddWithValue("@Correo", d.Correo);
                p.AddWithValue("@Telefono", (object?)d.Telefono ?? DBNull.Value);
                p.AddWithValue("@PrecioConsulta", d.PrecioConsulta);
                p.AddWithValue("@PasswordHash", hash);

                p.AddWithValue("@Consultorio", c.Nombre);
                p.AddWithValue("@Direccion", c.Direccion);
                p.AddWithValue("@Ciudad", c.Ciudad);
                p.AddWithValue("@Estado", (object?)c.Estado ?? DBNull.Value);
                p.AddWithValue("@Cp", (object?)c.Cp ?? DBNull.Value);
                p.AddWithValue("@Latitud", (object?)c.Latitud ?? DBNull.Value);
                p.AddWithValue("@Longitud", (object?)c.Longitud ?? DBNull.Value);

                p.AddWithValue("@DiaInicio", c.DiaInicio);
                p.AddWithValue("@DiaFin", c.DiaFin);
                p.AddWithValue("@HoraInicio", TimeSpan.Parse(c.HoraInicio));
                p.AddWithValue("@HoraFin", TimeSpan.Parse(c.HoraFin));
                p.AddWithValue("@DuracionSlotMin", c.DuracionSlotMin);

                p.AddWithValue("@Bio", (object?)d.Bio ?? DBNull.Value);
                p.AddWithValue("@AniosExperiencia", (object?)d.AniosExperiencia ?? DBNull.Value);
                p.AddWithValue("@Idiomas", (object?)d.Idiomas ?? DBNull.Value);
                p.AddWithValue("@ConsultaOnline", d.ConsultaOnline);
                p.AddWithValue("@FotoUrl", (object?)d.FotoUrl ?? DBNull.Value);
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

        private static MedicoBusqueda MapearBusqueda(SqlDataReader r)
        {
            int iRating = r.GetOrdinal("Rating");

            return new MedicoBusqueda
            {
                Id = r.GetGuid(r.GetOrdinal("Id")),
                Nombre = Texto(r, "Nombre"),
                Apellido = Texto(r, "Apellido"),
                Especialidad = Texto(r, "Especialidad"),
                AniosExperiencia = Entero(r, "AniosExperiencia"),
                Rating = r.IsDBNull(iRating) ? null : Convert.ToDecimal(r[iRating]),
                TotalResenas = Entero(r, "TotalResenas"),
                PrecioConsulta = r.GetDecimal(r.GetOrdinal("PrecioConsulta")),
                ConsultaOnline = r.GetBoolean(r.GetOrdinal("ConsultaOnline")),
                Verificado = r.GetBoolean(r.GetOrdinal("Verificado")),
                FotoUrl = Texto(r, "FotoUrl"),
                Consultorio = Texto(r, "Consultorio"),
                Ciudad = Texto(r, "Ciudad")
            };
        }

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

        public async Task<List<SlotDisponible>> Slots(Guid medicoId, DateOnly fecha)
        {
            var lista = new List<SlotDisponible>();

            await using var con = new SqlConnection(_conexion);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("sp_SlotsDisponibles", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@medico_id", SqlDbType.UniqueIdentifier) { Value = medicoId });
            cmd.Parameters.Add(new SqlParameter("@fecha", SqlDbType.Date) { Value = fecha.ToDateTime(TimeOnly.MinValue) });

            await using var r = await cmd.ExecuteReaderAsync();

            if (!r.HasRows) return lista;   

            int iCons = r.GetOrdinal("consultorio_id");
            int iFecha = r.GetOrdinal("fecha_hora");
            int iHora = r.GetOrdinal("hora");
            int iDur = r.GetOrdinal("duracion_min");

            while (await r.ReadAsync())
            {
                lista.Add(new SlotDisponible
                {
                    ConsultorioId = r.GetGuid(iCons),
                    FechaHora = r.GetDateTime(iFecha),
                    Hora = r.GetFieldValue<TimeSpan>(iHora).ToString(@"hh\:mm"),
                    DuracionMin = Convert.ToInt32(r[iDur])
                });
            }

            return lista;
        }
    }
}