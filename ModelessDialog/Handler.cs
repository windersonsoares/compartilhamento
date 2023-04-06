using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
// Adicionados
using AYPluginsRevit.ModelessDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AYPluginsRevit.ModelessDialog
{
    public class Handler : IExternalEventHandler
    {
        // Instâncias do Request e da Janela
        public RequestType RequestType { get; set; }
        public WPFWindow WPFWindow { get; set; }

        public void Execute(UIApplication app)
        {
            switch (RequestType)
            {
                case RequestType.NomeDoComando:
                    NomeDoComando(app.ActiveUIDocument);
                    break;
                default:
                    break;
            }
        }

        public string GetName()
        {
            return "Winderson";
        }

        #region COMANDOS

        private void NomeDoComando(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;



        }

        #endregion
    }
}
