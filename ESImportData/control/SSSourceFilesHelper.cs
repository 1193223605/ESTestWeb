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
        public static void ReadSSSourceFiles(ESProvider esProvider,string rootPath)
        {
            //string rootPath = @"D:\workshop\ElasticSearch\QG测试文件\";

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

                        var encoding = GetFileEncodeType(f);
                        //dotnet core 的 Encoding.Default 是 utf8
                        if (encoding.CodePage.Equals(Encoding.Default.CodePage)==false)
                        {
                            es_fileInfo.FileContent = File.ReadAllText(f,encoding);
                        }
                        else
                        {
                            es_fileInfo.FileContent = File.ReadAllText(f);
                        }
                        
                        if (es_fileInfo.FileName.EndsWith("lua", StringComparison.OrdinalIgnoreCase))
                        {
                            es_fileInfo.LuaOrXml = 1;
                        }
                        else
                        {
                            es_fileInfo.LuaOrXml = 2;
                        }

                        esProvider.PopulateIndex(es_fileInfo, "qgsourcefile");
                    }

                }
                readSSSourceFiles(subDir, esProvider);
            }
        }

        public static System.Text.Encoding GetFileEncodeType(string filename)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
            Byte[] buffer = br.ReadBytes(2);
            if (buffer[0] >= 0xEF)
            {
                if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                {
                    return System.Text.Encoding.UTF8;
                }
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    return System.Text.Encoding.BigEndianUnicode;
                }
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    return System.Text.Encoding.Unicode;
                }
                else
                {
                    return System.Text.Encoding.GetEncoding("GBK");
                }
            }
            else
            {
                return System.Text.Encoding.GetEncoding("GBK");
            }
        }
    }
}
