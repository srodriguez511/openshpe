//Steven Rodriguez
//Reads the api xml file
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace openshpe
{
    class OpenShpeApiConfigReader
    {
        private string xmlFileName;
        private XmlDocument xmlDoc;
        private List<ApiXmlObject> resultList;

        public OpenShpeApiConfigReader(string fileName)
        {
            xmlDoc = new XmlDocument();
            xmlFileName = fileName;
            resultList = new List<ApiXmlObject>();
        }

        //quick and dirty
        public List<ApiXmlObject> ReadConfigFile()
        {
            try
            {
                xmlDoc.Load(xmlFileName);

                //First grab all the types of contexts child nodes
                XmlNodeList contextNodeList = xmlDoc.GetElementsByTagName("context");
                XmlNodeList contextChildNodesList = null;
                XmlNodeList itemsNodeList = null;
                XmlNodeList innerItemsNodeList = null;
                XmlAttributeCollection innerItemsAttributes = null;
                ApiXmlObject tempHolder = null;
                
                //there should only be one
                foreach (XmlNode node in contextNodeList)
                {
                    contextChildNodesList = node.ChildNodes;
                }

                //now go through all of the children
                string urlBase; //this is part of the url
                foreach (XmlNode node in contextChildNodesList)
                {
                    urlBase = "/" + node.Name;

                    itemsNodeList = node.ChildNodes;
                    //go through all the item nodes 
                    //each item node is an object in our list
                    foreach (XmlNode itemNode in itemsNodeList)
                    {
                        //inner items are <name> and <url>
                        innerItemsNodeList = itemNode.ChildNodes;
                        tempHolder = new ApiXmlObject();
                        foreach (XmlNode innerItemNode in innerItemsNodeList)
                        {
                            if (innerItemNode.Name == "name")
                            {
                                tempHolder.Category = innerItemNode.InnerText; 
                            }
                                //url has attributes as well as inner text
                            else if (innerItemNode.Name == "URL")
                            {
                                //set the url
                                tempHolder.urlClass.URLS.Add(urlBase + innerItemNode.InnerText);

                                //methods and parameters are attributes
                                innerItemsAttributes = innerItemNode.Attributes;
                                if (innerItemsAttributes.Count > 0)
                                {
                                    foreach (XmlAttribute att in innerItemsAttributes)
                                    {
                                        //add even if blank so the size of the collections are always the same
                                        if (att.Name == "Methods")
                                        {
                                            tempHolder.urlClass.HttpMethods.Add(att.Value);
                                        }
                                        else if (att.Name == "Parameters")
                                        {
                                            tempHolder.urlClass.Parameters.Add(att.Value);
                                        }
                                    }
                                }
                            }
                        }

                        //add the item to the full list
                        resultList.Add(tempHolder);
                    }

                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return resultList;
        }
    }
}
