using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace acebook.ActionFilters
{
  public class AuthenticationFilter : IActionFilter {
    public void OnActionExecuting(ActionExecutingContext context) {
      int? user_id = context.HttpContext.Session.GetInt32("user_id");
      if(user_id == null) {
        context.Result = new RedirectResult("/signin");
        return;
      }
      else
      {
        return;
      }
    }

    public void OnActionExecuted(ActionExecutedContext context) {
      
    }
  }
}