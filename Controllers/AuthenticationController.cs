using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TMS_Project.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace TMS_Project.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly TmsContext _UserContext;
        public AuthenticationController(TmsContext context)
        {
            _UserContext = context;
        }
        public  IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            try
            {
                var user = await _UserContext.Users.Where(x => (x.MDelete == false || x.MDelete == null) && x.Email.ToLower() == loginModel.Username.ToLower() && x.Password == loginModel.Password).Include(x => x.Role).FirstOrDefaultAsync();

                if (user is null || user.UserId <= 0)
                {
                    throw new Exception("User Name or Password is invalid");
                }
               
               




                else if (user is not null || user.UserId > 0)
                {
                    var claims = new List<Claim>
        {
                       new Claim(ClaimTypes.Name, user.UserName),
                       new Claim(ClaimTypes.Email, user.Email),
                       new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                       new Claim(ClaimTypes.Role, user.Role.RoleName),
                       new Claim("RoleId", user.RoleId.ToString()),
        };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        //AllowRefresh = <bool>,
                        // Refreshing the authentication session should be allowed.

                        //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                        // The time at which the authentication ticket expires. A 
                        // value set here overrides the ExpireTimeSpan option of 
                        // CookieAuthenticationOptions set with AddCookie.

                        IsPersistent = true,
                        // Whether the authentication session is persisted across 
                        // multiple requests. When used with cookies, controls
                        // whether the cookie's lifetime is absolute (matching the
                        // lifetime of the authentication ticket) or session-based.

                        //IssuedUtc = <DateTimeOffset>,
                        // The time at which the authentication ticket was issued.

                        //RedirectUri = <string>
                        // The full path or absolute URI to be used as an http 
                        // redirect response value.
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                    var roleName = user.Role?.RoleName;

                    if (roleName == "Admin")
                    {
                        return RedirectToAction("Home", "Home");
                    }
                    else if (roleName == "Project Manager")
                    {
                        return RedirectToAction("Index", "Projects");
                    }
                    else if (roleName == "Team Lead")
                    {
                        return RedirectToAction("Index", "Projects");
                    }
                    else if (roleName == "Team member")
                    {
                        return RedirectToAction("Index", "Tasks");
                    }
                }


                throw new Exception("Some things went wrong");

            }
            catch (Exception exp)
            {

                loginModel.Password = "";
                loginModel.InvalidMessage = exp.Message;
                loginModel.isInvalid = true;


                return View(loginModel);
            }
        }

        //public async Task<IActionResult> Login(LoginModel loginModel)
        //{
        //    try
        //    {
        //        var user = await _UserContext.Users
        //            .Where(x => (x.MDelete == false || x.MDelete == null) && x.Email.ToLower() == loginModel.Username.ToLower() && x.Password == loginModel.Password)
        //            .Include(x => x.Role)
        //            .FirstOrDefaultAsync();

        //        if (user == null || user.UserId <= 0)
        //        {
        //            throw new Exception("User Name or Password is invalid");
        //        }

        //        var roleName = user.Role?.RoleName;

        //        if (roleName == "Admin")
        //        {
        //            return RedirectToAction("AllPages", "Admin");
        //        }
        //        else if (roleName == "ProjectManager")
        //        {
        //            return RedirectToAction("TeamAndProjectPages", "ProjectManager");
        //        }
        //        else if (roleName == "TeamLead")
        //        {
        //            return RedirectToAction("TeamAndProjectPages", "TeamLead");
        //        }
        //        else
        //        {
        //            throw new Exception("Invalid role");
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        loginModel.Password = "";
        //        loginModel.InvalidMessage = exp.Message;
        //        loginModel.isInvalid = true;

        //        return View(loginModel);
        //    }
        //}



        [Route("forgotpassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [Route("Logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            string returnUrl = null;
            // Clear the existing external cookies
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "/");
        }
    }
}

    
      

     
   
