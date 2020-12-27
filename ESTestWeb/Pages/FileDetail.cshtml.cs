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
        /// 完整的文件名
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
            //从Get参数中获取 qgssluaxmlfile

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
                        //找到文件，将文件内容读出后显示在界面上
                        this.FileContent = item.FileContent;
                        this.FileName = item.FileName;
                        break;
                    }
                }
            }
        }
    }
}
