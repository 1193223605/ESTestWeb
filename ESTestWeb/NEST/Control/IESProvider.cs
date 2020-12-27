using ESTest.model;
using ESTestWeb.NEST.Model;
using Nest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESTest.control
{
    public interface IESProvider
    {
        bool BulkPopulateIndex(List<MeetupEvents> posts, string strIndexName);
        IReadOnlyCollection<AnalyzeToken> ik_chnWord(string keyWord);
        bool PopulateIndex(MeetupEvents meetupevent, string strIndexName);
        bool PopulateIndex(QGSSLuaXmlFile ssSourceFile, string strIndexName);
        SearchRs QueryByKeyWord(string keyWord, string idxName, int pageIndex);

        Task<SearchRs> QueryByKeyWordAsync(string keyWord, string idxName, int pageIndex);
    }

    public class SearchRs
    {
        public IReadOnlyCollection<QGSSLuaXmlFile> Documents;
        public IReadOnlyCollection<IHit<QGSSLuaXmlFile>> Hits;
        public PageParam PageData { get; set; }
    }
}