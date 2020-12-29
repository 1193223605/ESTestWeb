using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESTestWeb.Model
{
    public class FrequentSearchKeyword
    {
        public string ESIndexName { get; set; }

        public string KeyWord { get; set; }

        public int Count { get; set; }

        public DateTime LastSearchTime { get; set; }

    }
}
