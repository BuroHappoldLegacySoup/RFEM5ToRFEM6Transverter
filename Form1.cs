using Dlubal.RFEM5;
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

using rf5 = Dlubal.RFEM5;
using rf6 = Dlubal.WS.Rfem6;
using rf6Model = Dlubal.WS.Rfem6.Model;
using static RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter;



namespace RFEM5ToRFEM6Transverter
{
    public partial class Form1 : Form
    {
        String filePathRFEM5 = "";
        String filePathRFEM6 = "";


        rf5.IModel rf5Model = null;
        rf5.IModelData rf5ModelData = null;
        rf5.IResults rf5Result1 = null;
        rf5.IResults2 rf5Result2 = null;
        List<rf5.MemberForces[]> rf5MemberForces = new List<rf5.MemberForces[]>();

        public Form1()
        {
            InitializeComponent();
            this.Text = "RFEM5 to RFEM6 Transverter";
        }

 


        private void button3_Click(object sender, EventArgs e)
        {
            //Connect and Load From RFEM5
            this.rf5Model = RFEM5ConnectionHandler.SelectCurrentRFEM5Model();
            this.rf5ModelData = rf5Model.GetModelData();

            rf5Model.GetCalculation().CalculateApp();
            rf5Result1 = rf5Model.GetCalculation().GetResultsInFeNodes(rf5.LoadingType.LoadCaseType, 1);

            List<int> memberNumbers=this.rf5ModelData.GetMembers().Select(m => m.No).ToList();

            memberNumbers.ForEach(m =>this.rf5MemberForces.Add(this.rf5Result1.GetMemberInternalForces(m, rf5.ItemAt.AtNo, true)));

            //this.rf5MemberForces.Add(this.rf5Result1.GetMemberInternalForces(1, rf5.ItemAt.AtNo, true));

            RFEM5ConnectionHandler.application.UnlockLicense();


            RFEM6ConnectionHandler.SelectCurrentRFEM6Model();




            //Reading elements from RFEM5 model
            var rf5Nodes = rf5ModelData.GetNodes().ToList();
            var rf5Lines = rf5ModelData.GetLines().ToList();
            var rf5Materials = rf5ModelData.GetMaterials().ToList();
            var rf5Sections = rf5ModelData.GetCrossSections().ToList();
            var rf5Members = rf5ModelData.GetMembers().ToList();
            var rf5NodalSupports = rf5ModelData.GetNodalSupports().ToList();

            // Transverting elements from RFEM5 to RFEM6
            Dictionary<Point3D, int> rf5ToRf6NodeDictionary = new Dictionary<Point3D, int>(new Point3DComparer());
            rf5Nodes.ForEach(n => rf5ToRf6NodeDictionary[new Point3D() {X=n.X,Y=n.Y, Z=n.Z }]=n.No);

            Dictionary<int, List<int>> memberIdNodeIdsListDict = new Dictionary<int, List<int>>();
            var memberLines = rf5Members.Select(m =>rf5ModelData.GetLine(m.LineNo,rf5.ItemAt.AtNo).GetData()).ToList();
            memberLines.ForEach(ml => memberIdNodeIdsListDict[ml.No] = GetNodeIdsFromLine(ml));
            
            

                foreach (var rf5Node in rf5Nodes)
            {

                var rf6Node = RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.NodeTransverter(rf5Node);

                RFEM6ConnectionHandler.m_Model.set_node(rf6Node);


            }

            foreach (var rf5Line in rf5Lines)
            {

                var rf6Line = RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.LineTransverter(rf5Line);

                RFEM6ConnectionHandler.m_Model.set_line(rf6Line);


            }

            foreach (var rf5Material in rf5Materials)
            {


                var rf6Material = RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.MaterialTransverter(rf5Material);

                RFEM6ConnectionHandler.m_Model.set_material(rf6Material);
            }

            foreach (var rf5Section in rf5Sections)
            {


                var rf6Setion = RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.SectionTransverter(rf5Section);

                RFEM6ConnectionHandler.m_Model.set_section(rf6Setion);
            }

            //Import of Members
            foreach (var rf5Member in rf5Members)
            {


                var rf6Member = RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.MemberTransverter(rf5Member);

                RFEM6ConnectionHandler.m_Model.set_member(rf6Member);
            }

            foreach (var rf5NodalSupport in rf5NodalSupports)
            {


                var rf6NodalSupport = RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.NodalSupportTransverter(rf5NodalSupport);

                RFEM6ConnectionHandler.m_Model.set_nodal_support(rf6NodalSupport);
            }

            RFEM6ConnectionHandler.m_Model.set_static_analysis_settings(RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.StaticAnalysisSettingCreator());

            RFEM6ConnectionHandler.m_Model.set_load_case(RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.LoadCaseCreator());

            
            var rfNodalLoads=RFEM5ToRFEM6Transverter.Transverter.RFEM5ToRFEM6Transverter.ConstantShearForcesToPointLoad(this.rf5MemberForces, memberIdNodeIdsListDict);

            rfNodalLoads.ForEach(nl => RFEM6ConnectionHandler.m_Model.set_nodal_load(1,nl));

            RFEM6ConnectionHandler.DisconnectFromRFEM6Model();


        }
    }
}
