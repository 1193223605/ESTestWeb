using ESTest.control;
using System;
using System.IO;
using System.Text;

namespace ESTestL
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            ESProvider m_ESProvider = new ESProvider();

            string searchPath = args[0];

            Console.WriteLine("import data from {0}!", searchPath);

            //测试linux 目录含中文

            //printDirName(searchPath);
            SSSourceFilesHelper.ReadSSSourceFiles(m_ESProvider, searchPath);
        }

        static void printDirName(string parentPath)
        {
            if (string.IsNullOrEmpty(parentPath) || Directory.Exists(parentPath) == false)
            {
                return;
            }

            string[] dirs = Directory.GetDirectories(parentPath);
            foreach (string subDir in dirs)
            {
                Console.WriteLine(subDir);
            }
        }
    }
}
