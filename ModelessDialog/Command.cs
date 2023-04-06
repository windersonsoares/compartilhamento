using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AYPluginsRevit.ModelessDialog
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region DADOS DO REVIT

            // Pega o UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Pega o Document
            Document doc = uidoc.Document;

            // Pega a UIApplication
            UIApplication uiapp = new UIApplication(doc.Application);

            // Pega a Application
            Application app = uiapp.Application;

            #endregion

            try
            {
                ExternalApplication.thisApp.ShowWallToolsWindow(commandData.Application);
                TaskDialog.Show("Teste", "Teste");

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed; ;
            }
        }
    }
}
