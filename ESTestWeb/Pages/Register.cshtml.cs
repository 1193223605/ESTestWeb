using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESTestWeb.Services;
using ESTestWeb.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ESTestWeb.Pages
{
    public class RegisterModel : PageModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
          System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string UserID { get; set; } = "";

        public string UserName { get; set; } = "";

        public string Password { get; set; } = "";
        public ILoginServices m_LoginService { get; }

        public RegisterModel(ILoginServices loginService)
        {
            this.m_LoginService = loginService;
        }

        string m_LoginResult = string.Empty;
        [BindProperty]
        public string LoginResult
        {
            get { return m_LoginResult; }
        }

        public void OnGet()
        {
            this.UserID = m_LoginService.GetEmptyUserID4Register();
        }

        /// <summary>
        /// Ajax���õĽӿ�
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public IActionResult OnPostRegister(string userID, string userName, string password)
        {
            log.InfoFormat("UserID:{0}", userID);
            log.InfoFormat("userName:{0}", userName);
            log.InfoFormat("Password:{0}", password);

            AjaxMsg msg = new AjaxMsg();

            string errMsg = string.Empty;
            bool br = m_LoginService.RegisterNewUser(userID, userName, password, out errMsg);

            msg.Success = br;
            msg.ReturnMessage = "";
            if (br == false)
            {
                m_LoginResult = $"ע�����û�ʧ�ܣ�ԭ��:{errMsg}";
                msg.ReturnMessage = LoginResult;
            }

            //ע��ɹ�����ת�� ��¼ҳ�棬�Զ��� UserID,�ȴ��û���������
            return new JsonResult(msg);
        }
    }
}
