using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESTest.model
{
    public class MeetupEvents
    {
        public long eventid { get; set; }
        public string orignalid { get; set; }
        public string eventname { get; set; }
        public string description { get; set; }
    }

    public class QGSSLuaXmlFile
    {
        /// <summary>
        /// 策略名称
        /// </summary>
        public string StrategyName { get; set; }

        /// <summary>
        /// 策略文件名称（可能是Lua文件，也可能是xml文件）
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 1:Lua  2:XML
        /// </summary>
        public int LuaOrXml { get; set; }

        /// <summary>
        /// 策略文件内容,不拿这个字段的数据
        /// </summary>
        string m_FileContent = string.Empty;
        public string FileContent
        {
            get { return m_FileContent; }
            set { m_FileContent = value; }
        }

        /// <summary>
        /// 文件位置
        /// </summary>
        public string FileLocation { get; set; }

        /// <summary>
        /// 文件完整名
        /// </summary>
        public string FullFileName { get; set; }

        /// <summary>
        /// 含高亮显示关键字代码片段的FileContent
        /// </summary>
        public string FileContentHighlight { get; set; }
    }


    public class QGSSLuaXmlFileDetail
    {
        /// <summary>
        /// 策略名称
        /// </summary>
        public string StrategyName { get; set; }

        /// <summary>
        /// 策略文件名称（可能是Lua文件，也可能是xml文件）
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 1:Lua  2:XML
        /// </summary>
        public int LuaOrXml { get; set; }

        /// <summary>
        /// 策略文件内容
        /// </summary>
        string m_FileContent = string.Empty;
        public string FileContent
        {
            get { return m_FileContent; }
            set { m_FileContent = value; }
        }

        /// <summary>
        /// 文件位置
        /// </summary>
        public string FileLocation { get; set; }

        /// <summary>
        /// 文件完整名
        /// </summary>
        public string FullFileName { get; set; }

        /// <summary>
        /// 含高亮显示关键字代码片段的FileContent
        /// </summary>
        public string FileContentHighlight { get; set; }
    }
}
