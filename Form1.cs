using RFEM5ToRFEM6Transverter.RFEM5;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFEM5ToRFEM6Transverter
{
    public partial class Form1 : Form
    {
        String filePathRFEM5="";

        public Form1()
        {
            InitializeComponent();
        }

        private void browsRFEM5File(object sender, EventArgs e)
        {
            // Set the filter before showing the dialog
            this.openFileDialog1.Filter = "RFEM5 Files (*.rf5)|*.rf5";

            // Show the dialog and check if the result is OK
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file name
                this.filePathRFEM5 = openFileDialog1.FileName;
                this.label1.Text = "File selected: " +this.filePathRFEM5.Split('\\').Last();
                
            }

            RFEM5ConnectionHandler.ConnectRFEM5(filePathRFEM5);

        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.label1.Text = "Currently no RFEM6 Selected ";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
