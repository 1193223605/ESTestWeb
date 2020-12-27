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

            try
            { 
                //查最近9条经常被查询的关键字
                string sql = @$"select * from FrequentSearchKeyword where ESIndexName = '{ESTest.control.SSSourceFilesHelper.ESINDEXNAME}' 
        order by Count desc,LastSearchTime desc limit 9";
                
                DataSet ds = DbHelperMySQL.GetInstance().Query(sql);

                int i = 0;
                foreach(DataRow dr in ds.Tables[0].Rows)
                {
                    items[i++] = dr["KeyWord"].ToString();

                    if (i>=9)
                    {
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
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
                string sql = @$"select count(*) from FrequentSearchKeyword where ESIndexName = '{ESTest.control.SSSourceFilesHelper.ESINDEXNAME}' 
                and KeyWord='{keyWord}'";
                try
                {
                    DataSet ds = DbHelperMySQL.GetInstance().Query(sql);
                    int cnt = DTSTools.ConvertToInt(ds.Tables[0].Rows[0][0].ToString(), 0);

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
                    DbHelperMySQL.GetInstance().ExecuteNonQuery(sql);
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return false;
                }
            });
        }
    }
}
