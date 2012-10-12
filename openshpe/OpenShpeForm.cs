//Steven Rodriguez
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace openshpe
{
    public partial class OpenShpeForm : Form
    {
        private OpenShpeApiConfigReader apiConfigReader; //reads the xml file
        private List<ApiXmlObject> apiElementsList; //holds all the results from the xml
        private const string defaultApiConfigFile = "OpenShpeAPI.xml";
        private DataSet returnedDataSet = null; //data returned from query

        public OpenShpeForm()
        {
            InitializeComponent();
            loadConfig(defaultApiConfigFile);
            statusLabel.Text = "Ready";
        }

        public OpenShpeForm(string filename)
        {
            InitializeComponent();
            loadConfig(filename);
            statusLabel.Text = "Ready";
        }

        private void loadConfig(string filename)
        {
            apiConfigReader = new OpenShpeApiConfigReader(filename);
            apiElementsList = new List<ApiXmlObject>();
            apiElementsList = apiConfigReader.ReadConfigFile();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("About", "SHPE");
        }

        private void performRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QueryForm myQuery = new QueryForm(apiElementsList, this);
            myQuery.Show();
        }

        //populate the grid with the results
        public void FillDataGrid(DataSet ds)
        {
            returnedDataSet = ds;
            dataGrid.DataSource = returnedDataSet.Tables[0];
            statusLabel.Text = "Ready";
        }

        /// <summary>
        /// Sets the status on the bottom of the form
        /// </summary>
        /// <param name="status"></param>
        public void setStatus(string status)
        {
            statusLabel.Text = status;
            this.Update();
        }

        //export the results to a csv file
        private void exportToCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Performing Export";
            if (dataGrid.DataSource != null)
            {
                StringBuilder str = new StringBuilder();

                DataRowCollection rows = returnedDataSet.Tables[0].Rows;
                DataColumnCollection cols = returnedDataSet.Tables[0].Columns;

                //populate the headers
                foreach(DataColumn col in cols)
                {
                    str.Append(col.ColumnName + ",");
                }

                str.Append("\n");

                //populate the fields row by row
                foreach (DataRow row in rows)
                {
                    foreach (object field in row.ItemArray)
                    {
                        //some results have commas in them. this will force it into a new cell. dont want that.
                        string fieldStr = field.ToString().Replace(",", " ");                  
                        str.Append(fieldStr + ",");
                    }
                    str.Append("\n");
                }

                //write out csv 
                try
                {
                    System.IO.File.WriteAllText(@"DataExport.csv", str.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            statusLabel.Text = "Export Complete";
        }

    }
}
