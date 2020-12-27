using ESTest.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ESTest.control
{
    public class SSSourceFilesHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
          System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string ESINDEXNAME = "qgsourcefile";
        public static void ReadSSSourceFiles(ESProvider esProvider)
        {
            string rootPath = @"D:\workshop\ElasticSearch\QG测试文件\";

            readSSSourceFiles(rootPath, esProvider);


        }

        static void readSSSourceFiles(string parentPath,ESProvider esProvider)
        {
            if (string.IsNullOrEmpty(parentPath) || Directory.Exists(parentPath)==false)
            {
                return;
            }
            string[] dirs = Directory.GetDirectories(parentPath);

            foreach (var subDir in dirs)
            {
                //Console.WriteLine(x);
                //显示xml文件和lua文件
                string[] files = Directory.GetFiles(subDir);
                foreach(var f in files)
                {
                    bool br = File.Exists(f);
                    //Console.WriteLine(f+" exists:"+br.ToString());

                    if (f.EndsWith("lua",StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith("xml", StringComparison.OrdinalIgnoreCase))
                    {
                        //new QGSSLuaXmlFile
                        QGSSLuaXmlFile es_fileInfo = new QGSSLuaXmlFile();

                        FileInfo fi = new FileInfo(f);
                        es_fileInfo.StrategyName = fi.Name.Substring(0,fi.Name.IndexOf("."));
                        es_fileInfo.FileName = fi.Name;
                        es_fileInfo.FullFileName = fi.FullName;
                        es_fileInfo.FileLocation = fi.DirectoryName;
                        es_fileInfo.FileContent = File.ReadAllText(f);
                        if (es_fileInfo.FileName.EndsWith("lua", StringComparison.OrdinalIgnoreCase))
                        {
                            es_fileInfo.LuaOrXml = 1;
                        }
                        else
                        {
                            es_fileInfo.LuaOrXml = 2;
                        }

                        esProvider.PopulateIndex(es_fileInfo, ESINDEXNAME);
                    }

                }
                readSSSourceFiles(subDir, esProvider);
            }
        }
    }
}
