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
using System.Windows.Threading;

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
        DispatcherTimer timeUpdater;

        ActionWaiter myTimer;

        // Main Window function
        public MainWindow()
        {
            InitializeComponent();

            daqSim = new DAQSimulator(5);

            sampleUpdatePeriod = new TimeSpan(0, 0, 0, 5, 700);

            timeUpdater = new DispatcherTimer();
            timeUpdater.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timeUpdater.Tick += new EventHandler(TimeUpdater_Elapsed);

            tbTimeNow.Text = DateTime.Now.ToString("hh:mm:ss.f");
            
            timeUpdater.Start();

            myTimer = new ActionWaiter(sampleUpdatePeriod);
            myTimer.Ready += new EventHandler(SamplingWaiter_Elapsed);
            myTimer.Go();

            dataGrid.ItemsSource = daqSim.analogueSensors;

            PerformSample();
        }
        
        private void SamplingWaiter_Elapsed(object sender, EventArgs e)
        {
            EnableSampleButton();
        }

        public delegate void DelegateEnableSampleButton();
        private void EnableSampleButton()
        {
            if( Dispatcher.CheckAccess() )
            {
                btnSample.IsEnabled = true;
            } else
            {
                Dispatcher.Invoke(new DelegateEnableSampleButton(EnableSampleButton),null);
            }           
        }

        private void PerformSample()
        {
            DateTime timeNow = DateTime.Now;

            tbLastSampleTime.Text = timeNow.ToString("hh:mm:ss.f");
            tbNextSampleTime.Text = timeNow.Add(sampleUpdatePeriod).ToString("hh:mm:ss.f");

            btnSample.IsEnabled = false;
            daqSim.DoSampleAnalogueSensors();

            // samplingWaiter.Start();
            myTimer.Go();
        }

        private void TimeUpdater_Elapsed(object sender, EventArgs e)
        {
            tbTimeNow.Text = DateTime.Now.ToString("hh:mm:ss.f");
        }

        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            PerformSample();
        }

        private void menuHelpAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            myTimer.Stop();
        }
    }
}
