//Steven Rodriguez
//Rough conversion of xml data to datatable
//need to check all path returns to see if this will work

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace openshpe
{
    class XmlToDataSet
    {
        private string fileName;
        private XmlDocument xdoc;
        private DataSet dataSet;
        private FileStream fs;
        private DataTable dataTable;
        private int rowIndex = 0;
        private XmlElement elem;

        public XmlToDataSet(string fileName)
        {
            this.fileName = fileName;
            this.dataSet = new DataSet();
            this.dataTable = new DataTable();
            OpenXmlFile();
        }


        private void OpenXmlFile()
        {
            try
            {
                xdoc = new XmlDocument();
                fs = new FileStream(@fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                xdoc.Load(fs);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
        
        private void RecurseNode(XmlNodeList nodes)
        {
            XmlAttributeCollection attsList = null;
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (!dataTable.Columns.Contains(node.Name))
                    {
                        dataTable.Columns.Add(node.Name);
                    }
                    //only add row for each type returned not for all the child nodes
                    if (node.ParentNode.Name == elem.Name)
                    {
                        dataTable.Rows.Add();
                    }
                    if (node.InnerText != null)
                    {
                        dataTable.Rows[rowIndex].SetField(node.Name, node.InnerText);
                    }

                    attsList = node.Attributes;

                    if (attsList != null && attsList.Count > 0)
                    {
                        foreach (XmlAttribute att in attsList)
                        {
                            if (!dataTable.Columns.Contains(att.Name))
                            {
                                dataTable.Columns.Add(att.Name);
                            }

                            dataTable.Rows[rowIndex].SetField(att.Name, att.Value);
                        }
                    }

                    RecurseNode(node.ChildNodes);
                    //means we finished a subchild(a complete item request)
                    if (node.ParentNode.Name == elem.Name)
                        rowIndex++;
                }
            }
        }

        public DataSet Convert()
        {
            elem = xdoc.DocumentElement;
            XmlNodeList nodes = elem.ChildNodes;

            if (nodes != null && nodes.Count > 0)
            {
                RecurseNode(nodes);
            }

            dataTable.Columns.Remove(nodes[0].Name); // first column gets corrupted during recursion this will suffice for now
            dataSet.Tables.Add(dataTable);
            fs.Close();
            return dataSet;
        }
    }

}
