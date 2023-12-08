using Dlubal.RFEM5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using rf5=Dlubal.RFEM5;

namespace RFEM5ToRFEM6Transverter.RFEM5
{

   

    public class RFEM5ConnectionHandler
    {


        static IApplication application = null;
        static IModel model = null;
        static IView view = null;

        public static IModel ConnectRFEM5(String filePath) {

            try
            {
                // Creates new RFEM5 instance and gets it's interface.
                application = new Dlubal.RFEM5.Application();

                // Shows RFEM GUI.
                application.Show();

                if (filePath != "") { model = application.OpenModel(filePath); }
                else
                {// Creates a new model and gets it's interface.
                    model = application.CreateModel("MyTestModel");
                }

                //SetStructuralData(model);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //labelInfo.Text = "Ready";

                // View All
                view = model.GetActiveView();
                view.ShowAll();
                view = null;

                // Releases RFEM model interface.
                model = null;

                // Unlocks licence and releases RFEM application interface.
                if (application != null)
                {
                    application.UnlockLicense();
                    application = null;
                }

                // Cleans Garbage Collector and releases all cached COM interfaces and objects.
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }


            return model;
        }



    }
}
