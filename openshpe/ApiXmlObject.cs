//Steven Rodriguez
//Simple class to hold all the items from the xml file
//This will be used to populate the query form dynamically

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace openshpe
{
    public class ApiXmlObject
    {
        public string Category { get; set; }
        public UrlClass urlClass { get; set; }

        public ApiXmlObject()
        {
            urlClass = new UrlClass();
        }

        public void Clear()
        {
            Category = null;
            urlClass.Clear();
        }

        //contains all the aspects of a url
        public class UrlClass
        {
            public List<string> URLS { get; set; }
            public List<string> HttpMethods { get; set;}
            public List<string> Parameters { get ; set; }

            public UrlClass()
            {
                URLS = new List<string>();
                HttpMethods = new List<string>();
                Parameters = new List<string>();
            }

            public void Clear()
            {
                URLS.Clear();
                HttpMethods.Clear();
                Parameters.Clear();
            }
        }
    }
}
