using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Helpers
{
    public class AllowUnauthenticatedAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.User.Identity.IsAuthenticated)
        {
            throw new AuthException("Cannot access these routes while authenticated, logout",null, (int)HttpStatusCode.Forbidden);
        }
    }
}
}