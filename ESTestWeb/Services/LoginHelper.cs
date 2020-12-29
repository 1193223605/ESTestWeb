using Dapper;
using ESTestWeb.Model;
using ESTestWeb.Tools;
using SLSM.Web.DTSWeb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ESTestWeb.Services
{
    public class LoginHelper : ILoginServices
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
                  System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool LoginUser(string userID, string password,out string userName)
        {
            userName = string.Empty;
            using (IDbConnection conn = DbHelperMySQL.GetDbConnection())
            {
                string sql = @"select * from UserInfo where UserID = @UserID";
                
                var user = conn.Query<UserInfo>(sql,new { UserID = userID}).FirstOrDefault<UserInfo>();

                if (string.IsNullOrEmpty(user.UserID))
                {
                    log.InfoFormat("数据库中不存在{0}的用户信息", userID);
                    return false;
                }

                userName = user.UserName;

                return user.Password.Equals(password);
            }
        }

        public string GetEmptyUserID4Register()
        {
            using (IDbConnection conn = DbHelperMySQL.GetDbConnection())
            {
                string sql = @"select UserID from UserInfo where UserID not like '9%' order by UserID";
                var userList = conn.Query<UserInfo>(sql);

                var user = userList.LastOrDefault();

                //maxUserID+1
                int i = DTSTools.ConvertToInt(user.UserID, 0);
                i++;

                return i.ToString("000");
            }

        }

        public bool RegisterNewUser(string userID,string userName,string password,out string errMsg)
        {
            errMsg = string.Empty;

            string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string reserveString = $"创建时间:{createTime}";

            using (IDbConnection conn = DbHelperMySQL.GetDbConnection())
            {
                UserInfo user = new UserInfo()
                {
                    UserID = userID,
                    UserName = userName,
                    Password = password,
                    ReserveString = reserveString,
                };
                int cnt = conn.Execute(@"replace into UserInfo(UserID,UserName,Password,ReserveString) values(@UserID,@UserName,@Password,@ReserveString)", user);

                if (cnt>0)
                {
                    return true;
                }
            }

            return false;
            
        }
    }
}
