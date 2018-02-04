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
using CamHelperFunctions;
using System.Collections.Specialized;
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
        int logEntryCount;

        int aiFilterLen;

        char logDelim;

        // Main Window function
        public MainWindow()
        {
            InitializeComponent();

            // Initialize timers
            timeUpdater = new DispatcherTimer();
            timeUpdater.Interval = Config.TimeSecondsKey("sysTimeUpdateInterval", 0.5F);
            timeUpdater.Tick += new EventHandler(TimeUpdater_Elapsed);
            timeUpdater.Start();

            samplingTimer = new ActionWaiter("SampleTimer", Config.TimeSecondsKey("sampleTime", 1.0F));
            samplingTimer.Ready += new EventHandler(SamplingWaiter_Elapsed);

            loggingTimer = new ActionWaiter("LoggingTimer", Config.TimeSecondsKey("loggingTime", 1.0F));
            loggingTimer.Ready += new EventHandler(LoggingTimer_Elapsed);

            // Initialize DAQ simulator and filtering objects
            daqSim = new DAQSimulator();
            aiFilters = new MAFilter[daqSim.AIDevCount];

            for( int i=0; i < aiFilters.Length; i++ )
            {
                string name = "aiFilter_" + i.ToString("G2");
                aiFilters[i] = new MAFilter(name, aiFilterLen);
            }
            
            dgAnalogueSamples.ItemsSource = daqSim.ai;
            dgDigitalSamples.ItemsSource = daqSim.di;

            // Setup data logging
            LogInitialize();

            // Window default state
            btnSample.IsEnabled = true;
            btnLog.IsEnabled = true;
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

            tbLastSampleTime.Text = timeNow.ToString("HH:mm:ss.f");
            tbNextSampleTime.Text = timeNow.Add(sampleUpdatePeriod).ToString("HH:mm:ss.f");

            btnSample.IsEnabled = false;
            daqSim.DoSampleSensors();

            for (int i = 0; i < aiFilters.Length; i++)
            {
                aiFilters[i].AddValue(daqSim.ai[i].SensValue);
            }

            samplingTimer.Go();
        }

        private void LogInitialize()
        {
            logToFile = new DataLog(Config.Charkey("dataLogDelim", ','));
            tbLogPath.Text = logToFile.FilePath;

            logEntryCount = 0;
            tbLogEntryCount.Text = logEntryCount.ToString();

            logToFile.BufferEntry("Timestamp");

            foreach (Sensor s in daqSim.ai)
                logToFile.BufferEntry(s.name);

            foreach (Sensor s in daqSim.di)
                logToFile.BufferEntry(s.name);

            logToFile.WriteEntry(tStamp: false, incrCtr: false);
        }

        private void PerformLogWrite()
        {
            DateTime timeNow = DateTime.Now;

            tbLastLogTime.Text = timeNow.ToString("HH:mm:ss.f");
            tbNextLogTime.Text = timeNow.Add(logUpdatePeriod).ToString("HH:mm:ss.f");

            btnLog.IsEnabled = false;

            for (int i = 0; i < aiFilters.Length; i++)
                logToFile.BufferEntry(aiFilters[i].output.ToString("F3"));

            foreach (Sensor s in daqSim.di)
                logToFile.BufferEntry(s.valStr);

            if( logToFile.WriteEntry() )
            {
                logEntryCount++;
                tbLogEntryCount.Text = logEntryCount.ToString();
                loggingTimer.Go();
            } else
            {
                tbLogEntryCount.Text = logEntryCount.ToString() + " !--ERR--!";
            }
        }

        private void TimeUpdater_Elapsed(object sender, EventArgs e)
        {
            tbTimeNow.Text = DateTime.Now.ToString("HH:mm:ss.f");
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
