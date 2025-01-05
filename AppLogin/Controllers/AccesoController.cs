using Microsoft.AspNetCore.Mvc;
using AppLogin.Data;
using AppLogin.Models;
using Microsoft.EntityFrameworkCore;
using AppLogin.ViewModels;

//seguridad
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AppLogin.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _appDbContext;
        public AccesoController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Registrarse(UsuarioVM modelo)
        {
            if(modelo.Clave != modelo.ConfirmarClave)
            {
                ViewData["Mensaje"] = "Las claves no coinciden";
                return View();
            }

            Usuario usuario = new Usuario()
            {
                NombreUsuario = modelo.NombreUsuario,
                Correo = modelo.Correo,
                Clave = modelo.Clave
            };

            await _appDbContext.Usuarios.AddAsync(usuario);
            await _appDbContext.SaveChangesAsync();

            if(usuario.IdUsuario != 0) return RedirectToAction("Login", "Acceso");
            ViewData["Mensaje"] = "No se pudo crear el usuario";

            return View();
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM modelo)
        {
            Usuario? usuario_encontrado = await _appDbContext.Usuarios
                .Where(u => 
                u.NombreUsuario == modelo.NombreUsuario && u.Clave == modelo.Clave)
                .FirstOrDefaultAsync();

            if(usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "No se encuentran coincidencias";
                return View();
            }

            //guardar la info del usuario
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, usuario_encontrado.NombreUsuario)
            };

            ClaimsIdentity claimsldentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsldentity),
            properties
            );
            
            //fin

            return RedirectToAction("Index", "Home");

        }
    }
}
