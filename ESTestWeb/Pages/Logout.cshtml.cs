using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ESTestWeb.Pages
{
    public class LogoutModel : PageModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
          System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult OnPost()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Task task = HttpContext.SignOutAsync();
                task.Wait();
            }
            return Redirect("/Login");
        }
    }
}
