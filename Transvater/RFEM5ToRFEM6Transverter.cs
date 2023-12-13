using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

using Dlubal.WS.Rfem6.Model;
using rf6 = Dlubal.WS.Rfem6.Model;
using rf5 = Dlubal.RFEM5;
using Dlubal.RFEM5;

namespace RFEM5ToRFEM6Transverter.Transverter
{
    public class RFEM5ToRFEM6Transverter
    {
        private static Dictionary<int, int> nodeIdTranslater = new Dictionary<int, int>();


        public static rf6.node NodeTransverter(rf5.Node rf5Node)
        {

            rf6.node rf6Node = new rf6.node()
            {
                no = rf5Node.No,
                coordinates = new vector_3d() { x = rf5Node.X, y = rf5Node.Y, z = rf5Node.Z },
                coordinate_system_type = rf6.node_coordinate_system_type.COORDINATE_SYSTEM_CARTESIAN,
                coordinate_system_typeSpecified = true,
                comment = ""

            };

            return rf6Node;
        }


        //public static rf6.section SectionTransverter(rf5.CrossSection rf5Section)
        //{

        //    rf6.section rf6Section = new rf6.section()
        //    {
        //        no = secNo,
        //        material = matNo,
        //        materialSpecified = true,
        //        name = bhSection.Name,
        //        typeSpecified = true,
        //        type = rfModel.section_type.TYPE_STANDARDIZED_STEEL,
        //        manufacturing_type = rfModel.section_manufacturing_type.MANUFACTURING_TYPE_HOT_ROLLED,
        //        manufacturing_typeSpecified = true,
        //        thin_walled_model = true,
        //        thin_walled_modelSpecified = true,
        //    };
        //    return rf6Section;
        //}

        public static rf6.material MaterialTransverter(rf5.Material rf5Material)
        {

            String[] splitRf5MaterialDesc = rf5Material.Description.Split('|')[0].Split(' ');
            String name = splitRf5MaterialDesc[1] + splitRf5MaterialDesc[2];

            rf6.material rf6Material = new rf6.material()
            {
                no = rf5Material.No,
                name = name,
                comment = "",
                material_type = splitRf5MaterialDesc[0].ToUpper().Equals("STEEL") ? rf6.material_material_type.TYPE_STEEL : rf6.material_material_type.TYPE_CONCRETE,
            };
            return rf6Material;
        }




        public static rf6.line LineTransverter(rf5.Line rf5Line)
        {

            List<List<int>> nestedNodeList = rf5Line.NodeList.Split(',').ToList().Select(n => GenerateSequenceFromRange(n)).ToList();
            List<int> nodeList = nestedNodeList.SelectMany(x => x).ToList();

            rf6.line rfLine = new rf6.line()
            {
                no = rf5Line.No,
                definition_nodes = nodeList.ToArray(),
                comment = "",
                type = rf6.line_type.TYPE_POLYLINE,
                typeSpecified = true,

            };

            return rfLine;
        }


        public static List<int> GetNodeIdsFromLine(rf5.Line rf5Line)
        {


            List<List<int>> nestedNodeList = rf5Line.NodeList.Split(',').ToList().Select(n => GenerateSequenceFromRange(n)).ToList();
            List<int> nodeList = nestedNodeList.SelectMany(x => x).ToList();

            return nodeList;

        }

        public static List<int> GenerateSequenceFromRange(string range)
        {
            string[] parts = range.Split('-');
            if (parts.Length != 2)
            {
                return new List<int>() { int.Parse(range) };
            }

            int start = int.Parse(parts[0]);
            int end = int.Parse(parts[1]);

            List<int> sequence = new List<int>();
            for (int i = start; i <= end; i++)
            {
                sequence.Add(i);
            }

            return sequence;
        }


        public static rf6.member MemberTransverter(rf5.Member rf5Member)
        {
            rf6.member rf6Member = new rf6.member()
            {
                no = rf5Member.No,
                type = rf6.member_type.TYPE_BEAM,
                typeSpecified = true,
                line = rf5Member.LineNo,
                lineSpecified = true,
                section_start = rf5Member.StartCrossSectionNo,
                section_startSpecified = true,
                comment = "",
            };

            return rf6Member;
        }

        public static rf6.section SectionTransverter(rf5.CrossSection rf5Section)
        {
            String name = rf5Section.Description.Split('|')[0];

            rf6.section rf6Section = new rf6.section()
            {
                no = rf5Section.No,
                material = rf5Section.MaterialNo,
                materialSpecified = true,
                name = name,
            };

            return rf6Section;
        }

        public static rf6.nodal_support NodalSupportTransverter(rf5.NodalSupport rf5NodalSupport)
        {

            List<List<int>> nestedNodeList = rf5NodalSupport.NodeList.Split(',').ToList().Select(n => GenerateSequenceFromRange(n)).ToList();
            List<int> nodeList = nestedNodeList.SelectMany(x => x).ToList();

            rf6.nodal_support rfNodelSupport = new rf6.nodal_support()
            {
                no = rf5NodalSupport.No,
                name = rf5NodalSupport.Comment,
                nodes = nodeList.ToArray(),
                spring = new rf6.vector_3d() { x = double.PositiveInfinity, y = double.PositiveInfinity, z = double.PositiveInfinity },
                rotational_restraint = new rf6.vector_3d() { x = double.PositiveInfinity, y = double.PositiveInfinity, z = double.PositiveInfinity },

            };

            return rfNodelSupport;
        }


