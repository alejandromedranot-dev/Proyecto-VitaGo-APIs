using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiWebAPI.Data;
using MiWebAPI.Models;
using MiWebAPI.Services;
using System.Security.Claims;

namespace MiWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Se compara contra este hash cuando el correo no existe, para que
        // la respuesta tarde lo mismo y no se pueda averiguar qué correos están registrados
        private static readonly string HashFalso =
            BCrypt.Net.BCrypt.HashPassword("contrasena-falsa-para-igualar-tiempos");

        private readonly AuthData _authData;
        private readonly JwtService _jwt;

        public AuthController(AuthData authData, JwtService jwt)
        {
            _authData = authData;
            _jwt = jwt;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            UsuarioLogin? u = await _authData.ObtenerPorCorreo(req.Correo.Trim());

            bool valida = Verificar(req.Password, u?.PasswordHash ?? HashFalso);

            if (u is null || u.PasswordHash is null || !valida)
                return Unauthorized(new RespuestaOperacion
                {
                    NumeroError = 401,
                    Respuesta = "Correo o contraseña incorrectos"
                });

            var (token, expira) = _jwt.Generar(u);

            return Ok(new LoginRespuesta
            {
                Token = token,
                ExpiraEn = expira,
                UsuarioId = u.UsuarioId,
                PerfilId = u.PerfilId,
                Rol = u.Rol,
                Nombre = u.Nombre,
                Correo = u.Correo
            });
        }

        // GET api/auth/yo  (sirve para comprobar que el token funciona)
        [Authorize]
        [HttpGet("yo")]
        public IActionResult Yo() => Ok(new
        {
            UsuarioId = User.FindFirstValue("sub"),
            Rol = User.FindFirstValue("role"),
            PerfilId = User.FindFirstValue("perfil_id")
        });

        private static bool Verificar(string password, string hash)
        {
            try { return BCrypt.Net.BCrypt.Verify(password, hash); }
            catch (Exception) { return false; } // hash inválido (p. ej. datos de ejemplo)
        }
    }
}