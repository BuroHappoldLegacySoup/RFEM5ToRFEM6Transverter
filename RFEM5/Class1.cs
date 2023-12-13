using Dlubal.RFEM5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rf5=Dlubal.RFEM5;

namespace RFEM5ToRFEM6Transverter.RFEM5
{



    public class Point3DComparer : IEqualityComparer<Point3D>
    {
        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public Point3DComparer()
        {
            
        }
        

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public bool Equals(Point3D p0, Point3D p1)
        {
            //return false;

            

            double tolerance = 0.0000001;

            if (Math.Abs(p0.X - p1.X) > tolerance)
            {

                return false;

            }
            if (Math.Abs(p0.Y - p1.Y) > tolerance)
            {

                return false;

            }
            if (Math.Abs(p0.Z - p1.Z) > tolerance)
            {

                return false;

            }


            return true;

        }

        /***************************************************/

        public int GetHashCode(Point3D load)
        {

            //return surfaceSupport.GetHashCode();

            return 0;

        }


    }

}
