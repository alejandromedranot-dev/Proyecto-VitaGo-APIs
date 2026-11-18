using Microsoft.AspNetCore.Mvc;
using MiWebAPI.Data;

namespace MiWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecialidadesController : ControllerBase
    {
        private readonly EspecialidadesData _data;

        public EspecialidadesController(EspecialidadesData data) => _data = data;

        // GET api/especialidades
        [HttpGet]
        public async Task<IActionResult> Lista() => Ok(await _data.Lista());
    }
}