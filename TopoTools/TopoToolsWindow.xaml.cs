using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AYPluginsRevit.Utilities;

namespace AYPluginsRevit.Apps.TopoTools
{
    /// <summary>
    /// Interaction logic for TopoToolsWindow.xaml
    /// </summary>
    public partial class TopoToolsWindow : Window, IDisposable
    {
        // Variáveis necessárias para a janela principal chamar um ExternalEvent para cada comando feito pelo Handler
        private TopoToolsHandler mHandler;
        private ExternalEvent mExEvent;

        // Variáveis de valores da janela
        public ObservableCollection<ElementGroup> elementGroups = new ObservableCollection<ElementGroup>();
        public ObservableCollection<ElementGroup> intersections = new ObservableCollection<ElementGroup>();

        public string strAngle
        {
            get { return tb_angle.Text; }
        }
        public string strFactor
        {
            get { return tb_factor.Text; }
        }
        public string strElevation
        {
            get { return tb_terrainElevation.Text; }
        }
        public string strExtraHeight
        {
            get { return tb_extraHeight.Text; }
        }
        public string strExtraLenght
        {
            get { return tb_extraLenght.Text; }
        }
        public string strDivisions
        {
            get { return tb_divisions.Text; }
        }
        public TopoToolsWindow(ExternalEvent exEvent, TopoToolsHandler handler)
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

        // Quando carregar a janela definir as referências das variáveis
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dg_groups.ItemsSource = elementGroups;
            dg_intersections.ItemsSource = intersections;
        }

        #endregion

        #region MÉTODOS DOS BOTÕES
        /*
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.ShowDatatable;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }
        */

