using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading;
namespace SLSM.Web.DTSWeb
{
    /// <summary>
    /// 数据访问抽象基础类
    /// Copyright (C) Maticsoft
    /// </summary>
    public class DbHelperMySQL
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
                  System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库.		
        //public static string connectionString = ConfigurationManager.ConnectionStrings["TestClientConnstring"].ConnectionString;
        public string connectionString = string.Empty;

        public static string DefaultConnectionString = string.Empty;

        private static Dictionary<string, DBHelperInfo> m_InstanceDic = new Dictionary<string, DBHelperInfo>();

        const int TIMEOUT_MYSQLCONNECTION = 1000;

        public static DbHelperMySQL GetInstance()
        {
            return GetInstance(DefaultConnectionString);
        }

        public static DbHelperMySQL GetInstance(string connStr)
        {
            //lock (m_InstanceDic)
            bool br = Monitor.TryEnter(m_InstanceDic, TIMEOUT_MYSQLCONNECTION);
            try
            {
                if (m_InstanceDic.ContainsKey(connStr) == false)
                {
                    DBHelperInfo dbHelper = new DBHelperInfo();
                    dbHelper.DBHelper = new DbHelperMySQL(connStr);

                    dbHelper.LastUsedTime = DateTime.Now;
                    m_InstanceDic[connStr] = dbHelper;
                }
                //log.DebugFormat("sessionID is {0},number of DbHelperMySQL is {1}", sessionID, m_InstanceDic.Count);
                m_InstanceDic[connStr].LastUsedTime = DateTime.Now;

                //DisposeUselessDbHelper();

                return m_InstanceDic[connStr].DBHelper;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                if (br == true)
                {
                    Monitor.Exit(m_InstanceDic);
                }
            }
            return m_InstanceDic[connStr].DBHelper;
        }

