using Microsoft.AspNetCore.Mvc;
using MiWebAPI.Data;
using MiWebAPI.Models;

namespace MiWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly PacientesData _data;

        public PacientesController(PacientesData data) => _data = data;

        // POST api/pacientes
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] PacienteRegistro objeto)
        {
            RespuestaOperacion resultado = await _data.Registrar(objeto);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }
    }
}