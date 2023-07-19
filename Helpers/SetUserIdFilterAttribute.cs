using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace RPG_dotnet.Helpers
{
    
public class SetUserIdFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            if (context.Controller is UserCharacterController controller)
            {
                controller.UserId = userId;
            }
        }

        base.OnActionExecuting(context);
    }
}
}