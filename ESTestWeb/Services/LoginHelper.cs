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
            
            string sql = @"select * from UserInfo where UserID = '{0}'";
            sql = string.Format(sql, userID);
            DataSet ds = DbHelperMySQL.GetInstance().Query(sql);

            if (ds.Tables[0].Rows.Count<=0)
            {
                log.InfoFormat("数据库中不存在{0}的用户信息", userID);
                return false;
            }

            userName = ds.Tables[0].Rows[0]["UserName"].ToString();

            string pwd_DB = ds.Tables[0].Rows[0]["Password"].ToString();

            return pwd_DB.Equals(password);
        }

        public string GetEmptyUserID4Register()
        {
            string sql = @"select * from UserInfo where UserID not like '9%' order by UserID";
            DataSet ds = DbHelperMySQL.GetInstance().Query(sql);

            int cnt = ds.Tables[0].Rows.Count - 1;
            string maxUserID = ds.Tables[0].Rows[cnt]["UserID"].ToString();

            //maxUserID+1
            int i = DTSTools.ConvertToInt(maxUserID, 0);
            i++;

            return i.ToString("000");
        }

        public bool RegisterNewUser(string userID,string userName,string password,out string errMsg)
        {
            errMsg = string.Empty;

            string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string reserveString = $"创建时间:{createTime}";

            string sql = @"replace into UserInfo(UserID,UserName,Password,ReserveString) values('{0}','{1}','{2}','{3}')";
            sql = string.Format(sql, userID, userName, password, reserveString);
            
            try
            { 
                int cnt = DbHelperMySQL.GetInstance().ExecuteNonQuery(sql);

                return true;
            }
            catch(Exception ex)
            {
                log.Error(ex);
                errMsg = ex.Message;
                return false;
            }
            
        }
    }
}
