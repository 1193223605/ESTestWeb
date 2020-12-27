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
    public class ESProvider
    {
        public ElasticClient m_client ;
        //public static string strIndexName = @"meetup".ToLower();
        public static string strDocType = "events".ToLower();

        public ESProvider()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
(sender, cert, chain, errors) => true;

            Uri uri = new Uri("http://localhost:9200");
            m_client = new ElasticClient(uri);
        }

        public bool PopulateIndex(MeetupEvents meetupevent,string strIndexName)
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

        public IReadOnlyCollection<QGSSLuaXmlFile> QueryByKeyWord(string keyWord,string idxName)
        {
            var rs = m_client.Search<QGSSLuaXmlFile>(s => s.Index(idxName).Query(q => q.QueryString(qs => qs.Query(keyWord).DefaultOperator(Operator.Or))));
            return rs.Documents;
        }

        public IReadOnlyCollection<AnalyzeToken> ik_chnWord(string keyWord)
        {
            var rs = m_client.Indices.Analyze(a => a.Analyzer("ik_smart").Text(keyWord));
            return rs.Tokens;
        }
    }
}
