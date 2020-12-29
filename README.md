# ESTestWeb
ESImportData 是导入测试数据到ES的工具，可以在windows和linux中运行。参数是测试数据的目录。要求本机的ES要正常运行。ES地址hardcoding localhost:9200

ESTestWeb 是搜索源代码的网站，用asp.net core 写成，需要连接 mysql数据库。
mysql数据库的建表sql文件位于: ESTestWeb/ESTestWeb/data/中


用kibana建Index语句如下：

put /qgsourcefile
{
  
    "mappings" : {
      "properties" : {
        "fileContent" : {
          "type" : "text",
	  "analyzer" : "ik_smart",	
          "fields" : {
            "keyword" : {
              "type" : "keyword",
              "ignore_above" : 256
            }
          }
        },
        "fileLocation" : {
          "type" : "text",
          "fields" : {
            "keyword" : {
              "type" : "keyword",
              "ignore_above" : 256
            }
          }
        },
        "fileName" : {
          "type" : "text",
          "fields" : {
            "keyword" : {
              "type" : "keyword",
              "ignore_above" : 256
            }
          }
        },
        "fullFileName" : {
          "type" : "text",
          "fields" : {
            "keyword" : {
              "type" : "keyword",
              "ignore_above" : 256
            }
          }
        },
        "luaOrXml" : {
          "type" : "long"
        },
        "strategyName" : {
          "type" : "text",
          "fields" : {
            "keyword" : {
              "type" : "keyword",
              "ignore_above" : 256
            }
          }
        }
      }
    }

}
