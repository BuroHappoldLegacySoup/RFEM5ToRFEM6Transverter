using RFEM5ToRFEM6Transverter.RFEM5;
using RFEM5ToRFEM6Transverter.RFEM6;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using rf5=Dlubal.RFEM5;
using rf6 = Dlubal.WS.Rfem6;
using rf6Model = Dlubal.WS.Rfem6.Model;



namespace RFEM5ToRFEM6Transverter
{
    public partial class Form1 : Form
    {
        String filePathRFEM5="";
        String filePathRFEM6 = "";


        rf5.IModel rf5Model = null;
        rf5.IModelData rf5ModelData = null;
        rf5.IResults rf5Result1 = null;
        rf5.IResults2 rf5Result2 = null;
        rf5.MemberForces[] rf5MemberForces = null;

        public Form1()
        {
            InitializeComponent();
            this.Text = "RFEM5 to RFEM6 Transverter";
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
            this.rf5Model =RFEM5ConnectionHandler.ConnectRFEM5(filePathRFEM5);
            this.rf5ModelData = rf5Model.GetModelData();

            var temt=rf5ModelData.GetNodes();
            rf5Model.GetCalculation().CalculateApp();
            rf5Result1 = rf5Model.GetCalculation().GetResultsInFeNodes(rf5.LoadingType.LoadCaseType, 1);

            this.rf5MemberForces=this.rf5Result1.GetMemberInternalForces(1,rf5.ItemAt.AtNo,true);

            RFEM5ConnectionHandler.application.UnlockLicense();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.label1.Text = "Currently no RFEM6 Selected ";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.openFileDialog2.Filter = "RF6 Files (*.rf6)|*.rf6";

            // Show the dialog and check if the result is OK
            if (this.openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file name
                this.filePathRFEM6 = openFileDialog2.FileName;
                //this.label1.Text = "File selected: " + this.filePathRFEM6.Split('\\').Last();

            }
            RFEM6ConnectionHandler.ConnectRFEM6(filePathRFEM6);

            RFEM6ConnectionHandler.DisconnectFromRFEM6Model();

        }

        private void button3_Click(object sender, EventArgs e)
        {

            RFEM6ConnectionHandler.ConnectToRFEM6Model();

           
            var rf5Nodes=rf5ModelData.GetNodes().ToList();
          
            //rf5Model.GetCalculation().CalculateApp();
            var res=rf5Model.GetCalculation().GetResultsInFeNodes(rf5.LoadingType.LoadCaseType,1);

            foreach (var rf5Node in rf5Nodes) {

                var rf6Node=RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.NodeTransverter(rf5Node);

                RFEM6ConnectionHandler.m_Model.set_node(rf6Node);

                
            }

           


            //var node= new rf6Model.node()
            //{
            //    no = 3,
            //    coordinates = new rf6Model.vector_3d() { x = 30, y = 10, z = 10 },
            //    coordinate_system_type = rf6Model.node_coordinate_system_type.COORDINATE_SYSTEM_CARTESIAN,
            //    coordinate_system_typeSpecified = true,
            //    comment = ""

            //};



            //RFEM6ConnectionHandler.m_Model.set_node(node);

            RFEM6ConnectionHandler.DisconnectFromRFEM6Model();


        }
    }
}
