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
            HashSet<int> sections = new HashSet<int>() { 1, 2, 3, 4, 5, 6 };
            //Connect and Load From RFEM5
            this.rf5Model = RFEM5ConnectionHandler.SelectCurrentRFEM5Model();
            this.rf5ModelData = rf5Model.GetModelData();

            rf5Model.GetCalculation().CalculateApp();
            rf5Result1 = rf5Model.GetCalculation().GetResultsInFeNodes(rf5.LoadingType.LoadCaseType, 1);

            List<int> memberNumbers = this.rf5ModelData.GetMembers().Select(m => m.No).ToList();

            if (sections.Count() != 0)
            {
                memberNumbers = (rf5ModelData.GetMembers().ToList()).Where(m => sections.Contains(m.StartCrossSectionNo)).Select(m => m.No).ToList();
            }

            memberNumbers.ForEach(m => this.rf5MemberForces.Add(this.rf5Result1.GetMemberInternalForces(m, rf5.ItemAt.AtNo, true)));

            //this.rf5MemberForces.Add(this.rf5Result1.GetMemberInternalForces(1, rf5.ItemAt.AtNo, true));

            RFEM5ConnectionHandler.application.UnlockLicense();


            RFEM6ConnectionHandler.SelectCurrentRFEM6Model();



            //Reading elements from RFEM5 model
            List<Node> rf5Nodes = rf5ModelData.GetNodes().ToList();
            var rf5Lines = rf5ModelData.GetLines().ToList();
            var rf5Materials = rf5ModelData.GetMaterials().ToList();
            var rf5Sections = rf5ModelData.GetCrossSections().ToList();
            var rf5Members = rf5ModelData.GetMembers().ToList();
            var rf5NodalSupports = rf5ModelData.GetNodalSupports().ToList();

            if (sections.Count() != 0)
            {

                rf5Sections = sections.Select(s => rf5ModelData.GetCrossSection(s, rf5.ItemAt.AtNo).GetData()).ToList();
                rf5Materials = rf5Sections.Select(s => rf5ModelData.GetMaterial(s.MaterialNo, rf5.ItemAt.AtNo).GetData()).ToList();
                rf5Members = (rf5ModelData.GetMembers().ToList()).Where(m => sections.Contains(m.StartCrossSectionNo)).ToList();
                HashSet<int> memberLineIdSet = new HashSet<int>(rf5Members.Select(m => m.LineNo));
                rf5Lines = (rf5ModelData.GetLines().ToList()).Where(l => memberLineIdSet.Contains(l.No)).ToList();
                HashSet<int> nodeIdList = rf5Lines.SelectMany(l => GetNodeIdsFromLine(l)).ToHashSet();
                rf5Nodes = (rf5ModelData.GetNodes().ToList()).Where(n => nodeIdList.Contains(n.No)).ToList();

                //rf5Nodes = rf5ModelData.GetNodes().ToList();
                //rf5NodalSupports = rf5ModelData.GetNodalSupports().ToList();
                rf5NodalSupports = rf5ModelData.GetNodalSupports().ToList().Where(n => GetIdFromString(n.NodeList).ToHashSet().Intersect(nodeIdList).Count() > 0).ToList();


            }


            // Transverting elements from RFEM5 to RFEM6
            //Dictionary<Point3D, int> rf5ToRf6NodeDictionary = new Dictionary<Point3D, int>(new Point3DComparer());
            //rf5Nodes.ForEach(n => rf5ToRf6NodeDictionary[new Point3D() { X = n.X, Y = n.Y, Z = n.Z }] = n.No);

            //Dictionary<int, List<int>> memberLineIdNodeIdsListDict = new Dictionary<int, List<int>>();
            //List<Line> memberLines = rf5Members.Select(m => rf5ModelData.GetLine(m.No, rf5.ItemAt.AtNo).GetData()).ToList();
            //var memberLineNodes = memberLines.Select(ml => GetNodeIdsFromLine(ml)).ToList();
            //Dictionary<int, List<int>> memberIdNodeIdsListDict = new Dictionary<int, List<int>>();

            //memberLines.ForEach(ml => memberLineIdNodeIdsListDict[ml.No] = GetNodeIdsFromLine(ml));



            foreach (var rf5Node in rf5Nodes)
            {

                var rf6Node = NodeTransverter(rf5Node);

                RFEM6ConnectionHandler.m_Model.set_node(rf6Node);


            }

            foreach (var rf5Line in rf5Lines)
            {

                var rf6Line = LineTransverter(rf5Line);

                RFEM6ConnectionHandler.m_Model.set_line(rf6Line);


            }

            foreach (var rf5Material in rf5Materials)
            {


                var rf6Material = MaterialTransverter(rf5Material);

                RFEM6ConnectionHandler.m_Model.set_material(rf6Material);
            }

            foreach (var rf5Section in rf5Sections)
            {


                var rf6Setion = SectionTransverter(rf5Section);

                RFEM6ConnectionHandler.m_Model.set_section(rf6Setion);
            }

            //Import of Members
            foreach (var rf5Member in rf5Members)
            {


                var rf6Member = MemberTransverter(rf5Member);

                RFEM6ConnectionHandler.m_Model.set_member(rf6Member);
            }

            foreach (var rf5NodalSupport in rf5NodalSupports)
            {


                var rf6NodalSupport = NodalSupportTransverter(rf5NodalSupport);

                RFEM6ConnectionHandler.m_Model.set_nodal_support(rf6NodalSupport);
            }

            RFEM6ConnectionHandler.m_Model.set_static_analysis_settings(StaticAnalysisSettingCreator());

            RFEM6ConnectionHandler.m_Model.set_load_case(LoadCaseCreator());



            //create Member Node List Dictionary
            List<Line> memberLineList = rf5Members.Select(m => rf5ModelData.GetLine(m.No, rf5.ItemAt.AtNo).GetData()).ToList();
            List<List<int>> memberLineNodeList= memberLineList.Select(ml => GetIdFromString(ml.NodeList)).ToList();
            Dictionary<int, List<int>> memberIdNodeIdsListDict = new Dictionary<int, List<int>>();
            for (int i = 0; i < rf5Members.Count(); i++) {

                memberIdNodeIdsListDict[rf5Members[i].No] = memberLineNodeList[i];

            }




            var rfNodalLoads = ConstantShearForcesToPointLoad2(this.rf5MemberForces, memberIdNodeIdsListDict, rf5Nodes);

            ////var rfNodalLoads = ConstantShearForcesToPointLoad(this.rf5MemberForces, memberIdNodeIdsListDict);


            rfNodalLoads.ForEach(nl => RFEM6ConnectionHandler.m_Model.set_nodal_load(1, nl));

            RFEM6ConnectionHandler.DisconnectFromRFEM6Model();


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }






    }
}
