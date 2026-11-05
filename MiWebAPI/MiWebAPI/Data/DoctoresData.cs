using MiWebAPI.Models;
using System.Data;
using System.Data.SqlClient; // O Microsoft.Data.SqlClient según el paquete que estés usando

namespace MiWebAPI.Data
{
    public class DoctoresData
    {
        private readonly string conexion;

        // Inyectamos IConfiguration para leer la cadena de conexión desde appsettings.json
        public DoctoresData(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("CadenaSQL");
        }

        // Método para listar los registros usando el Stored Procedure
        public async Task<List<Doctor>> Lista()
        {
            List<Doctor> lista = new List<Doctor>();

            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();

                // Asegúrate de poner aquí el nombre exacto de tu Stored Procedure en SQL Server
                using (SqlCommand cmd = new SqlCommand("SP_ListaEmpleados", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Doctor
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : string.Empty,
                                Apellido = reader["Apellido"] != DBNull.Value ? reader["Apellido"].ToString() : string.Empty,
                                Especialidad = reader["Especialidad"] != DBNull.Value ? reader["Especialidad"].ToString() : string.Empty,
                                Correo = reader["Correo"] != DBNull.Value ? reader["Correo"].ToString() : string.Empty,
                                Telefono = Convert.ToInt32(reader["Telefono"])
                            });
                        }

                    }

                }

                return lista;
            }
        }
        public async Task<Doctor> Obtener(int Id)
        {
            Doctor objeto = new Doctor();

            using (var con = new SqlConnection(conexion))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("sp_obtenerEmpleado", con);
                cmd.Parameters.AddWithValue("@IdEmpleado", Id);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        objeto = new Doctor
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Apellido = reader["Apellido"].ToString(),
                            Especialidad = reader["Especialidad"].ToString(),
                            Correo = reader["Correo"].ToString(),
                            Telefono = Convert.ToInt32(reader["Telefono"])
                        };
                    }
                }
            }
            return objeto;

        }
        public async Task<RespuestaOperacion> Crear(Doctor objeto)
        {
            using (var con = new SqlConnection(conexion))
            {
                using (SqlCommand cmd = new SqlCommand("sp_crearDoctor", con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Apellido", objeto.Apellido ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Especialidad", objeto.Especialidad ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Correo", objeto.Correo ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Telefono", objeto.Telefono);

                    SqlParameter numeroError = new SqlParameter("@numeroError", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    SqlParameter respuesta = new SqlParameter("@respuesta", SqlDbType.VarChar, 1000)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(numeroError);
                    cmd.Parameters.Add(respuesta);
                    cmd.CommandType = CommandType.StoredProcedure;

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    return new RespuestaOperacion
                    {
                        NumeroError = numeroError.Value != DBNull.Value
                            ? Convert.ToInt32(numeroError.Value)
                            : 0,

                        Respuesta = respuesta.Value != DBNull.Value
                            ? respuesta.Value.ToString() ?? string.Empty
                            : string.Empty
                    };
                }
            }
        }
        public async Task<RespuestaOperacion> Editar(Doctor objeto)
        {
            using (var con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("sp_editarDoctor", con);
                cmd.Parameters.AddWithValue("@IdDoctor", objeto.Id);
                cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", objeto.Apellido);
                cmd.Parameters.AddWithValue("@Especialidad", objeto.Especialidad);
                cmd.Parameters.AddWithValue("@Correo", objeto.Correo);
                cmd.Parameters.AddWithValue("@Telefono", objeto.Telefono);
                SqlParameter numeroError = new SqlParameter("@numeroError", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                SqlParameter respuesta = new SqlParameter("@respuesta", SqlDbType.VarChar, 1000)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(numeroError);
                cmd.Parameters.Add(respuesta);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    return new RespuestaOperacion
                    {
                        NumeroError = Convert.ToInt32(numeroError.Value),
                        Respuesta = respuesta.Value?.ToString() ?? string.Empty
                    };
                }
                catch (Exception ex)
                {
                    return new RespuestaOperacion
                    {
                        NumeroError = -1,
                        Respuesta = ex.Message
                    };
                }
            }
        }

        public async Task<RespuestaOperacion> Eliminar(int id)
        {
            using (var con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminarEmpleado", con);
                cmd.Parameters.AddWithValue("@IdEmpleado", id);
                SqlParameter numeroError = new SqlParameter("@numeroError", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                SqlParameter respuesta = new SqlParameter("@respuesta", SqlDbType.VarChar, 1000)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(numeroError);
                cmd.Parameters.Add(respuesta);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    return new RespuestaOperacion
                    {
                        NumeroError = Convert.ToInt32(numeroError.Value),
                        Respuesta = respuesta.Value?.ToString() ?? string.Empty
                    };
                }
                catch (Exception ex)
                {
                    return new RespuestaOperacion
                    {
                        NumeroError = -1,
                        Respuesta = ex.Message
                    };
                }
            }
        }

    }
}