        private void bt_CreateExcavation_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.CreateExcavation;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }

        private void bt_selectLevel_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.SelectLevel;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }

        private void bt_selectElement_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.SelectElement;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
        private void bt_addElements_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.AddElements;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }
        private void bt_addGroup_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.AddGroup;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }

        private void bt_removeGroup_Click(object sender, RoutedEventArgs e)
        {
            ElementGroup currentGroup = dg_groups.SelectedItem as ElementGroup;

            if (currentGroup != null)
            {
                elementGroups.Remove(currentGroup);
            }
        }

        private void bt_addIntersection_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.AddIntersection;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }

        private void bt_addSpace_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.AddSpace;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }

        private void bt_addProjection_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.AddProjection;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }

        private void bt_removeIntersections_Click(object sender, RoutedEventArgs e)
        {
            ElementGroup currentGroup = dg_intersections.SelectedItem as ElementGroup;

            if (currentGroup != null)
            {
                intersections.Remove(currentGroup);
            }
        }
        private void bt_CreateDifference_Click(object sender, RoutedEventArgs e)
        {
            mHandler.TopoToolsRequest = TopoToolsRequest.CreateDifference;
            mHandler.TopoToolsWindow = this;
            mExEvent.Raise();
        }

        #endregion

        #region MÉTODOS DAS CAIXAS DE TEXTO

        private void tb_factor_TextChanged(object sender, TextChangedEventArgs e)
        {
            string result = "";
            char[] validChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '.' }; // Caracteres que a TextBox irá aceitar
            foreach (char c in tb_factor.Text) // Checa os caracteres que o usuário insere
            {
                if (Array.IndexOf(validChars, c) != -1) // Checa se o caractere inserido faz parte do Array, índice de -1 é que não faz parte
                {
                    char cc = c;

                    // Altera ponto para vírgula
                    if (c == '.')
                    {
                        cc = ',';
                    }

                    result += cc;
                }
            }

            tb_factor.Text = result; // Altera o texto da TextBox para a versão sem os caracteres ilegais

            // Verifica se já existe uma vírgula no texto
            if (tb_factor.Text.Count(x => x == ',') > 1)
            {
                int firstComma = tb_factor.Text.LastIndexOf(',');
                if (firstComma != -1)
                {
                    string newResult = tb_factor.Text.Remove(firstComma, 1);
                    result = newResult;
                }
            }

            tb_factor.Text = result; // Altera o texto da TextBox para a versão sem os caracteres ilegais

            tb_factor.Select(tb_factor.Text.Length, 0); // Move a posição do mouse para o final do texto
        }

        private void tb_factor_LostFocus(object sender, RoutedEventArgs e)
        {
            // Pega o valor presente na TextBox
            double factor = double.Parse(strFactor.Replace(',', '.'), CultureInfo.InvariantCulture);
            double angle = (180 / Math.PI) * Math.Atan(factor / 1);

            // Define o valor da outra TextBox
            tb_angle.Text = Math.Round(angle, 2).ToString();
        }

        private void tb_extraHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            string result = "";
            char[] validChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '.' }; // Caracteres que a TextBox irá aceitar
            foreach (char c in tb_extraHeight.Text) // Checa os caracteres que o usuário insere
            {
                if (Array.IndexOf(validChars, c) != -1) // Checa se o caractere inserido faz parte do Array, índice de -1 é que não faz parte
                {
                    char cc = c;

                    // Altera ponto para vírgula
                    if (c == '.')
                    {
                        cc = ',';
                    }

                    result += cc;
                }
            }

            tb_extraHeight.Text = result; // Altera o texto da TextBox para a versão sem os caracteres ilegais

            // Verifica se já existe uma vírgula no texto
            if (tb_extraHeight.Text.Count(x => x == ',') > 1)
            {
                int firstComma = tb_extraHeight.Text.LastIndexOf(',');
                if (firstComma != -1)
                {
                    string newResult = tb_extraHeight.Text.Remove(firstComma, 1);
                    result = newResult;
                }
            }

            tb_extraHeight.Text = result; // Altera o texto da TextBox para a versão sem os caracteres ilegais

            tb_extraHeight.Select(tb_extraHeight.Text.Length, 0); // Move a posição do mouse para o final do texto
        }

        private void tb_extraLenght_TextChanged(object sender, TextChangedEventArgs e)
        {
            string result = "";
            char[] validChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '.' }; // Caracteres que a TextBox irá aceitar
            foreach (char c in tb_extraLenght.Text) // Checa os caracteres que o usuário insere
            {
                if (Array.IndexOf(validChars, c) != -1) // Checa se o caractere inserido faz parte do Array, índice de -1 é que não faz parte
                {
                    char cc = c;

                    // Altera ponto para vírgula
                    if (c == '.')
                    {
                        cc = ',';
                    }

                    result += cc;
                }
            }

            tb_extraLenght.Text = result; // Altera o texto da TextBox para a versão sem os caracteres ilegais

            // Verifica se já existe uma vírgula no texto
            if (tb_extraLenght.Text.Count(x => x == ',') > 1)
            {
                int firstComma = tb_extraLenght.Text.LastIndexOf(',');
                if (firstComma != -1)
                {
                    string newResult = tb_extraLenght.Text.Remove(firstComma, 1);
                    result = newResult;
                }
            }

            tb_extraLenght.Text = result; // Altera o texto da TextBox para a versão sem os caracteres ilegais

            tb_extraLenght.Select(tb_extraLenght.Text.Length, 0); // Move a posição do mouse para o final do texto
        }






        #endregion


    }
    public class ElementGroup : ICloneable
    {
        public string Tipo { get; set; }
        public List<Element> Elements { get; set; }
        public int Quantity { get; set; }

        // Constructors
        public ElementGroup()
        {
        }
        public ElementGroup(string tipo, List<Element> elements, int quantity)
        {
            Tipo = tipo;
            Elements = elements;
            Quantity = quantity;
        }

        // Método CLONE
        public object Clone()
        {
            List<Element> nElements = new List<Element>();
            foreach (Element element in Elements)
            {
                nElements.Add(element);
            }
            return new ElementGroup(Tipo, nElements, Quantity);
        }
    }
}
