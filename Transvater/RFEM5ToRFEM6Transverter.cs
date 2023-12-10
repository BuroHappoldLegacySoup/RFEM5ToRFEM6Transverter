using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

using Dlubal.WS.Rfem6.Model;
using rf6 =Dlubal.WS.Rfem6.Model;
using rf5=Dlubal.RFEM5;



namespace RFEM5ToRFEM6Transverter.Transverter
{
    public class RFEM5ToRFEM6Transverter
    {
        private static Dictionary<int, int> nodeIdTranslater = new Dictionary<int, int>();


        public static rf6.node NodeTransverter(rf5.Node rf5Node) { 
        
            rf6.node rf6Node = new rf6.node() {
                no = rf5Node.No,
                coordinates =  new vector_3d() { x = rf5Node.X, y = rf5Node.Y, z = rf5Node.Z },
                coordinate_system_type = rf6.node_coordinate_system_type.COORDINATE_SYSTEM_CARTESIAN,
                coordinate_system_typeSpecified = true,
                comment = ""

            };
            
            return rf6Node;
        }

        public static rf6.line LineTransverter(rf5.Line rf5Line)
        {



            rf6.line rfLine = new rf6.line()
            {
                no = rf5Line.No,
                definition_nodes = rf5Line.NodeList.Split(',').Select(x => int.Parse(x)).ToArray(),
                comment = "",
                type = rf6.line_type.TYPE_POLYLINE,
                typeSpecified = true,

            };

            return null;
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
                section_start = 1,
                section_startSpecified = true,
                comment = "",
            };

            return rf6Member;
        }

    }


}
