using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ESTestWeb.Services;
using ESTestWeb.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ESTestWeb.Pages
{
    public class LoginModel : PageModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
          System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private readonly ILogger<IndexModel> _logger;
        private readonly ILoginServices _LoginService;

        public LoginModel(ILoginServices loginService)
        {
            //_logger = logger;
            _LoginService = loginService;
        }

        private string m_UserID = string.Empty;
        [BindProperty]
        [Required]
        [MinLength(3)]
        [MaxLength(3)]
        public string UserID
        {
            get { return m_UserID; }
            set { m_UserID = value; }
        }

        private string m_Password = string.Empty;
        [BindProperty]
        [Required]
        public string Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }

        private string m_Title = "GFQG 策略代码搜索平台登录";
        public string Title => m_Title;
        public void OnGet()
        {
            //Validation.RequireField("UserID", "UserID is Required.");
        }

        string m_LoginResult = string.Empty;
        [BindProperty]
        public string LoginResult
        {
            get { return m_LoginResult; }
        }

        /// <summary>
        /// Ajax调用的接口
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public IActionResult OnPostLogin(string userID,string password)
        {
            log.InfoFormat("UserID:{0}", userID);
            log.InfoFormat("Password:{0}", password);

            AjaxMsg msg = new AjaxMsg();

            string userName;
            bool br = _LoginService.LoginUser(userID, password,out userName);

            msg.Success = br;
            msg.ReturnMessage = "";
            if (br == false)
            {
                m_LoginResult = "用户号或者密码错误，请重试";
                msg.ReturnMessage = LoginResult;
            }

            //登陆成功，将登录信息保存到cookie
            var claims = new[] { new Claim("UserName", userName) };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal user = new ClaimsPrincipal(claimsIdentity);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user);

            return new JsonResult(msg);
        }
    }

    public class LoginInfo
    {
        public string UserID { get; set; }
        public string Password { get; set; }
    }
}
