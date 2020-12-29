using Dapper;
using ESTestWeb.Model;
using Nest;
using SLSM.Web.DTSWeb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ESTestWeb.Tools
{
    public class DataProvider
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
          System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string[] QueryFrequentSearchKeyword()
        {
            string[] items = new string[9] { "", "", "", "", "", "", "", "", "" };

            using (IDbConnection conn = DbHelperMySQL.GetDbConnection())
            { 
                //查最近9条经常被查询的关键字
                string sql = @$"select * from FrequentSearchKeyword where ESIndexName = '{ESTest.control.SSSourceFilesHelper.ESINDEXNAME}' 
        order by Count desc,LastSearchTime desc limit 9";

                var kwList = conn.Query<FrequentSearchKeyword>(sql);

                int i = 0;
                foreach(var kw in kwList)
                {
                    items[i++] = kw.KeyWord;

                    if (i>=9)
                    {
                        break;
                    }
                }
            }

            return items;
        }

        public static Task<bool> SaveSearchKeyWordInfoAsync(string keyWord)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(keyWord))
                {
                    return false;
                }

                using (IDbConnection conn = DbHelperMySQL.GetDbConnection())
                {
                    string sql = @$"select count(*) from FrequentSearchKeyword where ESIndexName = '{ESTest.control.SSSourceFilesHelper.ESINDEXNAME}' 
                and KeyWord='{keyWord}'";

                    int cnt = conn.Query<int>(sql).Single();
                    log.InfoFormat("sql:{0},cnt:{1}", sql,cnt);

                    if (cnt == 0)
                    {
                        sql = @$"insert into FrequentSearchKeyword(ESIndexName,KeyWord,Count,LastSearchTime) 
values('{ESTest.control.SSSourceFilesHelper.ESINDEXNAME}','{keyWord}',1,Now())";

                    }
                    else
                    {
                        sql = @$"update FrequentSearchKeyword set Count=Count+1,LastSearchTime=Now() where ESIndexName = '{ESTest.control.SSSourceFilesHelper.ESINDEXNAME}' 
            and KeyWord='{keyWord}'";
                    }
                    
                    cnt = conn.Execute(sql);
                    log.InfoFormat("sql:{0},cnt:{1}", sql, cnt);
                    return true;
                }
                
                
            });
        }
    }
}
