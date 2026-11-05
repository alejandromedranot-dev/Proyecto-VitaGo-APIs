using Microsoft.AspNetCore.Mvc;
using MiWebAPI.Data;
using MiWebAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace VitaGo.API.Controllers
{
    [Route("api/[controller]")] 
    public class DoctoresController : ControllerBase
    {
        private readonly  DoctoresData _doctoresData;

        public DoctoresController(DoctoresData doctoresData)
        {
            _doctoresData = doctoresData;
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<Doctor> Lista = await _doctoresData.Lista();
            return StatusCode(StatusCodes.Status200OK, Lista);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            Doctor objeto = await _doctoresData.Obtener(id);
            return StatusCode(StatusCodes.Status200OK, objeto);
        }
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Doctor objeto)
        {
            RespuestaOperacion resultado = await _doctoresData.Crear(objeto);
            return StatusCode(StatusCodes.Status200OK, resultado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Doctor objeto)
        {
            objeto.Id = id;
            RespuestaOperacion resultado = await _doctoresData.Editar(objeto);
            return StatusCode(StatusCodes.Status200OK, resultado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            RespuestaOperacion resultado = await _doctoresData.Eliminar(id);
            return StatusCode(StatusCodes.Status200OK, resultado);
        }
    }
}