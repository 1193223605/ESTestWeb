using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESTest.control;
using ESTestWeb.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace ESTestWeb.Pages
{
    public class FileDetailModel : PageModel
    {
        string m_fullFileName = string.Empty;
        private readonly IMemoryCache m_memoryCache;

        /// <summary>
        /// �������ļ���
        /// </summary>
        public string FullFileName { get => m_fullFileName; set => m_fullFileName = value; }

        public string KeyWord { get; set; } = "";

        public int CurPageIndex { get; set; }

        public string FileName { get; set; }

        public string FileContent { get; set; }

        public FileDetailModel(IMemoryCache memoryCache)
        {
            this.m_memoryCache = memoryCache;
        }

        public void OnGet()
        {
            //��Get�����л�ȡ qgssluaxmlfile

            string tmpStr = HttpContext.Request.Query["fullFileName"].ToString();

            tmpStr = tmpStr ?? "";

            this.FullFileName = tmpStr;

            this.KeyWord = HttpContext.Request.Query["keyword"].ToString();

            this.CurPageIndex = DTSTools.ConvertToInt(HttpContext.Request.Query["pageIndex"].ToString(), 0);

            this.getFileContentFromCache();
        }

        void getFileContentFromCache()
        {
            string key = $"qgsourcefile_{this.KeyWord}_{this.CurPageIndex}";
            var searchRs = this.m_memoryCache.Get<SearchRs>(key);

            if (searchRs!= null)
            {
                foreach(var item in searchRs.Documents)
                {
                    if (item.FullFileName.Equals(this.FullFileName))
                    {
                        //�ҵ��ļ������ļ����ݶ�������ʾ�ڽ�����
                        this.FileContent = item.FileContent;
                        this.FileName = item.FileName;
                        break;
                    }
                }
            }
        }
    }
}
