using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace WebStack.QA.Test.OData
{
    public static class ODataHelper
    {
        public static string GetHttpPrefix(string httpMethod)
        {
            switch (httpMethod)
            { 
                case "GET":
                    return "Get";
                case "POST":
                    return "Post";
                case "PUT":
                    return "Put";
                case "MERGE":
                case "PATCH":
                    return "Patch";
                case "DELETE":
                    return "Delete";
                default:
                    return null;
            }
        }
    }
}
