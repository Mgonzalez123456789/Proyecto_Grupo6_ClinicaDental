﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicaDental.Controllers
{
    public class LoginController : Controller
    {
        private readonly Config _configuracion;
        private ILoginRepositorio _loginRepositorio;
        private IUsuarioRepositorio _usuarioRepositorio;

        public LoginController(Config config)
        {
            _configuracion = config;
            _loginRepositorio = new LoginRepositorio(config.CadenaConexion);
            _usuarioRepositorio = new UsuarioRepositorio(config.CadenaConexion);
        }

        [HttpPost("/account/login")]

        public async Task<IActionResult> Login(Login login)

        {
            string rol = string.Empty;
            try
            {
                bool usuarioValido = await _loginRepositorio.ValidarUsuario(login);

                if (usuarioValido)
                {
                    Usuario user = await _usuarioRepositorio.GetPorCodigo(login.CodigoUsuario);

                    if (user.EstaActivo)
                    {
                        rol = user.Rol;

                        var claims = new[]
                        {
                            //Codigo para iniciar sesion
                            new Claim(ClaimTypes.Name, user.Codigo),
                            new Claim(ClaimTypes.Role, rol)
                        };

                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddMinutes(20) });

                    }
                    else
                    {
                        return LocalRedirect("/login/El usuario no esta activo");
                    }
                }
                else
                {
                    return LocalRedirect("/login/Datos de usuarios Invalidos");
                }
            }
            catch (Exception ex)
            {
            }
            return LocalRedirect("/");
        }

        [HttpGet("/account/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/login");
        }
    }
}
