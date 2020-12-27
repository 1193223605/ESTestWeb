using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESTest.control;
using ESTest.model;
using ESTestWeb.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nest;

namespace ESTestWeb.Pages
{
    public class IndexModel : PageModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
          System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        bool m_IsAuthenticate = false;
        [BindProperty]
        public bool IsAuthenticate
        {
            get { return m_IsAuthenticate; }
        }

        string m_LoginUserName = string.Empty;
        [BindProperty]
        public string LoginUserName
        {
            get { return m_LoginUserName; }
        }

        string m_keyWord = string.Empty;
        [BindProperty]
        public string KeyWord
        {
            get { return m_keyWord; }
            set { m_keyWord = value; }
        }

        [BindProperty]
        public List<string> Tokens
        {
            get;set;
        }

        [BindProperty]
        public SearchRs QueryResult { get => m_QueryResult; set => m_QueryResult = value; }
        

        SearchRs m_QueryResult;

        int m_CurPageIndex = 0;
        /// <summary>
        /// 当前PageIndex，从0开始
        /// </summary>
        [BindProperty]
        public int CurPageIndexFrom0 { get => m_CurPageIndex; set => m_CurPageIndex = value; }

        [BindProperty]
        public string[] FrequentSearchKeyWord
        {
            get;set;
        }

        private readonly IESProvider _ESProvider;
        private readonly IMemoryCache m_memoryCache;

        public IndexModel(IESProvider provider, IMemoryCache memoryCache)
        {
            this._ESProvider = provider;
            this.m_memoryCache = memoryCache;
        }

        /// <summary>
        /// 临时session变量
        /// </summary>
        readonly string SearchKeyWord = "SearchKeyWord";

        public ActionResult OnGet()
        {
            log.Info("OnGet");

            log.InfoFormat("HttpContext.User.Identity.IsAuthenticated:{0}",
                HttpContext.User.Identity.IsAuthenticated);

            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                renderPage_IsAuthenticated();
            }
            else
            {
                return Redirect("./Login");
            }
            return Page();
        }

        void getPageIndexAndKeyword()
        {
            
            string tmpStr = HttpContext.Request.Query["pageIndex"].ToString();
            if (string.IsNullOrEmpty(tmpStr)==false)
            {
                // 取参数 pageIndex值，没取到默认是0,如果取到参数值，需要 - 1
                this.CurPageIndexFrom0 = DTSTools.ConvertToInt(tmpStr, 0);
                if (this.CurPageIndexFrom0 > 0)
                {
                    this.CurPageIndexFrom0--;
                }
            }
            
            if (string.IsNullOrEmpty(this.KeyWord))
            {
                //从页面参数中取
                this.KeyWord = HttpContext.Request.Query["keyword"].ToString();
            }
        }

        async void queryESData(int pageIndexFrom0,string keyWord)
        {
            if (string.IsNullOrEmpty(keyWord)==false)
            {
                keyWord = keyWord.ToLower();
                log.InfoFormat("query keyword:{0}", keyWord);

                this.QueryResult = _ESProvider.QueryByKeyWord(keyWord, "qgsourcefile", pageIndexFrom0);

                //将this.QueryResult 存入到缓存中，查看文件内容跳转到显示文件内容网页时会再用到 查询到的数据
                string key = $"qgsourcefile_{keyWord}_{pageIndexFrom0}";

                //保存到缓存中
                this.m_memoryCache.Set<SearchRs>(key, this.QueryResult, 
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(10)));

                await DataProvider.SaveSearchKeyWordInfoAsync(keyWord);
            }
        }

        public ActionResult OnPost()
        {
            this.KeyWord ??= string.Empty;
            log.InfoFormat("query keyword:{0}", this.KeyWord);

            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                renderPage_IsAuthenticated();
                return Page();
            }
            else
            {
                return Redirect("./Login");
            }
        }

        void queryFrequentSearchKeyword()
        {
            //查数据库表FrequentSearchKeyword
            this.FrequentSearchKeyWord = DataProvider.QueryFrequentSearchKeyword();
        }

        void renderPage_IsAuthenticated()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //这里通过 HttpContext.User.Claims 可以将我们在Login这个Action中存储到cookie中的所有
                //claims键值对都读出来，比如我们刚才定义的UserName的值Wangdacui就在这里读取出来了
                this.m_LoginUserName = HttpContext.User.Claims.First().Value;

                //取参数 pageIndex值，没取到默认是0,如果取到参数值，需要 -1
                this.getPageIndexAndKeyword();

                if (string.IsNullOrEmpty(this.KeyWord) == false)
                {
                    this.queryESData(this.CurPageIndexFrom0, this.KeyWord);
                }
                else
                {
                    this.queryFrequentSearchKeyword();
                }

            }
        }
    }
}
