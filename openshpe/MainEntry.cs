using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace openshpe
{
    class MainEntry
    {
        static void Main(string[] args)
        {
            Application.Run(new OpenShpeForm("OpenShpeAPI.xml"));
        }
    }
}
