//Steven Rodriguez
//Dynamically populates all the drop down boxes in flow order from the xml file
//First populate the categories. Once selected populate the urls
//After selecting a url the methods for that url are populated
//The parameters are also populated

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace openshpe
{
    public partial class QueryForm : Form
    {
        public List<ApiXmlObject> apiElementsList { get; set; } // complete list of everything

        private string id;
        private OpenShpe openShpeAPI;
        private OpenShpeForm openShpeForm;
        private ApiXmlObject currentSelectedObject;
        private Dictionary<Label, TextBox> parameterTextBoxList;

        private const string IDTAG = "{id}";

        public QueryForm(List<ApiXmlObject> myList, OpenShpeForm openShpeForm)
        {
            InitializeComponent();
            this.apiElementsList = myList;
            this.openShpeForm = openShpeForm;
            this.parameterTextBoxList = new Dictionary<Label,TextBox>();
            //First thing we do is populate all the possible categories obtained from the xml file
            PopulateCategory();
        }

        private void PopulateCategory()
        {
            //Get all the category names and populate the drop down
            foreach (ApiXmlObject obj in apiElementsList)
            {
                CategoryComboBox.Items.Add(obj.Category);
            }
        }

        //Once they selected a category we can then determine which URLS are associated with that category
        private void CategoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            URLComboBox.Enabled = true;
            PopulateURL();
        }

        private void ClearParameters()
        {
            if (parameterTextBoxList.Count > 0)
            {
                foreach (KeyValuePair<Label, TextBox> pair in parameterTextBoxList)
                {
                    this.Controls.Remove(pair.Key);
                    this.Controls.Remove(pair.Value);
                }
                parameterTextBoxList.Clear();
            }
        }

        //Get all the urls associated with the selected category
        private void PopulateURL()
        {
            //there may be something in there already from a previous selection
            URLComboBox.Items.Clear();
            URLComboBox.Text = "";
            MethodComboBox.Items.Clear();
            MethodComboBox.Text = "";
            ClearParameters();
            IDTextBox.Enabled = false;

            //find by selected
            currentSelectedObject = apiElementsList.Find(FindByCategory);

            if (currentSelectedObject != null)
            {
                //hold all the urls possible for this category
                List<string> urls = currentSelectedObject.urlClass.URLS;

                if (urls != null && urls.Count > 0)
                {
                    foreach (string s in urls)
                    {
                        URLComboBox.Items.Add(s);
                    }
                }
            }
        }

        // Explicit predicate delegate to determine if this current selected item is equal
        // to in the list
        private bool FindByCategory(ApiXmlObject obj)
        {
            string selectedItem = CategoryComboBox.SelectedItem.ToString();
            return (obj.Category == selectedItem);
        }

        //We selected a particular url now we must update the methods and possible parameters
        private void URLComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            IDTextBox.Enabled = false;
            CheckUrlForIds();
            MethodComboBox.Enabled = true;
            PopulateMethods();
            ClearParameters();
            PopulateParameters();
        }

        //Some URLS contain {id}. We must have the user populate this value.
        private void CheckUrlForIds()
        {
            string url = URLComboBox.SelectedItem.ToString();
            if (url.Contains(IDTAG))
            {
                IDTextBox.Enabled = true;
            }
        }

        //this is very ugly. can be fixed by setting all of the url types into a single structure
        private void PopulateMethods()
        {
            MethodComboBox.Items.Clear();
            MethodComboBox.Text = "";

            //for now only get is supported so only show that
            //SR_TEMP
            MethodComboBox.Items.Add("Get");

        }

        //populate the parameters combo box
        private void PopulateParameters()
        {
            //this is the currently objects parameter list
            List<string> parameters = currentSelectedObject.urlClass.Parameters;
            
            //now determine which url we hve and use that to index the correct parameter.
            int index = URLComboBox.SelectedIndex;

            if (index != -1)
            {
                string parameterString = parameters[index];

                List<string> parsedList = ParseParameters(parameterString);
                
                CreateParameterTextBoxes(parsedList);
            }
        }

        //Dynamically create labels and textboxes for the parameters
        private void CreateParameterTextBoxes(List<string> parsedList)
        {
            //label 4 "Parameters"
            Point parameterLabelPoint = label4.Location;

            int increaseYFactor = 30;
            int yPosition = parameterLabelPoint.Y;
            int xPosition = parameterLabelPoint.X;

            foreach (string s in parsedList)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    Label label = new Label();

                    label.AutoSize = true;
                    label.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    label.Location = new System.Drawing.Point(xPosition, yPosition + increaseYFactor); //leave space between the labels
                    label.Name = s;
                    label.Size = new System.Drawing.Size(105, 24);
                    label.TabIndex = 3;
                    label.Text = s.Trim();
                    this.Controls.Add(label);

                    TextBox textBox = new TextBox();
                    textBox.Enabled = true;
                    textBox.Location = new System.Drawing.Point(IDTextBox.Location.X, yPosition + increaseYFactor);
                    textBox.Name = s;
                    textBox.ReadOnly = false;
                    textBox.Size = new System.Drawing.Size(140, 20); //align them with the other textboxes
                    this.Controls.Add(textBox);

                    parameterTextBoxList.Add(label, textBox);

                    //update the current lowest y position
                    yPosition = label.Location.Y;
                }
            }
        }

        private List<string> ParseParameters(string str)
        {
            string[] list = str.Split(',');
            return new List<string>(list);
        }

        //Checks if we need to update the url with the id
        private string GetServicePath()
        {
            string s = URLComboBox.SelectedItem.ToString();
            if (s.Contains(IDTAG))
            {
                s = s.Replace(IDTAG, id);
            }
            return s;
        }

        //Get the current method input
        private string GetMethod()
        {
            string method = MethodComboBox.SelectedItem.ToString();

            switch (method)
            {
                case "Get":
                    return "G";
                case "Put":
                    return "P";
                case "Post":
                    return "O";
                case "Delete":
                    return "D";
                default:
                    return "G";
            }
        }

        //validate all the selections
        private bool ValidateQueryFields()
        {
            if (CategoryComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please selected a category", "Invalid Entry");
                return false;
            }

            if (URLComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please selected a URL", "Invalid Entry");
                return false;
            }

            if (MethodComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please selected a Method", "Invalid Entry");
                return false;
            }

            //validate the id field
            if (IDTextBox.Enabled && string.IsNullOrEmpty(IDTextBox.Text))
            {
                MessageBox.Show("Please enter a valid ID", "Invalid Entry");
                return false;
            }

            if (!ValidateParameters())
            {
                MessageBox.Show("Please enter a value for all parameters", "Invalid Entry");
                return false;
            }
            return true;
        }

        private bool ValidateParameters()
        {
            //if we even have any parameters
            if (parameterTextBoxList.Count > 0)
            {
                //go through each text box and make sure they are not empty
                foreach (KeyValuePair<Label, TextBox> pair in parameterTextBoxList)
                {
                    if(string.IsNullOrEmpty(pair.Value.Text))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        //return all the parameters for the api
        private SortedDictionary<string, string> GetParameters()
        {
            SortedDictionary<string, string> list = new SortedDictionary<string,string>();

            //check if we even have parameters for this object
            if (currentSelectedObject.urlClass.Parameters.Count > 0)
            {
                if(parameterTextBoxList != null){
                    foreach(KeyValuePair<Label, TextBox> pair in parameterTextBoxList)
                    {
                        string paramName = pair.Key.Name;
                        paramName = paramName.Split('(')[0]; //drop any of the (a,b) parts
                        string paramValue = pair.Value.Text;

                        list.Add(paramName.Trim(), paramValue.Trim());
                    }
                }
                return list;
            }

            return null;
        }

        //Calls the api to perform the http request for the information
        private void performQuery_Click(object sender, EventArgs e)
        {
            //validate the query here
            if (ValidateQueryFields())
            {
                openShpeForm.setStatus("Requesting information from server");
                openShpeAPI = new OpenShpe("openshpe.staging.shpe.org", "b4FYkwOxPktPGbDUiw2S", "OImgt?5oin[a<T_Qo71SD*IKiG39MFNP*JPADAHK", true);
                openShpeAPI.SetHttpMethod(GetMethod());
                Stream result = null;
                try
                {
                    result = openShpeAPI.GetRestResponse(GetServicePath(), GetParameters());
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                if (result != null)
                {
                    openShpeForm.setStatus("Populating Grid. This may take a moment!");
                    //Must use string builder these results can be very long
                    StreamReader reader = new StreamReader(result);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(reader.ReadToEnd());

                    DataSet dataSet = new DataSet();
                    StringReader stringReader = new StringReader(sb.ToString());
                    File.WriteAllText("XmlDump.xml", sb.ToString());
                    XmlToDataSet xds = new XmlToDataSet("XmlDump.xml");
                    dataSet = xds.Convert();
                    openShpeForm.FillDataGrid(dataSet);

                }
            }
        }

        private void IDTextBox_TextChanged(object sender, EventArgs e)
        {
            id = IDTextBox.Text;
        }
    }
}
