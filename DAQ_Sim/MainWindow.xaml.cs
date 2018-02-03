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
using DataLogging;
using System.Data;

namespace DAQ_Sim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DAQSimulator daqSim;
        MAFilter[] aiFilters;

        TimeSpan sampleUpdatePeriod;
        DispatcherTimer timeUpdater;
        ActionWaiter samplingTimer;

        TimeSpan logUpdatePeriod;
        ActionWaiter loggingTimer;

        DataLog logToFile;

        int numAiSensors = 2;
        int numDiSensors = 5;
        int aiFilterLen = 3;

        // Main Window function
        public MainWindow()
        {
            InitializeComponent();

            daqSim = new DAQSimulator(numAiSensors, numDiSensors);
            aiFilters = new MAFilter[numAiSensors];

            for( int i=0; i<numAiSensors; i++ )
            {
                string name = "aiFilter_" + i.ToString("G2");
                aiFilters[i] = new MAFilter(name, aiFilterLen);
            }

            logToFile = new DataLog(',');

            sampleUpdatePeriod = new TimeSpan(0, 0, 0, 5, 700);
            logUpdatePeriod = new TimeSpan(0, 0, 0, 10, 0);

            timeUpdater = new DispatcherTimer();
            timeUpdater.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timeUpdater.Tick += new EventHandler(TimeUpdater_Elapsed);

            tbTimeNow.Text = DateTime.Now.ToString("hh:mm:ss.f");
            
            timeUpdater.Start();

            samplingTimer = new ActionWaiter(sampleUpdatePeriod);
            samplingTimer.Ready += new EventHandler(SamplingWaiter_Elapsed);
            samplingTimer.Go();

            loggingTimer = new ActionWaiter(logUpdatePeriod);
            loggingTimer.Ready += new EventHandler(LoggingTimer_Elapsed);
            loggingTimer.Go();

            dgAnalogueSamples.ItemsSource = daqSim.analogueSensors;
            dgDigitalSamples.ItemsSource = daqSim.digitalSensors;

            PerformSample();
            PerformLogWrite();
        }
        
        private void SamplingWaiter_Elapsed(object sender, EventArgs e)
        {
            EnableControl(btnSample);
        }

        private void LoggingTimer_Elapsed(object sender, EventArgs e)
        {
            EnableControl(btnLog);
        }

        public delegate void DelegateEnableControl(Control ctl);
        private void EnableControl(Control ctl)
        {
            if( Dispatcher.CheckAccess() )
            {
                ctl.IsEnabled = true;
            } else
            {
                Dispatcher.Invoke(new DelegateEnableControl(EnableControl),ctl);
            }           
        }

        private void PerformSample()
        {
            DateTime timeNow = DateTime.Now;

            tbLastSampleTime.Text = timeNow.ToString("hh:mm:ss.f");
            tbNextSampleTime.Text = timeNow.Add(sampleUpdatePeriod).ToString("hh:mm:ss.f");

            btnSample.IsEnabled = false;
            daqSim.DoSampleAnalogueSensors();

            for (int i = 0; i < numAiSensors; i++)
            {
                aiFilters[i].AddValue(daqSim.analogueSensors[i].value);
            }

            samplingTimer.Go();
        }

        private void PerformLogWrite()
        {
            DateTime timeNow = DateTime.Now;

            tbLastLogTime.Text = timeNow.ToString("hh:mm:ss.f");
            tbNextLogTime.Text = timeNow.Add(logUpdatePeriod).ToString("hh:mm:ss.f");

            btnLog.IsEnabled = false;

            for (int i = 0; i < numAiSensors; i++)
            {
                logToFile.BufferEntry(aiFilters[i].output.ToString("F3"));
            }

            //foreach (Sensor s in daqSim.analogueSensors)
            //    logToFile.BufferEntry(s.valStr);

            foreach (Sensor s in daqSim.digitalSensors)
                logToFile.BufferEntry(s.valStr);

            if( logToFile.WriteEntry() )
                // update number entries
                // else
                // indicate error
            
            loggingTimer.Go();
        }

        private void TimeUpdater_Elapsed(object sender, EventArgs e)
        {
            tbTimeNow.Text = DateTime.Now.ToString("hh:mm:ss.f");
        }

        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            PerformSample();
        }

        private void btnLog_Click(object sender, RoutedEventArgs e)
        {
            PerformLogWrite();
        }

        private void menuHelpAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            samplingTimer.Stop();
            loggingTimer.Stop();
        }
    }
}