        private static void DisposeUselessDbHelper()
        {
            try
            {
                List<string> toDeleted = new List<string>();
                foreach (KeyValuePair<string, DBHelperInfo> item in m_InstanceDic)
                {
                    TimeSpan timeSpan = DateTime.Now - item.Value.LastUsedTime;
                    if (timeSpan.Seconds >= 30)
                    {
                        toDeleted.Add(item.Key);
                    }
                }
                foreach (string sessionID in toDeleted)
                {
                    if (m_InstanceDic.ContainsKey(sessionID) == true &&
                        m_InstanceDic[sessionID] != null &&
                        m_InstanceDic[sessionID].DBHelper != null)
                    {
                        m_InstanceDic[sessionID].DBHelper.DisposeResource();
                        m_InstanceDic.Remove(sessionID);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// 一个静态的DbHelperMySQL，主要是为了Transaction用的
        /// </summary>
        //private MySqlConnection m_connection = new MySqlConnection(connectionString);
        private MySqlConnection m_connection = null;
        private MySqlTransaction m_trans = null;

        private object objLock = new object();

        public object GetLockObject()
        {
            return objLock;
        }

        //public void LockTable(string tableName)
        //{
        //    string sql = string.Format("LOCK TABLES {0} WRITE", tableName);
        //    ExecuteNonQuery(sql);
        //}

        //public void UnLockTables()
        //{
        //    string sql = string.Format("UNLOCK TABLES");
        //    ExecuteNonQuery(sql);
        //}

        public void StartTrans()
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return;
            }

            m_connection = this.GetConnection();

            try
            {
                //if (m_connection.State != ConnectionState.Open)
                //{
                //    m_connection.Open();
                //}
                //else
                //{
                //    m_connection.Close();
                //    m_connection.Open();
                //}

                m_trans = m_connection.BeginTransaction();
            }
            catch (Exception e)
            {
                log.Error(e);

                m_connection = this.NewConnect(m_connection);
                m_trans = m_connection.BeginTransaction();

            }
            finally
            {
                Monitor.Exit(this.objLock);
            }
        }

        public void CommitTrans()
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return;
            }

            try
            {
                if (m_trans == null)
                {
                    log.Info("m_trans == null,re connect mysql");
                }
                else
                {
                    m_trans.Commit();
                    m_connection.Close();
                    m_trans = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            finally
            {
                Monitor.Exit(this.objLock);
            }

        }

        public void RollBackTrans()
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return;
            }

            try
            {
                if (m_trans == null)
                {
                    log.Info("m_trans == null,re connect mysql");
                }
                else
                {
                    m_trans.Rollback();
                    m_connection.Close();
                    m_trans = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            finally
            {
                Monitor.Exit(this.objLock);
            }
        }

        public void DisposeResource()
        {
            if (m_connection != null)
            {
                m_connection.Close();
            }

            //if (mscBackstage != null)
            //{
            //    mscBackstage.Close();
            //}
        }

        private DbHelperMySQL(string connStrName)
        {
            //connectionString = ConfigurationManager.ConnectionStrings[connStrName].ConnectionString;
            connectionString = connStrName;

            try
            {
                m_connection = new MySqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public MySqlConnection GetConnection()
        {
            return GetConnection(this.m_connection);
        }

        private void SetCmdTransaction(MySqlCommand cmd)
        {
            if (m_trans != null)
            {
                cmd.Transaction = m_trans;
            }
        }

        private void CloseConnectionWithoutTrans(MySqlConnection connection)
        {
            if (m_trans == null)
            {
                try
                {
                    connection.Close();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    connection = this.NewConnect(connection);
                }
            }
        }

        private void ReturnConnection(MySqlConnection connection)
        {
            return;
            if (m_trans == null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        /**
        public static string csBackstage = ConfigurationManager.ConnectionStrings["BackstageMySQLConnstring"].ConnectionString;
        private MySqlConnection mscBackstage;
        public MySqlConnection MscBackstage
        {
            get
            {
                if (mscBackstage == null)
                {
                    lock (objLock)
                    {
                        if (mscBackstage == null)
                        {
                            mscBackstage = new MySqlConnection(csBackstage);
                            ExecuteNonQueryBackstage("SET NAMES 'latin1';");
                        }
                    }
                }

                return mscBackstage;
            }
        }
         * */

        private MySqlConnection GetConnection(MySqlConnection msc)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return msc;
            }

            try
            {
                if (msc.State == ConnectionState.Broken)
                {
                    msc.Close();
                    msc.Open();
                }
                else if (msc.State == ConnectionState.Closed)
                {
                    msc.Open();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);

                msc = this.NewConnect(msc);

            }
            finally
            {
                Monitor.Exit(this.objLock);
            }

            return msc;
        }

        MySqlConnection NewConnect(MySqlConnection msc)
        {
            int times=0;
            do
            {
                log.Info("new Connection");
                try
                {
                    msc = new MySqlConnection(connectionString);
                    this.m_trans = null;
                    break;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
                times++;
                Thread.Sleep(500);
            } while (times < 10);

            return msc;
        }

        //public static string csZGRisk = ConfigurationManager.ConnectionStrings["ZGRiskMySQLConnstring"].ConnectionString;
        //private MySqlConnection mscZGRisk;
        //public MySqlConnection MscZGRisk
        //{
        //    get
        //    {
        //        if (mscZGRisk == null)
        //        {
        //            lock (objLock)
        //            {
        //                if (mscZGRisk == null)
        //                {
        //                    mscZGRisk = new MySqlConnection(csZGRisk);
        //                    ExecuteNonQueryZGRisk("SET NAMES 'latin1';");
        //                }
        //            }
        //        }

        //        return mscZGRisk;
        //    }
        //}

        #region  执行简单SQL语句

        ///// <summary>
        ///// 执行SQL语句，返回影响的记录数
        ///// </summary>
        ///// <param name="SQLString">SQL语句</param>
        ///// <returns>影响的记录数</returns>
        //public int ExecuteNonQueryBackstage(string SQLString)
        //{
        //    return ExecuteNonQuery(SQLString, GetConnection(this.MscBackstage));
        //}

        ///// <summary>
        ///// 执行SQL语句，返回影响的记录数
        ///// </summary>
        ///// <param name="SQLString">SQL语句</param>
        ///// <returns>影响的记录数</returns>
        //public int ExecuteNonQueryZGRisk(string SQLString)
        //{
        //    return ExecuteNonQuery(SQLString, GetConnection(this.MscZGRisk));
        //}

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteNonQuery(string SQLString)
        {
            //log.InfoFormat("execute sql:{0}", SQLString);
            return ExecuteNonQuery(SQLString, GetConnection());
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteNonQuery(string SQLString, MySqlConnection msc)
        {
            log.InfoFormat("execute sql:{0}", SQLString);

            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return 0;
            }

            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = msc;
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        cmd.CommandTimeout = 1000 * 60 * 3;
                        SetCmdTransaction(cmd);
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    //catch (MySql.Data.MySqlClient.MySqlException e)
                    catch (Exception e)
                    {
                        log.Error(e);
                        CloseConnectionWithoutTrans(connection);
                        //return 0;
                        throw e;
                    }
                    finally
                    {
                        Monitor.Exit(this.objLock);
                        ReturnConnection(connection);
                    }
                }
            }
        }


        public int ExecuteNonQueryByTime(string SQLString, int Times)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return 0;
            }

            log.InfoFormat("execute sql:{0}", SQLString);
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        SetCmdTransaction(cmd);
                        cmd.CommandTimeout = Times;
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    //catch (MySql.Data.MySqlClient.MySqlException e)
                    catch (Exception e)
                    {
                        log.Error(e);
                        CloseConnectionWithoutTrans(connection);
                        //return 0;
                        throw e;
                    }
                    finally
                    {
                        ReturnConnection(connection);
                        Monitor.Exit(this.objLock);
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        private int ExecuteNonQueryTran(List<String> SQLStringList)
        {
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connection;
                MySqlTransaction tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    int count = 0;
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch (Exception e)
                {
                    log.Error(e);
                    tx.Rollback();
                    return 0;
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteNonQuery(string SQLString, string content)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return 0;
            }

            log.InfoFormat("execute sql:{0}", SQLString);
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                MySqlCommand cmd = new MySqlCommand(SQLString, connection);
                MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    SetCmdTransaction(cmd);
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                //catch (MySql.Data.MySqlClient.MySqlException e)
                catch (Exception e)
                {
                    log.Error(e);
                    CloseConnectionWithoutTrans(connection);
                    //return 0;
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    ReturnConnection(connection);
                    Monitor.Exit(this.objLock);
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object ExecuteScalar(string SQLString)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return null;
            }

            log.InfoFormat("execute sql:{0}", SQLString);
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        SetCmdTransaction(cmd);
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    //catch (MySql.Data.MySqlClient.MySqlException e)
                    catch (Exception e)
                    {
                        log.Error(e);
                        CloseConnectionWithoutTrans(connection);
                        throw e;
                        //return null;
                    }
                    finally
                    {
                        ReturnConnection(connection);
                        Monitor.Exit(this.objLock);
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public MySqlDataReader ExecuteReader(string strSQL)
        {
            log.InfoFormat("execute sql:{0}", strSQL);

            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return null;
            }

            //MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlConnection connection = GetConnection();
            MySqlCommand cmd = new MySqlCommand(strSQL, connection);
            try
            {
                SetCmdTransaction(cmd);
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            //catch (MySql.Data.MySqlClient.MySqlException e)
            catch (Exception e)
            {
                log.Error(e);
                CloseConnectionWithoutTrans(connection);
                throw e;
                //return 0;
            }
            finally
            {
                ReturnConnection(connection);
                Monitor.Exit(this.objLock);
            }
        }

        private static void ProcEncoding(DataSet ds)
        {
            var en = System.Text.Encoding.GetEncoding(1252);
            foreach (DataTable dt in ds.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    foreach (DataColumn dlc in dt.Columns)
                    {
                        if (dlc.DataType == typeof(string))
                        {
                            var bs = en.GetBytes(dr[dlc.ColumnName].ToString());
                            var a = System.Text.Encoding.UTF8.GetString(bs);
                            dr[dlc.ColumnName] = a;
                        }
                    }
                }
            }
        }

        ///// <summary>
        ///// 执行查询语句，返回DataSet
        ///// </summary>
        ///// <param name="SQLString">查询语句</param>
        ///// <returns>DataSet</returns>
        //public DataSet QueryZGRisk(string SQLString)
        //{
        //    log.InfoFormat("execute sql:{0}", SQLString);
        //    DataSet ds = Query(SQLString, this.MscZGRisk);
        //    ProcEncoding(ds);
        //    return ds;
        //}

        ///// <summary>
        ///// 执行查询语句，返回DataSet
        ///// </summary>
        ///// <param name="SQLString">查询语句</param>
        ///// <returns>DataSet</returns>
        //public DataSet QueryBackstage(string SQLString)
        //{
        //    log.InfoFormat("Query sql:{0}", SQLString);
        //    DataSet ds = Query(SQLString, this.MscBackstage);
        //    ProcEncoding(ds);
        //    return ds;
        //}

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString)
        {
            log.InfoFormat("Query sql:{0}", SQLString);
            return Query(SQLString, GetConnection());
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, MySqlConnection msc)
        {
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            log.InfoFormat("Query sql:{0}", SQLString);
            DataSet ds = new DataSet();

            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return ds;
            }

            
            MySqlConnection connection = msc;
            
            try
            {
                MySqlDataAdapter command = new MySqlDataAdapter(SQLString, connection);
                command.Fill(ds, "ds");
            }
            //catch (MySql.Data.MySqlClient.MySqlException ex)
            catch(Exception ex)
            {
                log.Error(ex);
                CloseConnectionWithoutTrans(connection);
                throw new Exception(ex.Message);
            }
            finally
            {
                ReturnConnection(connection);

                Monitor.Exit(this.objLock);

            }
            return ds;
        
        }

        /// <summary>
        /// 执行查询语句，返回DataTable
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataTable QueryDT(string SQLString)
        {
            log.InfoFormat("Query sql:{0}", SQLString);
            return QueryDT(SQLString, GetConnection());
        }

        /// <summary>
        /// 执行查询语句，返回DataTable
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataTable QueryDT(string SQLString, MySqlConnection msc)
        {
            log.InfoFormat("Query sql:{0}", SQLString);
            DataTable dt = new DataTable();

            bool br = Monitor.TryEnter(this.objLock, 1000 * 60 * 10);

            if (br == false)
            {
                log.Info("can not get lock");
                return dt;
            }

            MySqlConnection connection = msc;

            try
            {
                MySqlCommand cmd = new MySqlCommand(SQLString, connection);
                cmd.CommandTimeout = 1000 * 60 * 10;
                MySqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            //catch (MySql.Data.MySqlClient.MySqlException ex)
            catch (Exception ex)
            {
                log.Error(ex);
                CloseConnectionWithoutTrans(connection);
                throw new Exception(ex.Message);
            }
            finally
            {
                ReturnConnection(connection);

                Monitor.Exit(this.objLock);

            }
            return dt;

        }


        #endregion

        #region 执行带参数的SQL语句

        ///// <summary>
        ///// 执行SQL语句，返回影响的记录数
        ///// </summary>
        ///// <param name="SQLString">SQL语句</param>
        ///// <returns>影响的记录数</returns>
        //public int ExecuteNonQueryBackstage(string SQLString, params MySqlParameter[] cmdParms)
        //{
        //    log.InfoFormat("execute sql:{0}", SQLString);
        //    //using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    MySqlConnection connection = GetConnection(this.MscBackstage);
        //    {
        //        using (MySqlCommand cmd = new MySqlCommand())
        //        {
        //            try
        //            {
        //                PrepareCommand(connection, cmd, SQLString, cmdParms);
        //                int rows = cmd.ExecuteNonQuery();
        //                cmd.Parameters.Clear();
        //                return rows;
        //            }
        //            catch (MySql.Data.MySqlClient.MySqlException e)
        //            {
        //                log.Error(e);
        //                CloseConnectionWithoutTrans(connection);
        //                throw e;
        //            }
        //            finally
        //            {
        //                ReturnConnection(connection);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteNonQuery(string SQLString, params MySqlParameter[] cmdParms)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return 0;
            }

            log.InfoFormat("execute sql:{0}", SQLString);
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(connection, cmd, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    //catch (MySql.Data.MySqlClient.MySqlException e)
                    catch (Exception e)
                    {
                        log.Error(e);
                        CloseConnectionWithoutTrans(connection);
                        throw e;
                    }
                    finally
                    {
                        ReturnConnection(connection);
                        Monitor.Exit(this.objLock);
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public void ExecuteNonQueryTran(Hashtable SQLStringList)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return;
            }

            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                //SetCmdTransaction(cmd);
                using (MySqlTransaction trans = connection.BeginTransaction())
                {
                    MySqlCommand cmd = new MySqlCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            PrepareCommand(connection, cmd, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        ReturnConnection(connection);
                        Monitor.Exit(this.objLock);
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public void ExecuteNonQueryTranWithIndentity(Hashtable SQLStringList)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return;
            }

            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                //CheckConnection();
                //SetCmdTransaction(cmd);
                using (MySqlTransaction trans = connection.BeginTransaction())
                {
                    MySqlCommand cmd = new MySqlCommand();
                    try
                    {
                        int indentity = 0;
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            foreach (MySqlParameter q in cmdParms)
                            {
                                if (q.Direction == ParameterDirection.InputOutput)
                                {
                                    q.Value = indentity;
                                }
                            }
                            PrepareCommand(connection, cmd, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            foreach (MySqlParameter q in cmdParms)
                            {
                                if (q.Direction == ParameterDirection.Output)
                                {
                                    indentity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        ReturnConnection(connection);
                        Monitor.Exit(this.objLock);
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object ExecuteScalar(string SQLString, params MySqlParameter[] cmdParms)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return null;
            }

            log.InfoFormat("execute sql:{0}", SQLString);
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(connection, cmd, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    //catch (MySql.Data.MySqlClient.MySqlException e)
                    catch (Exception e)
                    {
                        log.Error(e);
                        CloseConnectionWithoutTrans(connection);
                        throw e;
                    }
                    finally
                    {
                        Monitor.Exit(this.objLock);
                        ReturnConnection(connection);
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public MySqlDataReader ExecuteReader(string SQLString, params MySqlParameter[] cmdParms)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return null;
            }

            log.InfoFormat("execute sql:{0}", SQLString);
            //MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlConnection connection = GetConnection();
            MySqlCommand cmd = new MySqlCommand();
            try
            {
                PrepareCommand(connection, cmd, SQLString, cmdParms);
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            //catch (MySql.Data.MySqlClient.MySqlException e)
            catch (Exception e)
            {
                log.Error(e);
                CloseConnectionWithoutTrans(connection);
                throw e;
            }
            finally
            {
                Monitor.Exit(this.objLock);
                ReturnConnection(connection);
            }
        }

        ///// <summary>
        ///// 执行查询语句，返回DataSet
        ///// </summary>
        ///// <param name="SQLString">查询语句</param>
        ///// <returns>DataSet</returns>
        //public DataSet QueryBackstage(string SQLString, params MySqlParameter[] cmdParms)
        //{
        //    log.InfoFormat("Query sql:{0}", SQLString);
        //    //using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    MySqlConnection connection = GetConnection(this.MscBackstage);
        //    {
        //        MySqlCommand cmd = new MySqlCommand();
        //        PrepareCommand(connection, cmd, SQLString, cmdParms);
        //        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
        //        {
        //            DataSet ds = new DataSet();
        //            try
        //            {
        //                da.Fill(ds, "ds");
        //                cmd.Parameters.Clear();
        //            }
        //            catch (MySql.Data.MySqlClient.MySqlException ex)
        //            {
        //                log.Error(ex);
        //                CloseConnectionWithoutTrans(connection);
        //                throw new Exception(ex.Message);
        //            }
        //            finally
        //            {
        //                ReturnConnection(connection);
        //            }
        //            return ds;
        //        }
        //    }
        //}

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, params MySqlParameter[] cmdParms)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return null;
            }

            log.InfoFormat("Query sql:{0}", SQLString);
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                MySqlCommand cmd = new MySqlCommand();
                PrepareCommand(connection, cmd, SQLString, cmdParms);
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    //catch (MySql.Data.MySqlClient.MySqlException ex)
                    catch(Exception ex)
                    {
                        log.Error(ex);
                        CloseConnectionWithoutTrans(connection);
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        Monitor.Exit(this.objLock);
                        ReturnConnection(connection);
                    }
                    return ds;
                }
            }
        }


        private void PrepareCommand(MySqlConnection connection, MySqlCommand cmd, string cmdText, MySqlParameter[] cmdParms)
        {
            //CheckConnection();
            SetCmdTransaction(cmd);
            cmd.Connection = connection;
            cmd.CommandText = cmdText;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {


                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>MySqlDataReader</returns>
        public MySqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return null;
            }

            //MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlConnection connection = GetConnection();
            MySqlDataReader returnReader;
            //CheckConnection();
            try
            {
                MySqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
                command.CommandType = CommandType.StoredProcedure;
                returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                CloseConnectionWithoutTrans(connection);
                throw ex;
            }
            finally
            {
                Monitor.Exit(this.objLock);
                ReturnConnection(connection);
            }
            return returnReader;
        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return null;
            }

            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                DataSet dataSet = new DataSet();
                //CheckConnection();
                using (MySqlDataAdapter sqlDA = new MySqlDataAdapter())
                {
                    sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                    try
                    {
                        sqlDA.Fill(dataSet, tableName);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                        CloseConnectionWithoutTrans(connection);
                        throw ex;
                    }
                    finally
                    {
                        Monitor.Exit(this.objLock);
                        ReturnConnection(connection);
                    }
                    return dataSet;
                }
            }
        }



        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private MySqlCommand BuildQueryCommand(MySqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            MySqlCommand command = new MySqlCommand(storedProcName, connection);
            command.CommandTimeout = 300;
            command.CommandType = CommandType.StoredProcedure;
            SetCmdTransaction(command);
            foreach (MySqlParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    // 检查未分配值的输出参数,将其分配以DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        /// <summary>
        /// 执行存储过程，返回影响的行数		
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                rowsAffected = 0;
                return 0;
            }

            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                int result;
                //CheckConnection();
                MySqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                try
                {
                    rowsAffected = command.ExecuteNonQuery();
                    result = (int)command.Parameters["ReturnValue"].Value;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    CloseConnectionWithoutTrans(connection);
                    throw ex;
                }
                finally
                {
                    Monitor.Exit(this.objLock);
                    ReturnConnection(connection);
                }
                return result;
            }
        }

        /// <summary>
        /// 创建 SqlCommand 对象实例(用来返回一个整数值)	
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand 对象实例</returns>
        private MySqlCommand BuildIntCommand(MySqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            MySqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new MySqlParameter("ReturnValue",
                MySqlDbType.Int32, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        /// <summary>
        /// 执行存储过程，返回首行首列	
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>返回首行首列</returns>
        public int RunProcedureNonQuery(string storedProcName, IDataParameter[] parameters)
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            if (br == false)
            {
                log.Info("can not get lock");
                return 0;
            }
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            MySqlConnection connection = GetConnection();
            {
                //CheckConnection();
                MySqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                int result = 0;
                try
                {
                    result = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    CloseConnectionWithoutTrans(connection);
                    throw ex;
                }
                finally
                {
                    Monitor.Exit(this.objLock);
                    ReturnConnection(connection);
                }
                return result;
            }
        }

        #endregion

        public bool GetDBHelperLock()
        {
            bool br = Monitor.TryEnter(this.objLock, 5000);

            return br;
        }

        public void ReleaseDBHelperLock()
        {
            Monitor.Exit(this.objLock);
        }

    }

    class DBHelperInfo
    {
        public DbHelperMySQL DBHelper;
        public DateTime LastUsedTime;
    }
}
