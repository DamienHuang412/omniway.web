using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omniway.Web.App.Constants;
using Omniway.Web.App.Interfaces;
using Omniway.Web.App.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Omniway.Web.App.Controllers;

public class AuthController : Controller
{
    private readonly Core.Interfaces.IAuthenticationService _authenticationService;
    private readonly IAllowlistManager _allowlistManager;

    public AuthController(Core.Interfaces.IAuthenticationService authenticationService,
        IAllowlistManager allowlistManager)
    {
        _authenticationService = authenticationService;
        _allowlistManager = allowlistManager;
    }
    

    [HttpGet]
    [AllowAnonymous]
    [SwaggerIgnore]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerIgnore]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View();
        var result = _authenticationService.Login(model.UserName, model.Password, HttpContext.RequestAborted)
            .GetAwaiter().GetResult();

        if (!result.IsSuccess) return View();

        _allowlistManager.AddAllowlist(model.UserName);
        
        return View("../User/Index", new UserViewModel
        {
            UserName = HttpContext.User.Identity.Name
        });
    }
    
    [SwaggerIgnore]
    public IActionResult Logout()
    {
        _allowlistManager.RemoveAllowlist(HttpContext.User.Identity.Name);
        return RedirectToAction("Index", "Home");
    }

}