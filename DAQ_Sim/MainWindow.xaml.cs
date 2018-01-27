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

using System.Data;

namespace DAQ_Sim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DAQSimulator daqSim;
        DataTable anSamplesTable;

        // Main Window function
        public MainWindow()
        {
            InitializeComponent();

            daqSim = new DAQSimulator(5);
            dataGrid.ItemsSource = daqSim.lastSample;

            doAnalogSampleUpdate();
        }

        private void doAnalogSampleUpdate()
        {
            daqSim.DoSampleAnalogueSensors();
        }

        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            doAnalogSampleUpdate();
        }

        private void menuHelpAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
