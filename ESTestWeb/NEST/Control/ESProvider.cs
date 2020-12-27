using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ESTest.model;
using Nest;

namespace ESTest.control
{
    public class ESProvider : IESProvider
    {
        public ElasticClient m_client;
        //public static string strIndexName = @"meetup".ToLower();
        public static string strDocType = "events".ToLower();

        /// <summary>
        /// 每页数据10条记录
        /// </summary>
        public const int PageSize = 7;

        public ESProvider()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
(sender, cert, chain, errors) => true;

            m_client = new ElasticClient();
        }

        public bool PopulateIndex(MeetupEvents meetupevent, string strIndexName)
        {
            //var index = client.Index(meetupevent, i => i.Index(strIndexName).Type(strDocType).Id(meetupevent.eventid));
            var index = m_client.Index(meetupevent, i => i.Index(strIndexName).Id(meetupevent.eventid));
            //return index.Created;
            return index.IsValid;
        }

        public bool PopulateIndex(QGSSLuaXmlFile ssSourceFile, string strIndexName)
        {
            //var index = client.Index(meetupevent, i => i.Index(strIndexName).Type(strDocType).Id(meetupevent.eventid));
            var index = m_client.Index(ssSourceFile, i => i.Index(strIndexName).Id(ssSourceFile.FullFileName));
            //return index.Created;
            return index.IsValid;
        }

        public bool BulkPopulateIndex(List<MeetupEvents> posts, string strIndexName)
        {
            var bulkRequest = new BulkRequest(strIndexName) { Operations = new List<IBulkOperation>() };
            var idxops = posts.Select(o => new BulkIndexOperation<MeetupEvents>(o) { Id = o.eventid }).Cast<IBulkOperation>().ToList();
            bulkRequest.Operations = idxops;
            var response = m_client.Bulk(bulkRequest);
            return response.IsValid;
        }

        /// <summary>
        /// 查询ES数据
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="idxName"></param>
        /// <param name="pageIndex">分页的页号</param>
        /// <returns></returns>
        public SearchRs QueryByKeyWord(string keyWord, string idxName, int pageIndex)
        {
            SearchRs searchRs = new SearchRs();
            //高亮关键字？
            var rs = m_client.Search<QGSSLuaXmlFile>(s => s.Index(idxName).
            Query(
            q => q.Match(qs => qs.Query(keyWord).Operator(Operator.Or).Field(fd => fd.FileContent)))
            .Size(PageSize)
            .From(PageSize * pageIndex)
            .Highlight(h => h
                        .PreTags("<strong>")
                        .PostTags("</strong>")
                        .Encoder(HighlighterEncoder.Html)
                        .Fields(
                            fs => fs.Field(p => p.FileContent)
                        )
                    )
            );


            //高亮关键字内容存到字段 FileContentHighlight 
            string fileContent = string.Empty;
            foreach (var hit in rs.Hits)
            {
                foreach (var highlightField in hit.Highlight)
                {
                    if (highlightField.Key == "fileContent")
                    {
                        hit.Source.FileContentHighlight = string.Empty;
                        foreach (var highlight in highlightField.Value)
                        {
                            hit.Source.FileContentHighlight += highlight.ToString();
                        }
                    }
                }
            }

            searchRs.Documents = rs.Documents;
            searchRs.Hits = rs.Hits;

            searchRs.PageData = new ESTestWeb.NEST.Model.PageParam();

            //填充PageParam
            searchRs.PageData.PageIndex = pageIndex+1;
            searchRs.PageData.PageSize = PageSize;
            searchRs.PageData.TotalCount = rs.Total;
            searchRs.PageData.Took = rs.Took;
            //var rs = m_client.Search<QGSSLuaXmlFile>(s => s.Index(idxName).Query(q => q.QueryString(qs => qs.Query(keyWord).DefaultOperator(Operator.Or))));
            return searchRs;
        }

        public async Task<SearchRs> QueryByKeyWordAsync(string keyWord, string idxName,int pageIndex)
        {
            return await Task.Run(() =>
             {
                 SearchRs searchRs = new SearchRs();

                 var rs = m_client.Search<QGSSLuaXmlFile>(s => s.Index(idxName).Query(q => q.QueryString(qs => qs.Query(keyWord).DefaultOperator(Operator.Or)))
                 .Size(PageSize)
                 .From(PageSize * pageIndex));
                 searchRs.Documents = rs.Documents;

                 return searchRs;
             });
        }

        public IReadOnlyCollection<AnalyzeToken> ik_chnWord(string keyWord)
        {
            var rs = m_client.Indices.Analyze(a => a.Analyzer("ik_smart").Text(keyWord));
            return rs.Tokens;
        }
    }
}
