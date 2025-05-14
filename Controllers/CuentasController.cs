﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoIdentity.Models;
using System.Runtime.InteropServices;

namespace ProyectoIdentity.Controllers
{
    [Authorize]
    public class CuentasController : Controller
    {
        private readonly UserManager<IdentityUser>_userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public CuentasController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager= roleManager;
            _signInManager = signInManager;
            _emailSender= emailSender;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(string returnurl = null)
        {
            //Para la creacion de los rolesff
            if (!await _roleManager.RoleExistsAsync("Administrar"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Administrador"));
            }

            //Para la creacion de los registrado
            if (!await _roleManager.RoleExistsAsync("Registrado"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Registrado"));
            }

            ViewData["ReturnUrl"] = returnurl;
            RegistroViewModel registroVM = new RegistroViewModel();
            return View(registroVM);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]


        public async Task<IActionResult> Registro(RegistroViewModel rgViewModel, string returnurl=null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario { UserName = rgViewModel.Email, Email = rgViewModel.Email, Nombre = rgViewModel.Nombre, Url = rgViewModel.Url, CodigoPais = rgViewModel.CodigoPais, Telefono = rgViewModel.Telefono, Pais = rgViewModel.Pais, Ciudad = rgViewModel.Ciudad, Direccion = rgViewModel.Direccion, FechaNacimiento = rgViewModel.FechaNacimiento, Estado = rgViewModel.Estado };
                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);

                if (resultado.Succeeded)
                {
                    //Esta linea es para la asignacion del usuario que se registra al rol "Registrado"
                    await _userManager.AddToRoleAsync(usuario, "Administrador");

                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                    //return RedirectToAction("Index", "Home");
                    return LocalRedirect(returnurl);    
                }
                ValidarErrores(resultado);
            }

            return View(rgViewModel);

        }

        //Registro especial solo para administradores

        [HttpGet]
        ///[AllowAnonymous]
        public async Task<IActionResult> RegistroAdministrador(string returnurl = null)
        {
            //Para la creacion de los rolesff
            if (!await _roleManager.RoleExistsAsync("Administrador"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Administrador"));
            }

            //Para la creacion de los registrado
            if (!await _roleManager.RoleExistsAsync("Registrado"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Registrado"));
            }

            //Para la creacion de los registrado
            if (!await _roleManager.RoleExistsAsync("Lector 15 libros"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Lector 15 libros"));
            }

            //Para seleccion de rol

            List<SelectListItem> listaRoles = new List<SelectListItem>();
            listaRoles.Add(new SelectListItem()
            {
                Value = "Registrado",
                Text = "Registrado"
            });

            listaRoles.Add(new SelectListItem()
            {
                Value = "Administrador",
                Text = "Administrador"
            });

            listaRoles.Add(new SelectListItem()
            {
                Value = "Lector 15 libros",
                Text = "Lector 15 libros"
            });

            ViewData["ReturnUrl"] = returnurl;
            RegistroViewModel registroVM = new RegistroViewModel() 
            { 
                ListaRoles = listaRoles

            };
            return View(registroVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroAdministrador(RegistroViewModel rgViewModel, string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario
                {
                    UserName = rgViewModel.Email,
                    Email = rgViewModel.Email,
                    Nombre = rgViewModel.Nombre,
                    Url = rgViewModel.Url,
                    CodigoPais = rgViewModel.CodigoPais,
                    Telefono = rgViewModel.Telefono,
                    Pais = rgViewModel.Pais,
                    Ciudad = rgViewModel.Ciudad,
                    Direccion = rgViewModel.Direccion,
                    FechaNacimiento = rgViewModel.FechaNacimiento,
                    Estado = rgViewModel.Estado
                };

                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);

                if (resultado.Succeeded)
                {
                    // Para selección de rol
                    if (!string.IsNullOrEmpty(rgViewModel.RolSeleccionado) && rgViewModel.RolSeleccionado == "Administrador")
                    {
                        await _userManager.AddToRoleAsync(usuario, "Administrador");
                    }
                    else if (rgViewModel.RolSeleccionado == "Lector 15 libros")
                    {
                        await _userManager.AddToRoleAsync(usuario, "Lector 15 libros");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(usuario, "Registrado");
                    }

                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                    return LocalRedirect(returnurl);
                }


                ValidarErrores(resultado);
            }

            // Para selección de rol
            rgViewModel.ListaRoles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Registrado", Text = "Registrado" },
        new SelectListItem { Value = "Administrador", Text = "Administrador" },
        new SelectListItem { Value = "Lector 15 libros", Text = "Lector 15 libros" }

    };

            return View(rgViewModel);
        }


        [AllowAnonymous]

        private void ValidarErrores(IdentityResult resultado)
        {
            foreach(var error in resultado.Errors){

                ModelState.AddModelError(String.Empty, error.Description);
            }
        }

        //Metodo mostrar formulario de acceso

        [HttpGet]
        [AllowAnonymous]


        public IActionResult Acceso(string returnurl=null)
        {
            ViewData["ReturnUrl"]= returnurl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]


        public async Task<IActionResult> Acceso(AccesoViewModel accViewModel, string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl= returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                
                var resultado = await _signInManager.PasswordSignInAsync(accViewModel.Email, accViewModel.Password, accViewModel.RememberMe, lockoutOnFailure: true);

                if (resultado.Succeeded)
                {

                    //return RedirectToAction("Index", "Home");
                    return LocalRedirect(returnurl);
                }
                if (resultado.IsLockedOut)
                {

                    //return RedirectToAction("Index", "Home");
                    return View("Bloqueado");
                }

                else
                {
                    ModelState.AddModelError(String.Empty, "Acceso invalido");
                    return View(accViewModel);
                }
            }

            return View(accViewModel);

        }

        //Salir o cerrar sesion de la aplicacion (logout)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalirAplicacion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //metodo para olvido de contraseña

        [HttpGet]
        [AllowAnonymous]

        public IActionResult OlvidoPassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]

