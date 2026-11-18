using Microsoft.IdentityModel.Tokens;
using MiWebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiWebAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _cfg;

        public JwtService(IConfiguration cfg) => _cfg = cfg;

        public (string Token, DateTime Expira) Generar(UsuarioLogin u)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var expira = DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:ExpiraMinutos"] ?? "60"));

            var claims = new List<Claim>
            {
                new("sub", u.UsuarioId.ToString()),
                new("email", u.Correo),
                new("role", u.Rol)
            };
            if (u.PerfilId is Guid perfil)
                claims.Add(new Claim("perfil_id", perfil.ToString()));

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expira,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return (new JwtSecurityTokenHandler().WriteToken(token), expira);
        }
    }
}