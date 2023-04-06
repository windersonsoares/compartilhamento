using Autodesk.Revit.UI;
using AYPluginsRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AYPluginsRevit.Apps.WallsFromDWG
{
    /// <summary>
    /// Interaction logic for WallsFromDWGWindow.xaml
    /// </summary>
    public partial class WallsFromDWGWindow : Window, IDisposable
    {
        // Variáveis necessárias para a janela principal chamar um ExternalEvent para cada comando feito pelo Handler
        private WallsFromDWGRequestHandler mHandler;
        private ExternalEvent mExEvent;

        public WallsFromDWGWindow(ExternalEvent exEvent, WallsFromDWGRequestHandler handler)
        {
            // Define o Revit como pai da janela para sempre ficar sobre ele
            this.SetRevitAsParent();
            this.HideMinimizeMaximizeButton();

            InitializeComponent();

            // Define as variáveis
            mHandler = handler;
            mExEvent = exEvent;
        }

        #region MÉTODOS AUXILIARES DA JANELA

        // Método chamado pelo método OnShutDown no ExternalApplication para fechar corretamente a applicação e as instâncias do handler e ExternalEvent
        public void Dispose()
        {
            mExEvent.Dispose();
            mExEvent = null;
            mHandler = null;
            this.Close();
        }

        // Ativa os controles da janela após o uso caso necessário
        public void WakeUp()
        {
            this.IsEnabled = true;
        }

        // Desativa os controles da janela caso nécessário
        public void DozeOff()
        {
            this.IsEnabled = false;
        }

        #endregion

        #region MÉTODOS

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            mHandler.WallsFromDWGRequestType = WallsFromDWGRequestType.NomeDoComando;
            mHandler.WallsFromDWGWindow = this;
            mExEvent.Raise();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Dispose();
        }


        #endregion
    }
}