        public async Task<IActionResult> OlvidoPassword(OlvidoPasswordViewModel opViewModel)
        {
            if (ModelState.IsValid)
            {
                var usuario =  await _userManager.FindByEmailAsync(opViewModel.Email);
                if (usuario == null)
                {
                    return RedirectToAction("ConfirmacionOlvidoPassword");
                }
                var codigo = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var urlRetorno = Url.Action("ResetPassword", "Cuentas", new { userId = usuario.Id, code = codigo }, protocol: HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(opViewModel.Email, "Recuperar contraseña ", 
                    "Por favor recupere su contraseña dando click aqui: <a href=\""+ urlRetorno + "\">enlace</a>");
                return RedirectToAction("ConfirmacionOlvidoPassword");
            
            }
            return View(opViewModel);
            
        }

        [HttpGet]
        [AllowAnonymous]

        public IActionResult ConfirmacionOlvidoPassword() 
        {
            return View(); 
        }

        //Funcionalidad para recuperar contraseña
        [HttpGet]
        [AllowAnonymous]

        public IActionResult ResetPassword(string code=null) 
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]


        public async Task <IActionResult> ResetPassword(RecuperaPasswordViewModel rpViewModel)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(rpViewModel.Email);
                if (usuario == null)
                {
                    return RedirectToAction("ConfirmacionRecuperaPassword");
                }

                var resultado = await _userManager.ResetPasswordAsync(usuario, rpViewModel.Code, rpViewModel.Password);

                if (resultado.Succeeded)
                {
                    return RedirectToAction("ConfirmacionRecuperaPassword");
                }

                ValidarErrores(resultado);
            }
            return View(rpViewModel);
        }

        [HttpGet]
        [AllowAnonymous]

        public IActionResult ConfirmacionRecuperaPassword()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]

        public IActionResult Denegado(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            return View();
        }
    }
}
