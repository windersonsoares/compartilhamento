using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using AYPluginsRevit.Commands.RoomUtility;
using AYPluginsRevit.Requests;
using Autodesk.Revit.UI.Events;
using AYPluginsRevit.Apps.WallTools;

namespace AYPluginsRevit
{
    class ExternalApplication : IExternalApplication
    {
        // Instância a classe, inicialmente null pois ainda não foi definida
        static internal ExternalApplication thisApp = null;

        #region INSTÂNCIAS DAS JANELAS

        // Instância das janelas (modeless dialog)
        private Apps.WallTools.WallToolsWindow WallToolsWindow;
        
        #endregion
        
        public Result OnShutdown(UIControlledApplication application)
        {
            // Importante: sempre utilizar o método Dispose no Shutdown para fechar a janela e qualquer outros controles e instâncias iniciadas pela aplicação
            #region DISPOSE DAS JANELAS


            if (WallToolsWindow != null && WallToolsWindow.IsVisible)
            {
                WallToolsWindow.Dispose();
            }

            #endregion

            return Result.Succeeded;
        }
        
        public Result OnStartup(UIControlledApplication application)
        {
            // Pega o endereço da dll
            string path = Assembly.GetExecutingAssembly().Location;

            // Imagem padrão Autodesk
            BitmapImage autodeskImage = new BitmapImage(new Uri("pack://application:,,,/AYPluginsRevit;component/Resources/RibbonImages/autodesk_logo32.png"));

            #region CRIAÇÃO DE ABAS

            // Cria a aba
            application.CreateRibbonTab("AYOrc");
            //application.CreateRibbonTab("AYOrcQ");

            #endregion

            #region CRIAÇÃO DOS PAINÉIS

            // Cria os painéis
            RibbonPanel panelCreate = application.CreateRibbonPanel("AYOrc", "Criar");

            #endregion

            #region BOTÕES DO PAINEL CRIAÇÃO

			PushButtonData wallTools = new PushButtonData("WallTools", "Wall\r\nTools", path, "AYPluginsRevit.Apps.WallTools.WallToolsCommand");
			PushButton btPartTools = panelCreate.AddItem(wallTools) as PushButton;
			
			#endregion
			
            // Substitiu o null pela aplicação já que agora está definida
            thisApp = this;

            return Result.Succeeded;
        }

        #region JANELAS - MÉTODOS QUE SERÃO CHAMADOS PELAS CLASSES COMMAND DAS JANELAS

        #region Janela WallTools

        // Mostra a janela ao clicar no botão a partir do comando
        public void ShowWallToolsWindow(UIApplication uiapp)
        {
            // Mostra a janela
            if (WallToolsWindow == null || WallToolsWindow.IsLoaded == false)
            {
                // Cria o handler e o evento externo
                Apps.WallTools.WallToolsHandler handler = new Apps.WallTools.WallToolsHandler();
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // Inicia a janela com as instâncias acima como argumentos
                WallToolsWindow = new Apps.WallTools.WallToolsWindow(exEvent, handler);
                WallToolsWindow.Show();
            }
        }

        // Ativa a janela após algum comando ser executado por ela
        public void WakeUpWallToolsWindow()
        {
            if (WallToolsWindow != null)
            {
                WallToolsWindow.WakeUp();
                WallToolsWindow.Activate();
            }
        }

        #endregion

    }
}