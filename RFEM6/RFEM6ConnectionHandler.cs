using Dlubal.WS.Rfem6.Application;
using Dlubal.WS.Rfem6.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RFEM5ToRFEM6Transverter.RFEM6
{
    internal class RFEM6ConnectionHandler
    {

        public static RfemModelClient m_Model { get; set; } = null;

        private static EndpointAddress Address { get; set; } = new EndpointAddress("http://localhost:8081");

        private static BasicHttpBinding Binding
        {
            get
            {
                BasicHttpBinding binding = new BasicHttpBinding { SendTimeout = new TimeSpan(0, 0, 180), UseDefaultWebProxy = true, };
                return binding;
            }
        }
        private static RfemApplicationClient application = new RfemApplicationClient(Binding, Address);
        private static string modelUrl = "";


        //public static void ConnectRFEM6(String filePath)
        //{


        //    if (IsRFEM6ApplicationRunning()) { }

        //    else
        //    {

        //        String appPath = @"C:\Program Files\Dlubal\RFEM 6.04\bin\RFEM6.exe";
        //        var process = Process.Start(appPath);
        //        Thread.Sleep(50000);
        //        //System.GC.Collect();
        //        //System.GC.WaitForPendingFinalizers();
        //    }

        //    RfemModelClient model;

        //    modelUrl = "";
        //    if (filePath != "")
        //    {
        //        //application.get_model_list_with_indexes().ToList().ForEach(m=>application.close_model(m.index,false));
        //        String[] mdl = application.get_model_list();
        //        //if (mdl.Count() > 0) { application.get_model_list_with_indexes().ToList().ForEach(m => application.close_model(m.index, false)); }
        //        modelUrl = application.open_model(filePath);
        //        //Thread.Sleep(10000);
        //    }
        //    else
        //    {
        //        modelUrl = application.new_model("New Model");
        //        Thread.Sleep(10000);
        //    }
        //    Thread.Sleep(10000);
        //    //System.GC.Collect();
        //    //System.GC.WaitForPendingFinalizers();
        //    model = new RfemModelClient(Binding, new EndpointAddress(modelUrl));

        //    //model.set_node(new node()
        //    //{
        //    //    no = 3,
        //    //    coordinates = new vector_3d() { x = 30, y = 10, z = 10 },
        //    //    coordinate_system_type = node_coordinate_system_type.COORDINATE_SYSTEM_CARTESIAN,
        //    //    coordinate_system_typeSpecified = true,
        //    //    comment = ""

        //    //});

        //    m_Model = model;
        //}


        public static RfemModelClient SelectCurrentRFEM6Model() {


            string modelUrl = application.get_active_model();
            m_Model = new RfemModelClient(Binding, new EndpointAddress(modelUrl));

            return m_Model;
        }


        public static void DisconnectFromRFEM6Model()
        {
            m_Model.close_connection();
            m_Model = null;
        }

        public static void ConnectToRFEM6Model()
        {
            string modelUrl = application.get_active_model();
            m_Model = new RfemModelClient(Binding, new EndpointAddress(modelUrl));
        }


        //private static bool IsRFEM6ApplicationRunning()
        //{

        //    try
        //    {

        //        if (System.Diagnostics.Process.GetProcesses().Any(p => p.ProcessName.ToLower() == "rfem6")) { return true; }
        //        else { return false; }

        //    }
        //    catch (Exception)
        //    {

        //        return false;
        //    }

        //}





    }


}
