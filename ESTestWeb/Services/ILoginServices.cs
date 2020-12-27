using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESTestWeb.Services
{
    public interface ILoginServices
    {
        string GetEmptyUserID4Register();

        /// <summary>
        /// 登录用户，验证密码
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool LoginUser(string userID, string password,out string userName);
        bool RegisterNewUser(string userID, string userName, string password, out string errMsg);
    }
}
