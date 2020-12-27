using ESTest.control;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESTestWeb.NEST.Model
{
    /// <summary>
    /// 分页参数
    /// </summary>
    public class PageParam
    {
        /// <summary>
        /// 界面上显示用的PageIndex，从1开始
        /// </summary>
        public int PageIndex { get; set; }
        public int PageSize { get; set; } = ESProvider.PageSize;
        public long TotalCount { get; set; }
        public long TotalPage
        {
            get
            {
                long res = TotalCount % PageSize;
                return res == 0 ? TotalCount / PageSize : TotalCount / PageSize + 1;
            }
        }

        public long Took { get; set; } = 0;
    }
}
