using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using APIAlturas.Commons;
using APIAlturas.DAO;
using APIAlturas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace APIAlturas.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        [AllowAnonymous]
        [HttpPost]
        public object Post(
            [FromBody] User user,
            [FromServices] UsersDAO usersDAO,
            [FromServices] SigningConfigurations signingConfigurations,
            [FromServices] TokenConfigurations tokenConfigurations)
        {
            bool validCredentials = false;

            if (user != null && !String.IsNullOrWhiteSpace(user.UserID))
            {
                var userBase = usersDAO.Find(user.UserID);
                validCredentials = (userBase != null &&
                                      user.UserID == userBase.UserID &&
                                      user.AccessKey == userBase.AccessKey);
            }

            if (!validCredentials)
            {
                return new
                {
                    authenticated = false,
                    message = "Falha ao autenticar"
                };
            }

            var identity = new ClaimsIdentity(
                new GenericIdentity(user.UserID, "Login"),
                new[] {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserID)
                }
            );

            var dataCriacao = DateTime.Now;
            var dataExpiracao = dataCriacao +
                                     TimeSpan.FromSeconds(tokenConfigurations.Seconds);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(
                new SecurityTokenDescriptor
                {
                    Issuer = tokenConfigurations.Issuer,
                    Audience = tokenConfigurations.Audience,
                    SigningCredentials = signingConfigurations.SigningCredentials,
                    Subject = identity,
                    NotBefore = dataCriacao,
                    Expires = dataExpiracao
                });
            var token = handler.WriteToken(securityToken);

            return new
            {
                authenticated = true,
                created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                expiration = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                accessToken = token,
                message = "OK"
            };
        }
    }
}
