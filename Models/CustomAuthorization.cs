using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

using TMS_Project.Models;
public class CustomAuthorization : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            // Redirect unauthenticated users to the forbidden page
            context.Result = new RedirectToActionResult("ForbiddenPage", "Login", null);
        }
        base.OnActionExecuting(context);
    }
}