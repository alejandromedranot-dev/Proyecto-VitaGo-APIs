using Microsoft.AspNetCore.Mvc;
using MiWebAPI.Data;
using MiWebAPI.Models;

namespace MiWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctoresController : ControllerBase
    {
        private readonly DoctoresData _doctoresData;

        public DoctoresController(DoctoresData doctoresData)
        {
            _doctoresData = doctoresData;
        }

        // GET api/doctores
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<Doctor> lista = await _doctoresData.Lista();
            return Ok(lista);
        }

        // GET api/doctores/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Obtener(Guid id)
        {
            Doctor? doctor = await _doctoresData.Obtener(id);

            if (doctor is null)
                return NotFound(new RespuestaOperacion { NumeroError = 404, Respuesta = "El doctor no existe" });

            return Ok(doctor);
        }

        // POST api/doctores
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Doctor objeto)
        {
            if (string.IsNullOrWhiteSpace(objeto.Password) || objeto.Password.Length < 8)
                return BadRequest(new RespuestaOperacion
                {
                    NumeroError = 400,
                    Respuesta = "La contraseña es obligatoria y debe tener al menos 8 caracteres"
                });

            RespuestaOperacion resultado = await _doctoresData.Crear(objeto);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }

        // PUT api/doctores/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Editar(Guid id, [FromBody] Doctor objeto)
        {
            objeto.Id = id;
            RespuestaOperacion resultado = await _doctoresData.Editar(objeto);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }

        // DELETE api/doctores/{id}  (baja lógica)
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            RespuestaOperacion resultado = await _doctoresData.Eliminar(id);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }
    }
}