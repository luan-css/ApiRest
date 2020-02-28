using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsuarioController: ControllerBase
    {

        private readonly ApplicationDbContext database;

        public UsuarioController(ApplicationDbContext database){
            this.database = database;
        }

        [HttpPost("registro")]
        public IActionResult Registro([FromBody] Usuario usuario){
            database.Add(usuario);
            database.SaveChanges();
            return Ok(new {msg="Usuario cadastrado com sucesso"});
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] Usuario Credenciais){
            
            try{            
            Usuario usuario = database.Usuarios.First(user => user.Email.Equals(Credenciais.Email));
            if(usuario != null){
                if(usuario.Senha.Equals(Credenciais.Senha)){
                    string chave = "luanzaqwsxedcvfrtgbhh";
                    var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave));
                    var credenciaisDeAcesso = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256Signature);
                
                    var claims = new List<Claim>();
                    claims.Add(new Claim("id", usuario.Id.ToString()));
                    claims.Add(new Claim("email", usuario.Email.ToString()));
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    
                    var JWT = new JwtSecurityToken(
                        issuer: "Luan.com", /* Quem fornece o jwt para o usuario*/
                        expires: DateTime.Now.AddHours(1),
                        audience: "Normal",
                        signingCredentials: credenciaisDeAcesso,
                        claims: claims
                    );

                    return Ok(new JwtSecurityTokenHandler().WriteToken(JWT));
                }else{
                    Response.StatusCode = 401;
                    return new ObjectResult("");
                }
            }else{
                Response.StatusCode = 401;
                return new ObjectResult("");
            }
            }catch(Exception e){
                Response.StatusCode = 401;
                return new ObjectResult("");
            }

        }
    }
}