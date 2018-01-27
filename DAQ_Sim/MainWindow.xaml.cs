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
using System.Timers;

using System.Data;

namespace DAQ_Sim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DAQSimulator daqSim;
        TimeSpan sampleUpdatePeriod;
        Timer timeUpdater;
        Timer samplingWaiter;

        // Main Window function
        public MainWindow()
        {
            InitializeComponent();

            daqSim = new DAQSimulator(5);

            sampleUpdatePeriod = new TimeSpan(0, 0, 5);
            samplingWaiter = new Timer(sampleUpdatePeriod.TotalMilliseconds);
            samplingWaiter.Elapsed += SamplingWaiter_Elapsed;

            timeUpdater = new Timer(1000);
            timeUpdater.Elapsed += TimeUpdater_Elapsed;

            tbTimeNow.Text = DateTime.Now.ToString("hh:mm:ss.f");

            //timeUpdater.Start();

            dataGrid.ItemsSource = daqSim.analogueSensors;

            btnSample.IsEnabled = true;
        }

        private void SamplingWaiter_Elapsed(object sender, ElapsedEventArgs e)
        {
            btnSample.IsEnabled = true;
        }

        private void TimeUpdater_Elapsed(object sender, ElapsedEventArgs e)
        {
            tbTimeNow.Text = DateTime.Now.ToString("hh:mm:ss.f");
        }

        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            DateTime timeNow = DateTime.Now;

            tbLastSampleTime.Text = timeNow.ToString("hh:mm:ss.f");
            tbNextSampleTime.Text = timeNow.Add(sampleUpdatePeriod).ToString("hh:mm:ss.f");

            btnSample.IsEnabled = false;
            daqSim.DoSampleAnalogueSensors();

            samplingWaiter.Start();
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