        public static rf6.static_analysis_settings StaticAnalysisSettingCreator()
        {

            rf6.static_analysis_settings analysis = new static_analysis_settings()
            {
                no = 1,
                analysis_type = static_analysis_settings_analysis_type.GEOMETRICALLY_LINEAR,
                analysis_typeSpecified = true,
            };

            return analysis;
        }

        public static rf6.load_case LoadCaseCreator()
        {


            load_case rfLoadCase = new rf6.load_case()
            {
                no = 1,
                name = "ArnesLC",
                static_analysis_settings = 1,
                analysis_type = load_case_analysis_type.ANALYSIS_TYPE_STATIC,
                analysis_typeSpecified = true,
                static_analysis_settingsSpecified = true,
                action_category = "ACTION_CATEGORY_PERMANENT_G",
                calculate_critical_load = true,
                calculate_critical_loadSpecified = true,
                stability_analysis_settings = 1,
                stability_analysis_settingsSpecified = true,
            };


            return rfLoadCase;

        }

        public static List<rf6.nodal_load> ConstantShearForcesToPointLoad(List<rf5.MemberForces[]> rf5MemberForcesList, Dictionary<int, List<int>> memberIdNodeIdsListDict)
        {

            var nodalLoadList = new List<rf6.nodal_load>();
            //int nodalLoadCounter = 1;

            //foreach(var rfMF in rf5MemberForces.ToList())
            //{
            //    var loc=rfMF.Location;


            //}
            int globalNodalCounter=0;

            foreach (var rf5MemberForces in rf5MemberForcesList)
            {
                var nodeIds = memberIdNodeIdsListDict[rf5MemberForces[0].MemberNo].ToArray();
                int nodalLoadCounter = 0;
                for (int i = 1; i < rf5MemberForces.Count()-1; i++)
                {



                    int prevIndex = i - 1;

                    rf5.Point3D prevForces = rf5MemberForces[prevIndex].Forces;
                    rf5.Point3D currentForce = rf5MemberForces[i].Forces;

                    double tol = 0.00001;

                    if ((Math.Abs(prevForces.X - currentForce.X) > tol) || (Math.Abs(prevForces.Y - currentForce.Y) > tol) || (Math.Abs(prevForces.Z - currentForce.Z) > tol))
                    {

                        nodalLoadCounter++;
                        globalNodalCounter++;

                        nodal_load rf6NodalLoad = new rf6.nodal_load()
                        {

                            no = globalNodalCounter,
                            nodes = new int[] { nodeIds[nodalLoadCounter] },
                            components_force_x = prevForces.X - currentForce.X,
                            components_force_xSpecified = true,
                            components_force_y = prevForces.Y - currentForce.Y,
                            components_force_ySpecified = true,
                            components_force_z = prevForces.Z - currentForce.Z,
                            components_force_zSpecified = true,
                            //components_moment_x = prevMoment.X - currentMoment.X,
                            //components_moment_xSpecified = true,
                            //components_moment_y = prevMoment.Y - currentMoment.Y,
                            //components_moment_ySpecified = true,
                            //components_moment_z = prevMoment.Z - currentMoment.Z,
                            //components_moment_zSpecified = true,
                            load_type = nodal_load_load_type.LOAD_TYPE_COMPONENTS,
                            load_typeSpecified = true,
                        };

                        nodalLoadList.Add(rf6NodalLoad);
                    }
                    else if (i == rf5MemberForces.Count() - 2 && (rf5MemberForces[i - 1].Forces.X != 0 || rf5MemberForces[i - 1].Forces.Y != 0 || rf5MemberForces[i - 1].Forces.Z != 0))
                    {

                        nodalLoadCounter++;
                        globalNodalCounter++;


                        nodal_load rf6NodalLoad = new rf6.nodal_load()
                        {

                            no = globalNodalCounter,
                            nodes = new int[] { nodeIds[nodalLoadCounter] },
                            components_force_x = currentForce.X,
                            components_force_xSpecified = true,
                            components_force_y = currentForce.Y,
                            components_force_ySpecified = true,
                            components_force_z = currentForce.Z,
                            components_force_zSpecified = true,
                            //components_moment_x = prevMoment.X - currentMoment.X,
                            //components_moment_xSpecified = true,
                            //components_moment_y = prevMoment.Y - currentMoment.Y,
                            //components_moment_ySpecified = true,
                            //components_moment_z = prevMoment.Z - currentMoment.Z,
                            //components_moment_zSpecified = true,
                            load_type = nodal_load_load_type.LOAD_TYPE_COMPONENTS,
                            load_typeSpecified = true,
                        };

                        nodalLoadList.Add(rf6NodalLoad);




                    }




                }




            }


            return nodalLoadList;

        }

        private static Point3D Interpolate(Point3D start, Point3D end, double t)
        {


            double x = (start.X + end.X) * t;
            double y = (start.Y + end.Y) * t;
            double z = (start.Z + end.Z) * t;



            return new Point3D() { X = x, Y = y, Z = z };
        }
    }



}
